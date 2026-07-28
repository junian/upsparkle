using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using UpSparkle.Natives;

namespace UpSparkle
{
    /// <summary>
    /// Cross-platform software updater that wraps the native Sparkle framework on macOS
    /// and WinSparkle on Windows. Create one instance per application and keep it alive
    /// for the lifetime of the process.
    /// </summary>
    public class UpSparkleUpdater
    {
        private readonly INativeSparkle nativeSparkle = CreateNativeSparkle();

        /// <summary>
        /// Gets a value indicating whether <see cref="Init(string,string,string,string,string)"/>
        /// has been called successfully and the native updater is ready to use.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the appcast URL that was supplied to <see cref="Init(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string AppCastUrl { get; private set; }

        /// <summary>
        /// Gets the EdDSA public key that was supplied to <see cref="Init(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string PublicKey { get; private set; }

        /// <summary>
        /// Gets the company name that was supplied to (or resolved by)
        /// <see cref="Init(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string CompanyName { get; private set; }

        /// <summary>
        /// Gets the application name that was supplied to (or resolved by)
        /// <see cref="Init(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// Gets the application version that was supplied to (or resolved by)
        /// <see cref="Init(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string AppVersion { get; private set; }

        /// <summary>
        /// Initializes the native updater by reading the company name, application name, and
        /// version from the supplied assembly's attributes
        /// (<see cref="AssemblyCompanyAttribute"/>, <see cref="AssemblyProductAttribute"/>,
        /// and <see cref="AssemblyInformationalVersionAttribute"/> / <see cref="AssemblyVersionAttribute"/>).
        /// Any build-metadata suffix (e.g. <c>+abc123</c>) is stripped from the version string
        /// before it is passed to the native layer.
        /// </summary>
        /// <param name="appCastUrl">
        /// The URL of the appcast XML feed that the native framework will poll for updates.
        /// </param>
        /// <param name="publicKey">
        /// The EdDSA public key (Base64-encoded) used to verify update signatures.
        /// </param>
        /// <param name="assemblyInfo">
        /// The assembly whose attributes provide the company name, product name, and version.
        /// Pass <c>Assembly.GetExecutingAssembly()</c> for the typical case.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="assemblyInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the assembly is missing required attributes or version information.
        /// </exception>
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

        /// <summary>
        /// Initializes the native updater with the supplied application details and starts
        /// the underlying Sparkle / WinSparkle framework.
        /// </summary>
        /// <param name="appCastUrl">
        /// The URL of the appcast XML feed that the native framework will poll for updates.
        /// </param>
        /// <param name="publicKey">
        /// The EdDSA public key (Base64-encoded) used to verify update signatures.
        /// </param>
        /// <param name="companyName">The name of the company or publisher.</param>
        /// <param name="appName">The display name of the application.</param>
        /// <param name="appVersion">
        /// The current version string of the application (e.g. <c>"1.2.3"</c>).
        /// </param>
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
        /// Opens the native update UI so the user can review and install any available update.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the updater has not been initialized via <see cref="Init(string,string,string,string,string)"/>.
        /// </exception>
        public virtual void CheckUpdateWithUI()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(UpSparkle)} is not initialized");
            }

            nativeSparkle.CheckUpdateWithUI();
        }

        /// <summary>
        /// Shuts down the native updater and releases any native resources it holds.
        /// Call this when the application is closing to ensure a clean exit.
        /// After disposal, <see cref="IsInitialized"/> is set back to <see langword="false"/>.
        /// </summary>
        public virtual void Dispose()
        {
            nativeSparkle.Dispose();
            IsInitialized = false;
        }

        /// <summary>
        /// Factory method that creates the correct <see cref="INativeSparkle"/> implementation
        /// for the current operating system — <see cref="WinSparkle"/> on Windows and
        /// <see cref="MacSparkle"/> on macOS / Mac Catalyst.
        /// </summary>
        /// <returns>A platform-specific <see cref="INativeSparkle"/> instance.</returns>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown on any platform other than Windows and macOS.
        /// </exception>
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
