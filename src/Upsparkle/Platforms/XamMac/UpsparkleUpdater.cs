using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Juniansoft.Upsparkle
{
    public class UpsparkleUpdater : IUpsparkleUpdater
    {
        public bool AutomaticCheckForUpdates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeSpan UpdateCheckInterval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime LastCheckTime => throw new NotImplementedException();

        public event EventHandler<EventArgs> Error;
        public event EventHandler<EventArgs> CanShutdown;
        public event EventHandler<EventArgs> ShutdownRequest;
        public event EventHandler<EventArgs> DidFindUpdate;
        public event EventHandler<EventArgs> DidNotFindUpdate;
        public event EventHandler<EventArgs> UpdateCancelled;

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
    }
}