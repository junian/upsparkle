using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle;

public sealed class MacSparkle : IUpSparkle
{
    private const string LibObjc = "/usr/lib/libobjc.A.dylib";
    private static readonly string[] SparkleLibraryPaths =
    [
        Path.Combine(AppContext.BaseDirectory, "Sparkle.framework", "Sparkle"),
        Path.Combine(AppContext.BaseDirectory, "runtimes", "osx", "native", "Sparkle.framework", "Sparkle"),
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Sparkle.framework", "Sparkle")
    ];

    private static readonly IntPtr SparkleLibraryHandle = LoadSparkleLibrary();

    static MacSparkle()
    {
        NativeLibrary.SetDllImportResolver(typeof(MacSparkle).Assembly, ResolveLibrary);
    }

    [DllImport(LibObjc, EntryPoint = "objc_getClass", CharSet = CharSet.Ansi)]
    private static extern IntPtr ObjcGetClass(string name);

    [DllImport(LibObjc, EntryPoint = "sel_registerName", CharSet = CharSet.Ansi)]
    private static extern IntPtr SelRegisterName(string selectorName);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector, IntPtr argument);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.LPUTF8Str)] string argument);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    private static extern void ObjcMsgSendVoid(IntPtr receiver, IntPtr selector, IntPtr argument);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool ObjcMsgSendBool(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjc, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool ObjcMsgSendBool(IntPtr receiver, IntPtr selector, IntPtr argument);

    private static IntPtr ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, "Sparkle", StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        foreach (var candidate in SparkleLibraryPaths.Where(File.Exists))
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    private static IntPtr LoadSparkleLibrary()
    {
        foreach (var candidate in SparkleLibraryPaths.Where(File.Exists))
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        throw new DllNotFoundException("Unable to locate Sparkle.framework.");
    }

    private static IntPtr Class(string name) => ObjcGetClass(name);

    private static IntPtr Selector(string name) => SelRegisterName(name);

    private static IntPtr Send(IntPtr receiver, string selector) => ObjcMsgSend(receiver, Selector(selector));

    private static IntPtr Send(IntPtr receiver, string selector, IntPtr argument) =>
        ObjcMsgSend(receiver, Selector(selector), argument);

    private static IntPtr Send(IntPtr receiver, string selector, string argument) =>
        ObjcMsgSend(receiver, Selector(selector), argument);

    private static void SendVoid(IntPtr receiver, string selector, IntPtr argument) =>
        ObjcMsgSendVoid(receiver, Selector(selector), argument);

    private static bool SendBool(IntPtr receiver, string selector) =>
        ObjcMsgSendBool(receiver, Selector(selector));

    private static bool SendBool(IntPtr receiver, string selector, IntPtr argument) =>
        ObjcMsgSendBool(receiver, Selector(selector), argument);

    private static IntPtr CreateMainBundle()
    {
        var nsBundleClass = Class("NSBundle");
        return Send(nsBundleClass, "mainBundle");
    }

    private static IntPtr CreateString(string value)
    {
        var nsStringClass = Class("NSString");
        return Send(nsStringClass, "stringWithUTF8String:", value);
    }

    private static IntPtr CreateUrl(string value)
    {
        var nsUrlClass = Class("NSURL");
        var nsString = CreateString(value);
        var nsUrl = Send(nsUrlClass, "URLWithString:", nsString);

        if (nsUrl == IntPtr.Zero)
        {
            throw new ArgumentException("The app cast URL is not a valid URL.", nameof(value));
        }

        return nsUrl;
    }

    private IntPtr updater;

    public bool IsInitialized { get; private set; }
    public string? AppCastUrl { get; private set; }
    public string? PublicKey { get; private set; }
    public string? CompanyName { get; private set; }
    public string? AppName { get; private set; }
    public string? AppVersion { get; private set; }

    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appCastUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(appVersion);

        AppCastUrl = appCastUrl;
        PublicKey = publicKey;
        CompanyName = companyName;
        AppName = appName;
        AppVersion = appVersion;

        var bundle = CreateMainBundle();
        updater = Send(Class("SUUpdater"), "updaterForBundle:", bundle);

        if (updater == IntPtr.Zero)
        {
            throw new InvalidOperationException("Sparkle updater could not be created.");
        }

        SendVoid(updater, "setFeedURL:", CreateUrl(AppCastUrl));
        IsInitialized = true;
    }

    public void CheckUpdateWithUI()
    {
        if (!IsInitialized || updater == IntPtr.Zero)
        {
            throw new InvalidOperationException($"{nameof(MacSparkle)} is not initialized");
        }

        Send(updater, "checkForUpdates:", IntPtr.Zero);
    }

    public void Dispose()
    {
        updater = IntPtr.Zero;
        IsInitialized = false;
    }
}
