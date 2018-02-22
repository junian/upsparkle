using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Juniansoft.Upsparkle
{
    internal delegate void Callback();

    public class UpsparkleUpdater: IUpsparkleUpdater
    {
        private const string libraryName = "WinSparkle.dll";

        internal UpsparkleUpdater()
        {
            var dllpath = string.Empty;

            var resourceName = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            
            if (IntPtr.Size == 4)
            {
                // 32-bit
                resourceName = $"{typeof(IUpsparkleUpdater).Namespace}.x86.{libraryName}";
                dllpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x86");
            }
            else
            {
                // 64-bit
                resourceName = $"{typeof(IUpsparkleUpdater).Namespace}.x64.{libraryName}";
                dllpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64");
            }
            
            Utilites.LoadUnmanagedLibrary(Path.Combine(dllpath, libraryName));

            //Utilites.LoadUnmanagedLibraryFromResource(assembly, resourceName, libraryName);

            SetErrorCallback(() => Error?.Invoke(this, new EventArgs()));
            SetCanShutdownCallback(() => CanShutdown?.Invoke(this, new EventArgs()));
            SetShutdownRequestCallback(() => ShutdownRequest?.Invoke(this, new EventArgs()));
            SetDidFindUpdateCallback(() => DidFindUpdate?.Invoke(this, new EventArgs()));
            SetDidNotFindUpdateCallback(() => DidNotFindUpdate?.Invoke(this, new EventArgs()));
            SetUpdateCancelledCallback(() => UpdateCancelled?.Invoke(this, new EventArgs()));
        }
        
        public event EventHandler<EventArgs> Error;
        public event EventHandler<EventArgs> CanShutdown;
        public event EventHandler<EventArgs> ShutdownRequest;
        public event EventHandler<EventArgs> DidFindUpdate;
        public event EventHandler<EventArgs> DidNotFindUpdate;
        public event EventHandler<EventArgs> UpdateCancelled;
        
        #region Init and Cleanup

        public void Cleanup()
        {
            win_sparkle_cleanup();
        }

        public void Init()
        {
            win_sparkle_init();
        }

        #endregion

        #region Language settings

        public void SetLang(string lang)
        {
            win_sparkle_set_lang(lang);
        }

        public void SetLangId(ushort lang)
        {
            win_sparkle_set_langid(lang);
        }

        #endregion

        #region Configuration

        public bool AutomaticCheckForUpdates
        {
            get
            {
                return win_sparkle_get_automatic_check_for_updates() != 0;
            }

            set
            {
                win_sparkle_set_automatic_check_for_updates(value ? 1 : 0);
            }
        }

        public DateTime LastCheckTime
        {
            get
            {
                long time = win_sparkle_get_last_check_time();
                if (time > 0)
                    return new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(time);
                else
                    return DateTime.MinValue;
            }
        }

        public TimeSpan UpdateCheckInterval
        {
            get
            {
                return TimeSpan.FromSeconds(win_sparkle_get_update_check_interval());
            }

            set
            {
                win_sparkle_set_update_check_interval((int)value.TotalSeconds);
            }
        }

        public void SetAppBuildVersion(string build)
        {
            win_sparkle_set_app_build_version(build);
        }

        public void SetAppcastUrl(string url)
        {
            win_sparkle_set_appcast_url(url);
        }

        public void SetAppDetails(string companyName, string appName, string appVersion)
        {
            win_sparkle_set_app_details(companyName, appName, appVersion);
        }

        public void SetRegistryPath(string path)
        {
            win_sparkle_set_registry_path(path);
        }

        private void SetErrorCallback(Callback callback)
        {
            win_sparkle_set_error_callback(callback);
        }

        private void SetCanShutdownCallback(Callback callback)
        {
            win_sparkle_set_can_shutdown_callback(callback);
        }

        private void SetShutdownRequestCallback(Callback callback)
        {
            win_sparkle_set_shutdown_request_callback(callback);
        }

        private void SetDidFindUpdateCallback(Callback callback)
        {
            win_sparkle_set_did_find_update_callback(callback);
        }

        private void SetDidNotFindUpdateCallback(Callback callback)
        {
            win_sparkle_set_did_not_find_update_callback(callback);
        }

        private void SetUpdateCancelledCallback(Callback callback)
        {
            win_sparkle_set_update_cancelled_callback(callback);
        }

        #endregion

        #region Manual

        public void CheckUpdateWithoutUI()
        {
            win_sparkle_check_update_without_ui();
        }

        public void CheckUpdateWithUI()
        {
            win_sparkle_check_update_with_ui();
        }

        public void CheckUpdateWithUIAndInstall()
        {
            win_sparkle_check_update_with_ui_and_install();
        }

        #endregion

        // Initialization and shutdown
        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_init();

        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_cleanup();

        // Language Settings
        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_lang(string lang);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_langid(ushort lang);

        // Configuration
        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_appcast_url(string url);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_app_details(string companyName, string appName, string appVersion);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_app_build_version(string build);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_registry_path(string path);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_automatic_check_for_updates(int state);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int win_sparkle_get_automatic_check_for_updates();

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_set_update_check_interval(int interval);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int win_sparkle_get_update_check_interval();

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_get_last_check_time();

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_set_error_callback(Callback callback);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_set_can_shutdown_callback(Callback callback);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_set_shutdown_request_callback(Callback callback);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_set_did_find_update_callback(Callback callback);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_set_did_not_find_update_callback(Callback callback);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long win_sparkle_set_update_cancelled_callback(Callback callback);

        // Manual usage
        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_check_update_with_ui();

        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_check_update_with_ui_and_install();

        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void win_sparkle_check_update_without_ui();

    }
}
