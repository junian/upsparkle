﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Juniansoft.Upsparkle
{
    public class UpsparkleUpdater: IUpsparkleUpdater
    {
        private UpsparkleUpdater()
        {
            var dllpath = string.Empty;

            if (IntPtr.Size == 4)
            {
                // 32-bit
                //Utilites.LoadUnmanagedLibraryFromResource(
                //    Assembly.GetExecutingAssembly(),
                //    "WinSparkleDotNet.x86.WinSparkle.dll",
                //    "WinSparkle.dll"
                //    );
                dllpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x86\\WinSparkle.dll");
            }
            else
            {
                // 64-bit
                //Utilites.LoadUnmanagedLibraryFromResource(
                //    Assembly.GetExecutingAssembly(),
                //    "WinSparkleDotNet.x64.WinSparkle.dll",
                //    "WinSparkle.dll"
                //    );
                dllpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64\\WinSparkle.dll");
            }

            Debug.WriteLine("DLL Path: " + dllpath);
            Utilites.LoadUnmanagedLibrary(dllpath);
        }

        private static UpsparkleUpdater _current;
        public static UpsparkleUpdater Current
        {
            get { return _current ?? (_current = new UpsparkleUpdater()); }
        }

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

        public void SetErrorCallback(Callback callback)
        {
            win_sparkle_set_error_callback(callback);
        }

        public void SetCanShutdownCallback(Callback callback)
        {
            win_sparkle_set_can_shutdown_callback(callback);
        }

        public void SetShutdownRequestCallback(Callback callback)
        {
            win_sparkle_set_shutdown_request_callback(callback);
        }

        public void SetDidFindUpdateCallback(Callback callback)
        {
            win_sparkle_set_did_find_update_callback(callback);
        }

        public void SetDidNotFindUpdateCallback(Callback callback)
        {
            win_sparkle_set_did_not_find_update_callback(callback);
        }

        public void SetUpdateCancelledCallback(Callback callback)
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