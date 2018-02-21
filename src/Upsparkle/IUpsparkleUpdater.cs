using System;
using System.Collections.Generic;
using System.Text;

namespace Juniansoft.Upsparkle
{
    public delegate void Callback();

    internal interface IUpsparkleUpdater
    {
        void Init();
        void Cleanup();
        void SetLang(string lang);
        void SetLangId(ushort lang);
        void SetAppcastUrl(string url);
        void SetAppDetails(string companyName,
            string appName,
            string appVersion);
        void SetAppBuildVersion(string build);
        void SetRegistryPath(string path);
        bool AutomaticCheckForUpdates { get; set; }
        TimeSpan UpdateCheckInterval { get; set; }
        DateTime LastCheckTime { get; }

        void SetErrorCallback(Callback callback);
        void SetCanShutdownCallback(Callback callback);
        void SetShutdownRequestCallback(Callback callback);
        void SetDidFindUpdateCallback(Callback callback);
        void SetDidNotFindUpdateCallback(Callback callback);
        void SetUpdateCancelledCallback(Callback callback);

        void CheckUpdateWithUI();
        void CheckUpdateWithUIAndInstall();
        void CheckUpdateWithoutUI();
    }
}
