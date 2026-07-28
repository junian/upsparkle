using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using UpSparkle.Natives;

namespace UpSparkle
{
    public class UpSparkleUpdater
    {
        private readonly INativeSparkle nativeSparkle = CreateNativeSparkle();

        public bool IsInitialized { get; private set; }
        public string AppCastUrl { get; private set; }
        public string PublicKey { get; private set; }
        public string CompanyName { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }

        public virtual void Init(string appCastUrl, string publicKey, Assembly assemblyInfo)
        {
            if(assemblyInfo == null)
                throw new ArgumentNullException(nameof(assemblyInfo));

            var companyName = assemblyInfo.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company
                              ?? throw new ArgumentException("Assembly is missing AssemblyCompanyAttribute.",
                                  nameof(assemblyInfo));

            var appName = assemblyInfo.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                          ?? assemblyInfo.GetName().Name
                          ?? throw new ArgumentException("Assembly has no product name or assembly name.",
                              nameof(assemblyInfo));

            var appVersion = assemblyInfo.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                 ?.InformationalVersion
                             ?? assemblyInfo.GetName().Version?.ToString()
                             ?? throw new ArgumentException("Assembly has no version information.",
                                 nameof(assemblyInfo));

            // Strip any build metadata suffix (e.g. "1.0.0+abc123" -> "1.0.0")
            var plusIndex = appVersion.IndexOf('+');
            if (plusIndex >= 0)
                appVersion = appVersion.Substring(0, plusIndex);

            Init(appCastUrl, publicKey, companyName, appName, appVersion);
        }

        public virtual void Init(string appCastUrl, string publicKey, string companyName, string appName,
            string appVersion)
        {
            AppCastUrl = appCastUrl;
            PublicKey = publicKey;
            CompanyName = companyName;
            AppName = appName;
            AppVersion = appVersion;

            nativeSparkle.Init(appCastUrl, publicKey, companyName, appName, appVersion);
            IsInitialized = true;
        }

        /// <summary>
        /// Check Update with native UI
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void CheckUpdateWithUI()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(UpSparkle)} is not initialized");
            }

            nativeSparkle.CheckUpdateWithUI();
        }

        /// <summary>
        /// Dispose the native object
        /// </summary>
        public virtual void Dispose()
        {
            nativeSparkle.Dispose();
            IsInitialized = false;
        }

        /// <summary>
        /// Create native Sparkle object based on Operating system
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        private static INativeSparkle CreateNativeSparkle()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WinSparkle();
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) 
                || RuntimeInformation.OSDescription.StartsWith("Mac Catalyst"))
            {
                return new MacSparkle();
            }

            throw new PlatformNotSupportedException("UpSparkle is only supported on Windows and macOS.");
        }
    }
}
