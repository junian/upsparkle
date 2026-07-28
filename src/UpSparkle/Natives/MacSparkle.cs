using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle.Natives
{
    /// <summary>
    /// Sparkle for macOS wrapper in C#
    /// </summary>
    internal class MacSparkle : INativeSparkle
    {
        private const string LibName = "libMacSparkle";

        // On modern macOS, libdl's symbols are folded into libSystem;
        // "libdl.dylib" still resolves correctly via the shared cache.
        private const string LibDl = "libdl.dylib";

        private const int RTLD_NOW = 2;

        [DllImport(LibDl, EntryPoint = "dlopen")]
        private static extern IntPtr dlopen(string path, int mode);

        [DllImport(LibDl, EntryPoint = "dlsym")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(LibDl, EntryPoint = "dlerror")]
        private static extern IntPtr dlerror();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_set_appcast_url_delegate(
            [MarshalAs(UnmanagedType.LPStr)] string url);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_init_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_check_update_with_ui_delegate();

        
        private static readonly mac_sparkle_set_appcast_url_delegate mac_sparkle_set_appcast_url;
        private static readonly mac_sparkle_init_delegate mac_sparkle_init;
        private static readonly mac_sparkle_check_update_with_ui_delegate mac_sparkle_check_update_with_ui;

        static MacSparkle()
        {
            var handle = LoadNativeLibrary();
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException(
                    $"Unable to load native library '{LibName}.dylib'.");
            }

            mac_sparkle_set_appcast_url = GetDelegate<mac_sparkle_set_appcast_url_delegate>(handle, nameof(mac_sparkle_set_appcast_url));
            mac_sparkle_init = GetDelegate<mac_sparkle_init_delegate>(handle, nameof(mac_sparkle_init));
            mac_sparkle_check_update_with_ui = GetDelegate<mac_sparkle_check_update_with_ui_delegate>(handle, nameof(mac_sparkle_check_update_with_ui));
        }

        private static T GetDelegate<T>(IntPtr moduleHandle, string procName) where T : class
        {
            var procAddress = dlsym(moduleHandle, procName);
            if (procAddress == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException(
                    $"Unable to find entry point '{procName}' in '{LibName}.dylib'.");
            }

            return Marshal.GetDelegateForFunctionPointer<T>(procAddress);
        }

        /// <summary>
        /// Load native library
        /// </summary>
        /// <returns></returns>
        private static IntPtr LoadNativeLibrary()
        {
            var baseDir = AppContext.BaseDirectory;
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = string.IsNullOrEmpty(assemblyLocation)
                ? baseDir
                : Path.GetDirectoryName(assemblyLocation) ?? baseDir;

            // Probe candidate paths in priority order:
            //   1. NuGet package layout: runtimes/osx/native/ resolves to plain
            //      libMacSparkle.dylib next to the app.
            //   2. Local dev layout: plain dylib under the app base dir.
            //   3. Assembly directory (self-contained publish, single-file scenarios).
            var candidates = new[]
            {
                Path.Combine(baseDir, "runtimes", "osx", "native", $"{LibName}.dylib"),
                Path.Combine(assemblyDir, "runtimes", "osx", "native", $"{LibName}.dylib"),
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    var handle = dlopen(candidate, RTLD_NOW);
                    if (handle != IntPtr.Zero)
                        return handle;
                }
            }

            // Fall back to the default OS resolver (DYLD_LIBRARY_PATH, rpath, etc.)
            return dlopen($"{LibName}.dylib", RTLD_NOW);
        }

        /// <summary>
        /// Initialize Sparkle for macOS
        /// </summary>
        /// <param name="appCastUrl"></param>
        /// <param name="publicKey"></param>
        /// <param name="companyName"></param>
        /// <param name="appName"></param>
        /// <param name="appVersion"></param>
        public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
        {
            mac_sparkle_set_appcast_url(appCastUrl);
            mac_sparkle_init();
        }

        /// <summary>
        /// Check Update with Sparkle UI
        /// </summary>
        public void CheckUpdateWithUI()
        {
            mac_sparkle_check_update_with_ui();
        }

        /// <summary>
        /// Dispose the native object
        /// </summary>
        public void Dispose()
        {
        }
    }
}
