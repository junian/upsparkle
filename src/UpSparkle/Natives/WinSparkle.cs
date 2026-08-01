using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UpSparkle.Natives
{
    /// <summary>
    /// Windows-specific implementation of <see cref="INativeSparkle"/> that wraps the
    /// WinSparkle native DLL via P/Invoke. The correct architecture variant of
    /// <c>WinSparkle.dll</c> (x86, x64, or arm64) is loaded at class initialization time
    /// from the NuGet <c>runtimes/win-{rid}/native/</c> layout.
    /// </summary>
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_check_update_without_ui_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void win_sparkle_set_http_header_delegate(
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_clear_http_headers_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_error_callback_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_set_error_callback_delegate(IntPtr callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_set_automatic_check_for_updates_delegate(int state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int win_sparkle_get_automatic_check_for_updates_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void win_sparkle_set_update_check_interval_delegate(uint interval);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint win_sparkle_get_update_check_interval_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long win_sparkle_get_last_check_time_delegate();

        private static readonly win_sparkle_init_delegate win_sparkle_init;
        private static readonly win_sparkle_cleanup_delegate win_sparkle_cleanup;
        private static readonly win_sparkle_set_appcast_url_delegate win_sparkle_set_appcast_url;
        private static readonly win_sparkle_set_eddsa_public_key_delegate win_sparkle_set_eddsa_public_key;
        private static readonly win_sparkle_set_app_details_delegate win_sparkle_set_app_details;
        private static readonly win_sparkle_check_update_with_ui_delegate win_sparkle_check_update_with_ui;
        private static readonly win_sparkle_check_update_without_ui_delegate win_sparkle_check_update_without_ui;
        private static readonly win_sparkle_set_http_header_delegate win_sparkle_set_http_header;
        private static readonly win_sparkle_clear_http_headers_delegate win_sparkle_clear_http_headers;
        private static readonly win_sparkle_set_error_callback_delegate win_sparkle_set_error_callback;
        private static readonly win_sparkle_set_automatic_check_for_updates_delegate win_sparkle_set_automatic_check_for_updates;
        private static readonly win_sparkle_get_automatic_check_for_updates_delegate win_sparkle_get_automatic_check_for_updates;
        private static readonly win_sparkle_set_update_check_interval_delegate win_sparkle_set_update_check_interval;
        private static readonly win_sparkle_get_update_check_interval_delegate win_sparkle_get_update_check_interval;
        private static readonly win_sparkle_get_last_check_time_delegate win_sparkle_get_last_check_time;

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
        private static readonly win_sparkle_error_callback_delegate errorCallbackHandle =
            new win_sparkle_error_callback_delegate(ErrorCallbackDispatcher);

        /// <summary>
        /// AOT-compatible bridge invoked by the native library when an error occurs.
        /// Dispatches to the current <see cref="errorCallback"/>, if any.
        /// </summary>
        [MonoPInvokeCallback(typeof(win_sparkle_error_callback_delegate))]
        private static void ErrorCallbackDispatcher()
        {
            errorCallback?.Invoke();
        }

        /// <summary>
        /// Loads <c>WinSparkle.dll</c> for the current process architecture and resolves
        /// all required native function pointers. Runs once per app domain.
        /// </summary>
        /// <exception cref="DllNotFoundException">
        /// Thrown when <c>WinSparkle.dll</c> cannot be found in any of the probed locations.
        /// </exception>
        /// <exception cref="EntryPointNotFoundException">
        /// Thrown when a required exported function cannot be found in the loaded DLL.
        /// </exception>
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
            win_sparkle_check_update_without_ui = GetDelegate<win_sparkle_check_update_without_ui_delegate>(handle, nameof(win_sparkle_check_update_without_ui));
            win_sparkle_set_http_header = GetDelegate<win_sparkle_set_http_header_delegate>(handle, nameof(win_sparkle_set_http_header));
            win_sparkle_clear_http_headers = GetDelegate<win_sparkle_clear_http_headers_delegate>(handle, nameof(win_sparkle_clear_http_headers));
            win_sparkle_set_error_callback = GetDelegate<win_sparkle_set_error_callback_delegate>(handle, nameof(win_sparkle_set_error_callback));
            win_sparkle_set_automatic_check_for_updates = GetDelegate<win_sparkle_set_automatic_check_for_updates_delegate>(handle, nameof(win_sparkle_set_automatic_check_for_updates));
            win_sparkle_get_automatic_check_for_updates = GetDelegate<win_sparkle_get_automatic_check_for_updates_delegate>(handle, nameof(win_sparkle_get_automatic_check_for_updates));
            win_sparkle_set_update_check_interval = GetDelegate<win_sparkle_set_update_check_interval_delegate>(handle, nameof(win_sparkle_set_update_check_interval));
            win_sparkle_get_update_check_interval = GetDelegate<win_sparkle_get_update_check_interval_delegate>(handle, nameof(win_sparkle_get_update_check_interval));
            win_sparkle_get_last_check_time = GetDelegate<win_sparkle_get_last_check_time_delegate>(handle, nameof(win_sparkle_get_last_check_time));
        }

        /// <summary>
        /// Resolves a named exported function from a loaded native module and returns it as a
        /// managed delegate of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The delegate type that matches the native function's signature.</typeparam>
        /// <param name="moduleHandle">The module handle returned by <see cref="LoadLibrary"/>.</param>
        /// <param name="procName">The name of the exported function to resolve.</param>
        /// <returns>A delegate wrapping the native function pointer.</returns>
        /// <exception cref="EntryPointNotFoundException">
        /// Thrown when <paramref name="procName"/> cannot be found in the module.
        /// </exception>
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

        /// <summary>
        /// Probes well-known paths for the architecture-appropriate <c>WinSparkle.dll</c> and
        /// loads it into the current process. Falls back to the OS loader (PATH / DLL search
        /// order) if no candidate file is found on disk.
        /// </summary>
        /// <returns>
        /// A non-zero module handle on success, or <see cref="IntPtr.Zero"/> if the library
        /// could not be loaded.
        /// </returns>
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

        /// <summary>
        /// Set App Details to the native updater
        /// </summary>
        /// <param name="companyName">The name of the company or publisher.</param>
        /// <param name="appName">The display name of the application.</param>
        /// <param name="appVersion">The current version string of the application.</param>
        public void SetAppDetails(string companyName, string appName, string appVersion)
        {
            win_sparkle_set_app_details(companyName, appName, appVersion);
        }

        /// <summary>
        /// Set the Appcast URL for the native updater
        /// </summary>
        /// <param name="appcastUrl">The URL of the appcast XML feed.</param>
        public void SetAppcastUrl(string appcastUrl)
        {
            win_sparkle_set_appcast_url(appcastUrl);
        }

        /// <summary>
        /// Set the EdDSA public key for signature verification in the native updater
        /// </summary>
        /// <param name="edDSAPublicKey">The EdDSA public key (Base64-encoded) for signature verification.</param>
        public void SetEdDSAPublicKey(string edDSAPublicKey)
        {
            win_sparkle_set_eddsa_public_key(edDSAPublicKey);
        }

        /// <summary>
        /// Initialize and starts WinSparkle.
        /// </summary>
        public void Initialize()
        {
            win_sparkle_init();
        }

        /// <summary>
        /// Triggers the WinSparkle update UI, which checks for a new version and, if one is
        /// available, presents the user with a download and install dialog.
        /// </summary>
        public void CheckUpdateWithUI()
        {
            win_sparkle_check_update_with_ui();
        }

        /// <summary>
        /// Triggers an update check in the background without user interface feedback.
        /// Use with caution and generally not recommended: by default WinSparkle
        /// checks for updates automatically, and calling this manually may interfere
        /// with its scheduler.
        /// </summary>
        public void CheckUpdateWithoutUI()
        {
            win_sparkle_check_update_without_ui();
        }

        /// <summary>
        /// Sets an HTTP header to be sent with update requests (appcast checks and
        /// update downloads). Calling again with the same name replaces the previous value.
        /// </summary>
        /// <param name="name">The HTTP header name.</param>
        /// <param name="value">The HTTP header value.</param>
        public void SetHttpHeader(string name, string value)
        {
            win_sparkle_set_http_header(name, value);
        }

        /// <summary>
        /// Clears all HTTP headers previously set via <see cref="SetHttpHeader"/>.
        /// </summary>
        public void ClearHttpHeaders()
        {
            win_sparkle_clear_http_headers();
        }

        /// <summary>
        /// Sets a callback to be invoked when the WinSparkle updater encounters an error.
        /// Pass <see langword="null"/> to clear a previously set callback.
        /// </summary>
        /// <param name="callback">
        /// The method to invoke when the updater encounters an error, or
        /// <see langword="null"/> to clear the previously set callback.
        /// </param>
        public void SetErrorCallback(NativeSparkleCallback.NativeSparkleErrorCallback callback)
        {
            errorCallback = callback;

            win_sparkle_set_error_callback(callback != null
                ? Marshal.GetFunctionPointerForDelegate<win_sparkle_error_callback_delegate>(errorCallbackHandle)
                : IntPtr.Zero);
        }

        /// <summary>
        /// Gets or sets a value indicating whether WinSparkle should automatically check for updates.
        /// </summary>
        public bool IsAutomaticCheckForUpdates
        {
            get { return win_sparkle_get_automatic_check_for_updates() != 0; }
            set { win_sparkle_set_automatic_check_for_updates(value ? 1 : 0); }
        }

        /// <summary>
        /// Gets or sets the interval in seconds between automatic update checks.
        /// </summary>
        public int UpdateCheckInterval
        {
            get { return (int)win_sparkle_get_update_check_interval(); }
            set { win_sparkle_set_update_check_interval((uint)value); }
        }

        /// <summary>
        /// Gets the time of the last update check, or <see langword="null"/> if updates
        /// have never been checked.
        /// </summary>
        public DateTime? LastCheckTime
        {
            get
            {
                var unixTime = win_sparkle_get_last_check_time();
                if (unixTime <= 0)
                    return null;
                return UnixEpoch.AddSeconds(unixTime);
            }
        }

        /// <summary>
        /// Shuts down the WinSparkle background thread and releases its resources by calling
        /// <c>win_sparkle_cleanup</c>. Should be called before the application exits.
        /// </summary>
        public void Dispose()
        {
            win_sparkle_cleanup();
        }
    }
}
