using System;

namespace Juniansoft.Upsparkle
{
    public static class AppUpdater
    {
        public static IUpsparkleUpdater Current
        {
            get
            {
                IUpsparkleUpdater ret = GetInstance();
                if (ret == null)
                {
                    new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
                }
                return ret;
            }
        }

        private static IUpsparkleUpdater _instance = null;
        private static IUpsparkleUpdater GetInstance()
        {
#if PORTABLE || NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
            if(_instance == null)
                _instance = new UpsparkleUpdater();
            return _instance;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        }
    }
}
