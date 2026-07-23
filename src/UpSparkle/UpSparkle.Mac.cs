using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle;

internal sealed class MacUpSparkleImplementation : IUpSparklePlatformImplementation
{
    private const string LibObjc = "/usr/lib/libobjc.A.dylib";

    private static readonly string[] SparkleLibraryPaths =
    [
        Path.Combine(AppContext.BaseDirectory, "runtimes", "osx", "native", "Sparkle.framework", "Sparkle"),
        Path.Combine(AppContext.BaseDirectory, "libs", "Sparkle.framework", "Sparkle"),
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Sparkle.framework", "Sparkle"),
    ];

    // Eagerly load Sparkle so its ObjC classes register before we call objc_getClass.
    private static readonly IntPtr SparkleLibraryHandle = LoadSparkleLibrary();

    static MacUpSparkleImplementation()
    {
        NativeLibrary.SetDllImportResolver(typeof(MacUpSparkleImplementation).Assembly, ResolveLibrary);
    }

    // -------------------------------------------------------------------------
    // libobjc P/Invokes
    // -------------------------------------------------------------------------

    [DllImport(LibObjc, EntryPoint = "objc_getClass", CharSet = CharSet.Ansi)]
    private static extern IntPtr ObjcGetClass(string name);

    [DllImport(LibObjc, EntryPoint = "sel_registerName", CharSet = CharSet.Ansi)]
    private static extern IntPtr SelRegisterName(string name);

    [DllImport(LibObjc, EntryPoint = "objc_allocateClassPair")]
    private static extern IntPtr ObjcAllocateClassPair(IntPtr superclass, string name, nint extraBytes);

    [DllImport(LibObjc, EntryPoint = "objc_registerClassPair")]
    private static extern void ObjcRegisterClassPair(IntPtr cls);

    [DllImport(LibObjc, EntryPoint = "class_addMethod")]
    private static extern bool ClassAddMethod(IntPtr cls, IntPtr selector, IntPtr implementation, string types);

    [DllImport(LibObjc, EntryPoint = "object_setInstanceVariable")]
    private static extern IntPtr ObjectSetInstanceVariable(IntPtr obj, string name, IntPtr value);

    [DllImport(LibObjc, EntryPoint = "object_getInstanceVariable")]
    private static extern IntPtr ObjectGetInstanceVariable(IntPtr obj, string name, out IntPtr outValue);

    [DllImport(LibObjc, EntryPoint = "class_addIvar")]
    private static extern bool ClassAddIvar(IntPtr cls, string name, nint size, byte alignment, string types);

    // objc_msgSend: no-arg → id
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSend(IntPtr receiver, IntPtr sel);

    // objc_msgSend: bool, id, id → id  (initWithStartingUpdater:updaterDelegate:userDriverDelegate:)
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendInitCtrl(
        IntPtr receiver, IntPtr sel,
        byte startingUpdater,      // BOOL is unsigned char on arm64 / signed char on x86; byte is safe
        IntPtr updaterDelegate,
        IntPtr userDriverDelegate);

    // objc_msgSend: no-arg → void  (startUpdater, checkForUpdates)
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void MsgSendVoid(IntPtr receiver, IntPtr sel);

    // objc_msgSend: id → void  (setFeedURL: — kept for reference, not used)
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void MsgSendVoidId(IntPtr receiver, IntPtr sel, IntPtr arg);

    // objc_msgSend: const char* → id  (stringWithUTF8String:)
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendStr(IntPtr receiver, IntPtr sel, [MarshalAs(UnmanagedType.LPUTF8Str)] string str);

    // objc_msgSend: id → id  (URLWithString:, updater property, etc.)
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSendId(IntPtr receiver, IntPtr sel, IntPtr arg);

    [DllImport(LibObjc, EntryPoint = "objc_retain")]
    private static extern IntPtr ObjcRetain(IntPtr obj);

    [DllImport(LibObjc, EntryPoint = "objc_release")]
    private static extern void ObjcRelease(IntPtr obj);

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private IntPtr updaterController = IntPtr.Zero;

    // GCHandle keeps the delegate callback alive so the GC can't collect it.
    private GCHandle delegateCallbackHandle;

    // -------------------------------------------------------------------------
    // Library loading / DllImport resolver
    // -------------------------------------------------------------------------

