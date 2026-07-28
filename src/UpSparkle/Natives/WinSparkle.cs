using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle.Natives
{
    internal sealed class WinSparkle : INativeSparkle
    {
        private const string LibName = "WinSparkle";

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_init_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_cleanup_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void win_sparkle_set_appcast_url_delegate(
            [MarshalAs(UnmanagedType.LPStr)] string url);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void win_sparkle_set_eddsa_public_key_delegate(
            [MarshalAs(UnmanagedType.LPStr)] string publicKey);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void win_sparkle_set_app_details_delegate(
            [MarshalAs(UnmanagedType.LPWStr)] string companyName,
            [MarshalAs(UnmanagedType.LPWStr)] string appName,
            [MarshalAs(UnmanagedType.LPWStr)] string appVersion);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_check_update_with_ui_delegate();

        private static readonly win_sparkle_init_delegate win_sparkle_init;
        private static readonly win_sparkle_cleanup_delegate win_sparkle_cleanup;
        private static readonly win_sparkle_set_appcast_url_delegate win_sparkle_set_appcast_url;
        private static readonly win_sparkle_set_eddsa_public_key_delegate win_sparkle_set_eddsa_public_key;
        private static readonly win_sparkle_set_app_details_delegate win_sparkle_set_app_details;
        private static readonly win_sparkle_check_update_with_ui_delegate win_sparkle_check_update_with_ui;

        static WinSparkle()
        {
            var handle = LoadNativeLibrary();
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException(
                    $"Unable to load native library '{LibName}.dll'.");
            }

            win_sparkle_init = GetDelegate<win_sparkle_init_delegate>(handle, nameof(win_sparkle_init));
            win_sparkle_cleanup = GetDelegate<win_sparkle_cleanup_delegate>(handle, nameof(win_sparkle_cleanup));
            win_sparkle_set_appcast_url = GetDelegate<win_sparkle_set_appcast_url_delegate>(handle, nameof(win_sparkle_set_appcast_url));
            win_sparkle_set_eddsa_public_key = GetDelegate<win_sparkle_set_eddsa_public_key_delegate>(handle, nameof(win_sparkle_set_eddsa_public_key));
            win_sparkle_set_app_details = GetDelegate<win_sparkle_set_app_details_delegate>(handle, nameof(win_sparkle_set_app_details));
            win_sparkle_check_update_with_ui = GetDelegate<win_sparkle_check_update_with_ui_delegate>(handle, nameof(win_sparkle_check_update_with_ui));
        }

        private static T GetDelegate<T>(IntPtr moduleHandle, string procName) where T : class
        {
            var procAddress = GetProcAddress(moduleHandle, procName);
            if (procAddress == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException(
                    $"Unable to find entry point '{procName}' in '{LibName}.dll'.");
            }

            return Marshal.GetDelegateForFunctionPointer<T>(procAddress);
        }

        private static IntPtr LoadNativeLibrary()
        {
            var arch = "x86";
            
            switch(RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                default:
                    arch = "x86";
                    break;
            };

            var baseDir = AppContext.BaseDirectory;
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = string.IsNullOrEmpty(assemblyLocation)
                ? baseDir
                : Path.GetDirectoryName(assemblyLocation) ?? baseDir;

            // Probe candidate paths in priority order:
            //   1. NuGet package layout: runtimes/win-{rid}/native/ resolves to plain
            //      WinSparkle.dll next to the app (standard .NET NuGet native resolution).
            //   2. Local dev layout: libs/WinSparkle.{arch}.dll under the app base dir.
            //   3. Assembly directory (self-contained publish, single-file scenarios).
            var candidates = new[]
            {
                Path.Combine(baseDir, "runtimes", $"win-{arch}", "native", $"{LibName}.dll"),
                Path.Combine(assemblyDir, "runtimes", $"win-{arch}", "native", $"{LibName}.dll"),
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    var handle = LoadLibrary(candidate);
                    if (handle != IntPtr.Zero)
                        return handle;
                }
            }

            // Fall back to the default OS resolver (PATH, etc.)
            return LoadLibrary($"{LibName}.dll");
        }

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
}
