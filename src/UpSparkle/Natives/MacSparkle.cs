using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle.Natives;

internal class MacSparkle: INativeSparkle
{
    private const string LibName = "libMacSparkle";

    static MacSparkle()
    {
        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(),
            static (libraryName, assembly, searchPath) =>
            {
                if (libraryName != LibName)
                    return IntPtr.Zero;

                /*
                var arch = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.Arm64 => "arm64",
                    Architecture.X86   => "x64",
                    _                  => "x86",
                };
                */
                
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
                    Path.Combine(baseDir, $"{LibName}.dylib"),
                    Path.Combine(baseDir, "runtimes", "osx", "native", $"{LibName}.dylib"),
                    // Alongside the managed assembly itself
                    Path.Combine(assemblyDir, $"{LibName}.dylib"),
                    Path.Combine(assemblyDir, "runtimes", "osx", "native", $"{LibName}.dylib"),
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
    private static extern void mac_sparkle_set_appcast_url([MarshalAs(UnmanagedType.LPStr)] string url);
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void mac_sparkle_init();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void mac_sparkle_check_update_with_ui();
    
    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        mac_sparkle_set_appcast_url(appCastUrl);
        mac_sparkle_init();
    }

    public void CheckUpdateWithUI()
    {
        mac_sparkle_check_update_with_ui();
    }

    public void Dispose()
    {
    }
}