    private static IntPtr ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, "Sparkle", StringComparison.Ordinal))
            return IntPtr.Zero;

        foreach (var candidate in SparkleLibraryPaths.Where(File.Exists))
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
                return handle;
        }

        return IntPtr.Zero;
    }

    private static IntPtr LoadSparkleLibrary()
    {
        foreach (var candidate in SparkleLibraryPaths.Where(File.Exists))
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
                return handle;
        }

        throw new DllNotFoundException(
            $"Unable to locate Sparkle.framework. Searched:{Environment.NewLine}" +
            string.Join(Environment.NewLine, SparkleLibraryPaths));
    }

    // -------------------------------------------------------------------------
    // ObjC helpers
    // -------------------------------------------------------------------------

    private static IntPtr Sel(string name) => SelRegisterName(name);

    private static IntPtr NSString(string value) =>
        MsgSendStr(ObjcGetClass("NSString"), Sel("stringWithUTF8String:"), value);

    // -------------------------------------------------------------------------
    // Delegate class — provides feedURLStringForUpdater: at runtime
    //
    // We synthesise a minimal NSObject subclass that implements the one
    // SPUUpdaterDelegate method we need.  The feed URL string is stored
    // in an ivar so the callback can read it without a static field.
    // -------------------------------------------------------------------------

    // Unmanaged callback signature: (id self, SEL _cmd, id updater) -> id
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr FeedUrlCallback(IntPtr self, IntPtr sel, IntPtr updater);

    private static readonly string DelegateClassName = "UpSparkleDelegateImpl_" + Guid.NewGuid().ToString("N");

    // Register the delegate class once.
    private static IntPtr RegisterDelegateClass()
    {
        var nsObjectClass = ObjcGetClass("NSObject");
        var cls = ObjcAllocateClassPair(nsObjectClass, DelegateClassName, 0);
        if (cls == IntPtr.Zero)
            throw new InvalidOperationException("Failed to allocate ObjC delegate class.");

        // Add an ivar to hold a GCHandle pointer (IntPtr.Size bytes, pointer alignment).
        byte ptrLog2 = (byte)(IntPtr.Size == 8 ? 3 : 2);
        ClassAddIvar(cls, "_feedUrlPtr", (nint)IntPtr.Size, ptrLog2, "^v");

        ObjcRegisterClassPair(cls);
        return cls;
    }

    private static readonly IntPtr DelegateClass = RegisterDelegateClass();

    // feedURLStringForUpdater: — returns an NSString of the feed URL
    private static IntPtr FeedUrlForUpdater(IntPtr self, IntPtr sel, IntPtr updater)
    {
        // Read the feed URL string pointer we stored in the ivar.
        ObjectGetInstanceVariable(self, "_feedUrlPtr", out var ptr);
        if (ptr == IntPtr.Zero) return IntPtr.Zero;

        var url = Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
        return NSString(url);
    }

    // -------------------------------------------------------------------------
    // IUpSparklePlatformImplementation
    // -------------------------------------------------------------------------

    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        // 1. Create a delegate instance that implements feedURLStringForUpdater:
        //    so Sparkle picks up our runtime feed URL without using the deprecated
        //    setFeedURL: / NSUserDefaults path.
        var feedUrlCallback = new FeedUrlCallback(FeedUrlForUpdater);
        delegateCallbackHandle = GCHandle.Alloc(feedUrlCallback);

        // Add the method to the class (safe to call multiple times; subsequent
        // calls for the same selector are no-ops in the ObjC runtime).
        var feedSelName = "feedURLStringForUpdater:";
        // ObjC type encoding: id (^@:@) = return id, self id, sel SEL, arg id
        ClassAddMethod(DelegateClass, Sel(feedSelName),
            Marshal.GetFunctionPointerForDelegate(feedUrlCallback), "@@:@");

        // Alloc + init the delegate object
        var delegateObj = MsgSend(MsgSend(DelegateClass, Sel("alloc")), Sel("init"));
        if (delegateObj == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create updater delegate object.");

        // Store the UTF-8 feed URL string in the ivar.
        // We pin the managed string as a native UTF-8 buffer.
        var urlBytes = System.Text.Encoding.UTF8.GetBytes(appCastUrl + "\0");
        var urlHandle = GCHandle.Alloc(urlBytes, GCHandleType.Pinned);
        ObjectSetInstanceVariable(delegateObj, "_feedUrlPtr", urlHandle.AddrOfPinnedObject());
        // urlHandle is intentionally kept alive for the lifetime of this object;
        // store it so we can free it on Dispose.
        if (delegateCallbackHandle.IsAllocated)
            delegateCallbackHandle.Free();
        delegateCallbackHandle = urlHandle;

        var delegateRef = ObjcRetain(delegateObj);

        // 2. Create SPUStandardUpdaterController with startingUpdater:NO so we
        //    can pass the delegate before the first validation pass runs.
        var ctrlClass = ObjcGetClass("SPUStandardUpdaterController");
        if (ctrlClass == IntPtr.Zero)
            throw new InvalidOperationException(
                "SPUStandardUpdaterController not found. Ensure Sparkle 2.x is loaded.");

        var alloc = MsgSend(ctrlClass, Sel("alloc"));
        // - (instancetype)initWithStartingUpdater:(BOOL)startingUpdater
        //                        updaterDelegate:(id<SPUUpdaterDelegate>)updaterDelegate
        //                    userDriverDelegate:(id<SPUUserDriverDelegate>)userDriverDelegate
        var ctrl = MsgSendInitCtrl(alloc,
            Sel("initWithStartingUpdater:updaterDelegate:userDriverDelegate:"),
            0,            // NO — we start manually after config
            delegateRef,
            IntPtr.Zero);

        if (ctrl == IntPtr.Zero)
            throw new InvalidOperationException("Failed to initialize SPUStandardUpdaterController.");

        updaterController = ObjcRetain(ctrl);

        // 3. Start the updater.
        // -[SPUStandardUpdaterController startUpdater] — void, no args.
        // This calls -[SPUUpdater startUpdater:(NSError**)error] internally and
        // shows an alert to the user on misconfiguration (rather than throwing).
        MsgSendVoid(updaterController, Sel("startUpdater"));
    }

    public void CheckUpdateWithUI()
    {
        if (updaterController == IntPtr.Zero)
            throw new InvalidOperationException("Sparkle updater is not initialized.");

        // Get SPUUpdater from the controller and call -checkForUpdates (no args).
        // -[SPUUpdater checkForUpdates] is the user-initiated, UI-showing variant.
        var updater = MsgSend(updaterController, Sel("updater"));
        if (updater == IntPtr.Zero)
            throw new InvalidOperationException("Could not retrieve SPUUpdater from controller.");

        MsgSendVoid(updater, Sel("checkForUpdates"));
    }

    public void Dispose()
    {
        if (updaterController != IntPtr.Zero)
        {
            ObjcRelease(updaterController);
            updaterController = IntPtr.Zero;
        }

        if (delegateCallbackHandle.IsAllocated)
            delegateCallbackHandle.Free();
    }
}
