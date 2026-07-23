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
                var assemblyDir = Path.GetDirectoryName(assembly.Location) ?? baseDir;

                // Probe candidate paths in priority order:
                //   1. NuGet package layout: runtimes/win-{rid}/native/ resolves to plain
                //      WinSparkle.dll next to the app (standard .NET NuGet native resolution).
                //   2. Local dev layout: libs/WinSparkle.{arch}.dll under the app base dir.
                //   3. Assembly directory (self-contained publish, single-file scenarios).
                var candidates = new[]
                {
                    // NuGet / publish output — plain name, no arch suffix
                    Path.Combine(baseDir, $"{LibName}.dll"),
                    // Local dev build — arch-suffixed under libs/
                    Path.Combine(baseDir, "libs", $"{LibName}.{arch}.dll"),
                    // Alongside the managed assembly itself
                    Path.Combine(assemblyDir, $"{LibName}.dll"),
                    Path.Combine(assemblyDir, "libs", $"{LibName}.{arch}.dll"),
                };

                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate) &&
                        NativeLibrary.TryLoad(candidate, out var handle))
                    {
                        return handle;
                    }
                }

                // Fall back to the default OS resolver (PATH, etc.)
                return IntPtr.Zero;
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
