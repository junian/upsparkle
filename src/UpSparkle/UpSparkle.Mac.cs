using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle;

internal sealed class MacUpSparkleImplementation : IUpSparklePlatformImplementation
{
    private const string LibObjc = "/usr/lib/libobjc.A.dylib";

    private static readonly string[] SparkleLibraryPaths =
    [
        Path.Combine(AppContext.BaseDirectory, "runtimes", "osx", "native", "Sparkle.framework", "Sparkle"),
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Sparkle.framework", "Sparkle"),
    ];

    // Eagerly load Sparkle so its classes are registered with the ObjC runtime before we call objc_getClass.
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

    // objc_msgSend variants — each distinct call-site signature needs its own import.

    // alloc / init (no args, returns id)
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector);

    // initWithStartingUpdater:updaterDelegate:userDriverDelegate: (bool, id, id) -> id
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSendInit(
        IntPtr receiver, IntPtr selector,
        [MarshalAs(UnmanagedType.I1)] bool startingUpdater,
        IntPtr updaterDelegate,
        IntPtr userDriverDelegate);

    // setFeedURL: (NSURL*) -> void
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void ObjcMsgSendSetUrl(IntPtr receiver, IntPtr selector, IntPtr url);

    // checkForUpdates: (id sender) -> void
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void ObjcMsgSendCheckForUpdates(IntPtr receiver, IntPtr selector, IntPtr sender);

    // URLWithString: (NSString*) -> id
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSendWithArg(IntPtr receiver, IntPtr selector, IntPtr arg);

    // stringWithUTF8String: (const char*) -> id
    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSendWithStr(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.LPUTF8Str)] string str);

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    // The SPUStandardUpdaterController instance. We hold a strong ref via
    // CFRetain so ARC (if any) does not collect it between calls.
    private IntPtr updaterController = IntPtr.Zero;

    // -------------------------------------------------------------------------
    // Library loading
    // -------------------------------------------------------------------------

    private static IntPtr ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // We only handle Sparkle here; everything else falls through to the default resolver.
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
    // Helpers
    // -------------------------------------------------------------------------

    private static IntPtr Sel(string name) => SelRegisterName(name);

    private static IntPtr MakeNSString(string value)
    {
        var cls = ObjcGetClass("NSString");
        return ObjcMsgSendWithStr(cls, Sel("stringWithUTF8String:"), value);
    }

    private static IntPtr MakeNSURL(string url)
    {
        var nsString = MakeNSString(url);
        var cls = ObjcGetClass("NSURL");
        var nsUrl = ObjcMsgSendWithArg(cls, Sel("URLWithString:"), nsString);
        if (nsUrl == IntPtr.Zero)
            throw new ArgumentException($"'{url}' is not a valid URL.", nameof(url));
        return nsUrl;
    }

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "CFRetain")]
    private static extern IntPtr CFRetain(IntPtr cf);

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "CFRelease")]
    private static extern void CFRelease(IntPtr cf);

    // -------------------------------------------------------------------------
    // IUpSparklePlatformImplementation
    // -------------------------------------------------------------------------

    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        // Allocate + init SPUStandardUpdaterController programmatically.
        // Equivalent ObjC:
        //   SPUStandardUpdaterController *ctrl =
        //       [[SPUStandardUpdaterController alloc]
        //            initWithStartingUpdater:YES
        //                   updaterDelegate:nil
        //               userDriverDelegate:nil];
        var cls  = ObjcGetClass("SPUStandardUpdaterController");
        if (cls == IntPtr.Zero)
            throw new InvalidOperationException(
                "SPUStandardUpdaterController class not found. " +
                "Ensure Sparkle.framework is loaded and is version 2.x.");

        var alloc = ObjcMsgSend(cls, Sel("alloc"));
        if (alloc == IntPtr.Zero)
            throw new InvalidOperationException("Failed to allocate SPUStandardUpdaterController.");

        var ctrl = ObjcMsgSendInit(
            alloc,
            Sel("initWithStartingUpdater:updaterDelegate:userDriverDelegate:"),
            startingUpdater: true,
            updaterDelegate: IntPtr.Zero,
            userDriverDelegate: IntPtr.Zero);

        if (ctrl == IntPtr.Zero)
            throw new InvalidOperationException("Failed to initialize SPUStandardUpdaterController.");

        // Retain so the object survives across managed/unmanaged boundary.
        updaterController = CFRetain(ctrl);

        // Get the SPUUpdater from the controller and set the feed URL.
        // Equivalent ObjC: [ctrl.updater setFeedURL:[NSURL URLWithString:appCastUrl]];
        var updater = ObjcMsgSend(updaterController, Sel("updater"));
        if (updater == IntPtr.Zero)
            throw new InvalidOperationException("SPUUpdater could not be retrieved from controller.");

        ObjcMsgSendSetUrl(updater, Sel("setFeedURL:"), MakeNSURL(appCastUrl));
    }

    public void CheckUpdateWithUI()
    {
        if (updaterController == IntPtr.Zero)
            throw new InvalidOperationException("Sparkle updater is not initialized.");

        // -[SPUStandardUpdaterController checkForUpdates:(id)sender]
        ObjcMsgSendCheckForUpdates(updaterController, Sel("checkForUpdates:"), IntPtr.Zero);
    }

    public void Dispose()
    {
        if (updaterController != IntPtr.Zero)
        {
            CFRelease(updaterController);
            updaterController = IntPtr.Zero;
        }
    }
}
