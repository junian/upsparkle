using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle;

internal sealed class WindowsUpSparkleImplementation : IUpSparklePlatformImplementation
{
    private const string LibName = "WinSparkle";

    static WindowsUpSparkleImplementation()
    {
        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(),
            static (libraryName, assembly, searchPath) =>
            {
                if (libraryName != LibName)
                    return IntPtr.Zero;

                var arch = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.Arm64 => "arm64",
                    _                  => "x86_64",
                };

                var baseDir = AppContext.BaseDirectory;
                var dllPath = Path.Combine(baseDir, "libs", $"WinSparkle.{arch}.dll");

                return NativeLibrary.Load(dllPath, assembly, searchPath);
            });
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_init();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_cleanup();

    [DllImport(LibName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_set_appcast_url(string url);

    [DllImport(LibName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_set_eddsa_public_key(string publicKey);

    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_set_app_details(
        string companyName,
        string appName,
        string appVersion);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_check_update_with_ui();

    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        win_sparkle_set_appcast_url(appCastUrl);
        win_sparkle_set_eddsa_public_key(publicKey);
        win_sparkle_set_app_details(companyName, appName, appVersion);
        win_sparkle_init();
    }

    public void CheckUpdateWithUI()
    {
        win_sparkle_check_update_with_ui();
    }

    public void Dispose()
    {
        win_sparkle_cleanup();
    }
}
