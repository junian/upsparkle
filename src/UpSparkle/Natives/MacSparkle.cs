using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle.Natives
{
    /// <summary>
    /// macOS-specific implementation of <see cref="INativeSparkle"/> that wraps the
    /// Sparkle framework via the <c>libMacSparkle.dylib</c> bridge library.
    /// The dylib is loaded at class initialization time using <c>dlopen</c>, with candidate
    /// paths probed in NuGet-layout order before falling back to the OS dynamic linker.
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

        /// <summary>
        /// Loads <c>libMacSparkle.dylib</c> and resolves all required native function pointers.
        /// Runs once per app domain.
        /// </summary>
        /// <exception cref="DllNotFoundException">
        /// Thrown when <c>libMacSparkle.dylib</c> cannot be found in any of the probed locations.
        /// </exception>
        /// <exception cref="EntryPointNotFoundException">
        /// Thrown when a required exported symbol cannot be found in the loaded library.
        /// </exception>
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

        /// <summary>
        /// Resolves a named symbol from a loaded native library handle and returns it as a
        /// managed delegate of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The delegate type that matches the native function's signature.</typeparam>
        /// <param name="moduleHandle">The library handle returned by <c>dlopen</c>.</param>
        /// <param name="procName">The name of the symbol to resolve via <c>dlsym</c>.</param>
        /// <returns>A delegate wrapping the native function pointer.</returns>
        /// <exception cref="EntryPointNotFoundException">
        /// Thrown when <paramref name="procName"/> cannot be found in the library.
        /// </exception>
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
        /// Probes well-known paths for <c>libMacSparkle.dylib</c> and loads it via
        /// <c>dlopen</c>. Falls back to the OS dynamic linker (<c>DYLD_LIBRARY_PATH</c>,
        /// rpath, etc.) if no candidate file is found on disk.
        /// </summary>
        /// <returns>
        /// A non-zero library handle on success, or <see cref="IntPtr.Zero"/> if the library
        /// could not be loaded.
        /// </returns>
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
        /// Configures and starts the Sparkle updater by setting the appcast URL and calling
        /// <c>mac_sparkle_init</c>. The public key and app details are managed by the
        /// Sparkle framework via the app bundle's <c>Info.plist</c> on macOS.
        /// </summary>
        /// <param name="appCastUrl">The URL of the appcast XML feed.</param>
        /// <param name="publicKey">
        /// The EdDSA public key. Accepted for API consistency but handled natively via
        /// <c>Info.plist</c> on macOS; this parameter is not forwarded to the Sparkle framework.
        /// </param>
        /// <param name="companyName">The name of the company or publisher (unused on macOS).</param>
        /// <param name="appName">The display name of the application (unused on macOS).</param>
        /// <param name="appVersion">The current version string of the application (unused on macOS).</param>
        public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
        {
            mac_sparkle_set_appcast_url(appCastUrl);
            mac_sparkle_init();
        }

        /// <summary>
        /// Triggers the Sparkle update UI, which checks for a new version and, if one is
        /// available, presents the user with a download and install dialog.
        /// </summary>
        public void CheckUpdateWithUI()
        {
            mac_sparkle_check_update_with_ui();
        }

        /// <summary>
        /// No-op on macOS. The Sparkle framework manages its own lifecycle through the
        /// Objective-C runtime and does not require an explicit cleanup call.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
