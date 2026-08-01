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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_check_update_without_ui_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_set_automatic_check_for_updates_delegate(int state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int mac_sparkle_get_automatic_check_for_updates_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_set_update_check_interval_delegate(int interval);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int mac_sparkle_get_update_check_interval_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long mac_sparkle_get_last_check_time_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void mac_sparkle_set_http_header_delegate(
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_clear_http_headers_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_error_callback_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void mac_sparkle_set_error_callback_delegate(IntPtr callback);

        private static readonly mac_sparkle_set_appcast_url_delegate mac_sparkle_set_appcast_url;
        private static readonly mac_sparkle_init_delegate mac_sparkle_init;
        private static readonly mac_sparkle_check_update_with_ui_delegate mac_sparkle_check_update_with_ui;
        private static readonly mac_sparkle_check_update_without_ui_delegate mac_sparkle_check_update_without_ui;
        private static readonly mac_sparkle_set_automatic_check_for_updates_delegate mac_sparkle_set_automatic_check_for_updates;
        private static readonly mac_sparkle_get_automatic_check_for_updates_delegate mac_sparkle_get_automatic_check_for_updates;
        private static readonly mac_sparkle_set_update_check_interval_delegate mac_sparkle_set_update_check_interval;
        private static readonly mac_sparkle_get_update_check_interval_delegate mac_sparkle_get_update_check_interval;
        private static readonly mac_sparkle_get_last_check_time_delegate mac_sparkle_get_last_check_time;
        private static readonly mac_sparkle_set_http_header_delegate mac_sparkle_set_http_header;
        private static readonly mac_sparkle_clear_http_headers_delegate mac_sparkle_clear_http_headers;
        private static readonly mac_sparkle_set_error_callback_delegate mac_sparkle_set_error_callback;

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The user-supplied error callback, invoked by <see cref="ErrorCallbackDispatcher"/>.
        /// Holds a strong reference so the callback is not garbage collected while the
        /// native library still holds a reference to it.
        /// </summary>
        private static NativeSparkleCallback.NativeSparkleErrorCallback errorCallback;

        /// <summary>
        /// Delegate passed to the native library. Wraps the static
        /// <see cref="ErrorCallbackDispatcher"/> method so the callback can be AOT-compiled:
        /// Mono AOT / IL2CPP require callbacks from native code to target a static method
        /// annotated with <see cref="MonoPInvokeCallbackAttribute"/>. Kept in a static field
        /// so it is not garbage collected while the native library holds the function pointer.
        /// </summary>
        private static readonly mac_sparkle_error_callback_delegate errorCallbackHandle =
            new mac_sparkle_error_callback_delegate(ErrorCallbackDispatcher);

        /// <summary>
        /// AOT-compatible bridge invoked by the native library when an error occurs.
        /// Dispatches to the current <see cref="errorCallback"/>, if any.
        /// </summary>
        [MonoPInvokeCallback(typeof(mac_sparkle_error_callback_delegate))]
        private static void ErrorCallbackDispatcher()
        {
            errorCallback?.Invoke();
        }

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
            mac_sparkle_check_update_without_ui = GetDelegate<mac_sparkle_check_update_without_ui_delegate>(handle, nameof(mac_sparkle_check_update_without_ui));
            mac_sparkle_set_automatic_check_for_updates = GetDelegate<mac_sparkle_set_automatic_check_for_updates_delegate>(handle, nameof(mac_sparkle_set_automatic_check_for_updates));
            mac_sparkle_get_automatic_check_for_updates = GetDelegate<mac_sparkle_get_automatic_check_for_updates_delegate>(handle, nameof(mac_sparkle_get_automatic_check_for_updates));
            mac_sparkle_set_update_check_interval = GetDelegate<mac_sparkle_set_update_check_interval_delegate>(handle, nameof(mac_sparkle_set_update_check_interval));
            mac_sparkle_get_update_check_interval = GetDelegate<mac_sparkle_get_update_check_interval_delegate>(handle, nameof(mac_sparkle_get_update_check_interval));
            mac_sparkle_get_last_check_time = GetDelegate<mac_sparkle_get_last_check_time_delegate>(handle, nameof(mac_sparkle_get_last_check_time));
            mac_sparkle_set_http_header = GetDelegate<mac_sparkle_set_http_header_delegate>(handle, nameof(mac_sparkle_set_http_header));
            mac_sparkle_clear_http_headers = GetDelegate<mac_sparkle_clear_http_headers_delegate>(handle, nameof(mac_sparkle_clear_http_headers));
            mac_sparkle_set_error_callback = GetDelegate<mac_sparkle_set_error_callback_delegate>(handle, nameof(mac_sparkle_set_error_callback));
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
        /// Set App Details to the native updater.
        /// It does nothing to Sparkle on macOS, as Sparkle reads app details from the app bundle's Info.plist.
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="appName"></param>
        /// <param name="appVersion"></param>
        public void SetAppDetails(string companyName, string appName, string appVersion) { }

        /// <summary>
        /// Set Appcast URL to Sparkle native updater.
        /// </summary>
        /// <param name="appcastUrl"></param>
        public void SetAppcastUrl(string appcastUrl)
        {
            mac_sparkle_set_appcast_url(appcastUrl);
        }

        /// <summary>
        /// Set EdDSA public key to Sparkle native updater.
        /// It does nothing to Sparkle on macOS, as Sparkle reads the public key from the app bundle's Info.plist.
        /// </summary>
        /// <param name="edDSAPublicKey"></param>
        public void SetEdDSAPublicKey(string edDSAPublicKey) { }

        /// <summary>
        /// Initialize and start the Sparkle framework.
        /// </summary>
        public void Initialize()
        {
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
        /// Triggers an update check in the background without user interface feedback.
        /// Use with caution and generally not recommended: by default Sparkle schedules
        /// background checks automatically, and calling this manually may interfere
        /// with Sparkle's scheduler.
        /// </summary>
        public void CheckUpdateWithoutUI()
        {
            mac_sparkle_check_update_without_ui();
        }

        /// <summary>
        /// Sets an HTTP header to be sent with update requests (appcast checks,
        /// release note downloads, and update downloads). The header is stored in the
        /// updater's HTTP headers dictionary; calling again with the same name replaces
        /// the previous value.
        /// </summary>
        /// <param name="name">The HTTP header name.</param>
        /// <param name="value">The HTTP header value.</param>
        public void SetHttpHeader(string name, string value)
        {
            mac_sparkle_set_http_header(name, value);
        }

        /// <summary>
        /// Clears all HTTP headers previously set via <see cref="SetHttpHeader"/>.
        /// </summary>
        public void ClearHttpHeaders()
        {
            mac_sparkle_clear_http_headers();
        }

        /// <summary>
        /// Sets a callback to be invoked when the Sparkle updater encounters an error.
        /// The callback is invoked on the main thread with no arguments.
        /// Pass <see langword="null"/> to clear a previously set callback.
        /// The callback is not invoked for the normal "no update found" outcome or for
        /// a user-canceled installation.
        /// </summary>
        /// <param name="callback">
        /// The method to invoke when the updater encounters an error, or
        /// <see langword="null"/> to clear the previously set callback.
        /// </param>
        public void SetErrorCallback(NativeSparkleCallback.NativeSparkleErrorCallback callback)
        {
            errorCallback = callback;

            mac_sparkle_set_error_callback(callback != null
                ? Marshal.GetFunctionPointerForDelegate<mac_sparkle_error_callback_delegate>(errorCallbackHandle)
                : IntPtr.Zero);
        }

        /// <summary>
        /// Gets or sets a value indicating whether Sparkle should automatically check for updates.
        /// </summary>
        public bool IsAutomaticCheckForUpdates
        {
            get { return mac_sparkle_get_automatic_check_for_updates() != 0; }
            set { mac_sparkle_set_automatic_check_for_updates(value ? 1 : 0); }
        }

        /// <summary>
        /// Gets or sets the interval in seconds between automatic update checks.
        /// </summary>
        public int UpdateCheckInterval
        {
            get { return mac_sparkle_get_update_check_interval(); }
            set { mac_sparkle_set_update_check_interval(value); }
        }

        /// <summary>
        /// Gets the time of the last update check, or <see langword="null"/> if updates
        /// have never been checked.
        /// </summary>
        public DateTime? LastCheckTime
        {
            get
            {
                var unixTime = mac_sparkle_get_last_check_time();
                if (unixTime <= 0)
                    return null;
                return UnixEpoch.AddSeconds(unixTime);
            }
        }

        /// <summary>
        /// Dispose the native Sparkle Updater.
        /// It does not do anything on macOS, but is required for API consistency with other platforms.
        /// </summary>
        public void Dispose() { }
    }
}
