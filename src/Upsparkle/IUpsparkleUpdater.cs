using System;
using System.Collections.Generic;
using System.Text;

namespace Juniansoft.Upsparkle
{
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

        event EventHandler<EventArgs> Error;
        event EventHandler<EventArgs> CanShutdown;
        event EventHandler<EventArgs> ShutdownRequest;
        event EventHandler<EventArgs> DidFindUpdate;
        event EventHandler<EventArgs> DidNotFindUpdate;
        event EventHandler<EventArgs> UpdateCancelled;

        void CheckUpdateWithUI();
        void CheckUpdateWithUIAndInstall();
        void CheckUpdateWithoutUI();
    }
}
