using System;
using System.Collections.Generic;
using System.Text;

namespace Juniansoft.Upsparkle
{
    public class UpsparkleUpdater : IUpsparkleUpdater
    {
        public bool AutomaticCheckForUpdates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeSpan UpdateCheckInterval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime LastCheckTime => throw new NotImplementedException();

        public void CheckUpdateWithoutUI()
        {
            throw new NotImplementedException();
        }

        public void CheckUpdateWithUI()
        {
            throw new NotImplementedException();
        }

        public void CheckUpdateWithUIAndInstall()
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void SetAppBuildVersion(string build)
        {
            throw new NotImplementedException();
        }

        public void SetAppcastUrl(string url)
        {
            throw new NotImplementedException();
        }

        public void SetAppDetails(string companyName, string appName, string appVersion)
        {
            throw new NotImplementedException();
        }

        public void SetCanShutdownCallback(Callback callback)
        {
            throw new NotImplementedException();
        }

        public void SetDidFindUpdateCallback(Callback callback)
        {
            throw new NotImplementedException();
        }

        public void SetDidNotFindUpdateCallback(Callback callback)
        {
            throw new NotImplementedException();
        }

        public void SetErrorCallback(Callback callback)
        {
            throw new NotImplementedException();
        }

        public void SetLang(string lang)
        {
            throw new NotImplementedException();
        }

        public void SetLangId(ushort lang)
        {
            throw new NotImplementedException();
        }

        public void SetRegistryPath(string path)
        {
            throw new NotImplementedException();
        }

        public void SetShutdownRequestCallback(Callback callback)
        {
            throw new NotImplementedException();
        }

        public void SetUpdateCancelledCallback(Callback callback)
        {
            throw new NotImplementedException();
        }
    }
}
