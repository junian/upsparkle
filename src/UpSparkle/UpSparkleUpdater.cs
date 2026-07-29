using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UpSparkle.Natives;

namespace UpSparkle
{
    /// <summary>
    /// Cross-platform software updater that wraps the native Sparkle framework on macOS
    /// and WinSparkle on Windows. Create one instance per application and keep it alive
    /// for the lifetime of the process.
    /// </summary>
    public class UpSparkleUpdater: IDisposable
    {
        private readonly INativeSparkle nativeSparkle;

        /// <summary>
        /// Initializes a new instance of <see cref="UpSparkleUpdater"/> using the
        /// platform-appropriate native Sparkle implementation.
        /// </summary>
        public UpSparkleUpdater()
        {
            nativeSparkle = CreateNativeSparkle();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UpSparkleUpdater"/> with an explicit
        /// <see cref="INativeSparkle"/> implementation. Intended for unit testing only.
        /// </summary>
        /// <param name="nativeSparkle">The native Sparkle implementation to use.</param>
        internal UpSparkleUpdater(INativeSparkle nativeSparkle)
        {
            this.nativeSparkle = nativeSparkle ?? throw new ArgumentNullException(nameof(nativeSparkle));
        }

        /// <summary>
        /// The metadata key used to embed the appcast feed URL in an assembly via
        /// <see cref="AssemblyMetadataAttribute"/>.
        /// Matches the macOS <c>Info.plist</c> key used by the Sparkle framework.
        /// </summary>
        /// <example>
        /// In your <c>.csproj</c> (works on .NET Framework and modern .NET):
        /// <code>
        /// &lt;ItemGroup&gt;
        ///   &lt;AssemblyMetadata Include="SUFeedURL" Value="https://example.com/appcast.xml" /&gt;
        /// &lt;/ItemGroup&gt;
        /// </code>
        /// Or in <c>AssemblyInfo.cs</c> for classic .NET Framework projects:
        /// <code>
        /// [assembly: AssemblyMetadata("SUFeedURL", "https://example.com/appcast.xml")]
        /// </code>
        /// </example>
        public const string AppcastUrlMetadataKey = "SUFeedURL";

        /// <summary>
        /// The metadata key used to embed the EdDSA public key in an assembly via
        /// <see cref="AssemblyMetadataAttribute"/>.
        /// Matches the macOS <c>Info.plist</c> key used by the Sparkle framework.
        /// </summary>
        /// <example>
        /// In your <c>.csproj</c> (works on .NET Framework and modern .NET):
        /// <code>
        /// &lt;ItemGroup&gt;
        ///   &lt;AssemblyMetadata Include="SUPublicEDKey" Value="&lt;your-base64-key&gt;" /&gt;
        /// &lt;/ItemGroup&gt;
        /// </code>
        /// Or in <c>AssemblyInfo.cs</c> for classic .NET Framework projects:
        /// <code>
        /// [assembly: AssemblyMetadata("SUPublicEDKey", "&lt;your-base64-key&gt;")]
        /// </code>
        /// </example>
        public const string EdDSAPublicKeyMetadataKey = "SUPublicEDKey";

        /// <summary>
        /// Gets a value indicating whether <see cref="Initialize(string,string,string,string,string)"/>
        /// has been called successfully and the native updater is ready to use.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the appcast URL that was supplied to <see cref="Initialize(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string AppcastUrl { get; private set; }

        /// <summary>
        /// Gets the EdDSA public key that was supplied to <see cref="Initialize(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string EdDSAPublicKey { get; private set; }

        /// <summary>
        /// Gets the company name that was supplied to (or resolved by)
        /// <see cref="Initialize(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string CompanyName { get; private set; }

        /// <summary>
        /// Gets the application name that was supplied to (or resolved by)
        /// <see cref="Initialize(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// Gets the application version that was supplied to (or resolved by)
        /// <see cref="Initialize(string,string,string,string,string)"/>.
        /// Returns <see langword="null"/> before initialization.
        /// </summary>
        public string AppVersion { get; private set; }

        /// <summary>
        /// Initializes the native updater by resolving application details from the supplied
        /// assembly's attributes. The appcast URL and EdDSA public key can be embedded in the
        /// assembly via <see cref="AssemblyMetadataAttribute"/> (set in the <c>.csproj</c> or
        /// <c>AssemblyInfo.cs</c>) and will be used as fallbacks when the corresponding
        /// parameters are <see langword="null"/> or empty.
        /// <para>
        /// Resolution order for <paramref name="appcastUrl"/>:
        /// <list type="number">
        ///   <item>The <paramref name="appcastUrl"/> parameter, if provided.</item>
        ///   <item><see cref="AssemblyMetadataAttribute"/> with key <c>"SUFeedURL"</c>
        ///         (<see cref="AppcastUrlMetadataKey"/>).</item>
        /// </list>
        /// </para>
        /// <para>
        /// Resolution order for <paramref name="edDSAPublicKey"/>:
        /// <list type="number">
        ///   <item>The <paramref name="edDSAPublicKey"/> parameter, if provided.</item>
        ///   <item><see cref="AssemblyMetadataAttribute"/> with key <c>"SUPublicEDKey"</c>
        ///         (<see cref="EdDSAPublicKeyMetadataKey"/>).</item>
        /// </list>
        /// </para>
        /// <para>
        /// Company name, product name, and version are always read from
        /// <see cref="AssemblyCompanyAttribute"/>, <see cref="AssemblyProductAttribute"/>,
        /// and <see cref="AssemblyInformationalVersionAttribute"/> respectively.
        /// Any build-metadata suffix (e.g. <c>+abc123</c>) is stripped from the version.
        /// </para>
        /// </summary>
        /// <param name="assemblyInfo">
        /// The assembly whose attributes provide all required values.
        /// Pass <c>Assembly.GetExecutingAssembly()</c> for the typical case.
        /// </param>
        /// <param name="appcastUrl">
        /// The URL of the appcast XML feed. When <see langword="null"/> or empty, the value
        /// is resolved from an <see cref="AssemblyMetadataAttribute"/> with key
        /// <c>"SUFeedURL"</c> embedded in <paramref name="assemblyInfo"/>.
        /// </param>
        /// <param name="edDSAPublicKey">
        /// The EdDSA public key (Base64-encoded) for verifying update signatures. When
        /// <see langword="null"/> or empty, the value is resolved from an
        /// <see cref="AssemblyMetadataAttribute"/> with key <c>"SUPublicEDKey"</c> embedded
        /// in <paramref name="assemblyInfo"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="assemblyInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a required value cannot be resolved from the parameters or the
        /// assembly's attributes.
        /// </exception>
        public void Initialize(Assembly assemblyInfo, string appcastUrl = null, string edDSAPublicKey = null)
        {
            if (assemblyInfo == null)
                throw new ArgumentNullException(nameof(assemblyInfo));

            // Resolve appcast URL: parameter → assembly metadata fallback
            if (string.IsNullOrWhiteSpace(appcastUrl))
                appcastUrl = GetAssemblyMetadata(assemblyInfo, AppcastUrlMetadataKey);

            // Resolve EdDSA public key: parameter → assembly metadata fallback
            if (string.IsNullOrWhiteSpace(edDSAPublicKey))
                edDSAPublicKey = GetAssemblyMetadata(assemblyInfo, EdDSAPublicKeyMetadataKey);

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

            Initialize(companyName, appName, appVersion, appcastUrl, edDSAPublicKey);
        }

        /// <summary>
        /// Initializes the native updater with the specified application details.
        /// </summary>
        /// <param name="assemblyInfo"></param>
        /// <param name="appcastUrl"></param>
        /// <param name="edDSAPublicKey"></param>
        /// <returns></returns>
        public Task InitializeAsync(Assembly assemblyInfo, string appcastUrl = null, string edDSAPublicKey = null)
        {
            this.Initialize(assemblyInfo, appcastUrl, edDSAPublicKey);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the native updater with the supplied application details and starts
        /// the underlying Sparkle / WinSparkle framework.
        /// </summary>
        /// <param name="companyName">The name of the company or publisher.</param>
        /// <param name="appName">The display name of the application.</param>
        /// <param name="appVersion">
        /// The current version string of the application (e.g. <c>"1.2.3"</c>).
        /// </param>
        /// <param name="appcastUrl">
        /// The URL of the appcast XML feed that the native framework will poll for updates.
        /// </param>
        /// <param name="edDSAPublicKey">
        /// The EdDSA public key (Base64-encoded) used to verify update signatures.
        /// </param>
        private void Initialize(
            string companyName, string appName, string appVersion,
            string appcastUrl, 
            string edDSAPublicKey)
        {
            CompanyName = companyName;
            AppName = appName;
            AppVersion = appVersion;
            nativeSparkle.SetAppDetails(CompanyName, AppName, AppVersion);

            AppcastUrl = appcastUrl;
            nativeSparkle.SetAppcastUrl(AppcastUrl);

            if(!string.IsNullOrWhiteSpace(edDSAPublicKey))
            {
                EdDSAPublicKey = edDSAPublicKey;
                nativeSparkle.SetEdDSAPublicKey(EdDSAPublicKey);
            }

            nativeSparkle.Initialize();
            IsInitialized = true;
        }

        /// <summary>
        /// Opens the native update UI so the user can review and install any available update.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the updater has not been initialized via <see cref="Initialize()"/>.
        /// </exception>
        public void CheckUpdateWithUI()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(UpSparkle)} is not initialized");
            }

            nativeSparkle.CheckUpdateWithUI();
        }

        /// <summary>
        /// Opens the native update UI so the user can review and install any available update.
        /// </summary>
        /// <returns></returns>
        public Task CheckUpdateWithUIAsync()
        {
            this.CheckUpdateWithUI();
            return Task.CompletedTask;
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
        /// Looks up a single <see cref="AssemblyMetadataAttribute"/> value by key.
        /// Returns <see langword="null"/> when no matching attribute is found.
        /// Uses the non-generic <c>GetCustomAttributes</c> API for compatibility with
        /// .NET Framework 4.6.2 targets where the generic overload may not be available.
        /// </summary>
        /// <param name="assembly">The assembly to inspect.</param>
        /// <param name="key">The metadata key to look up.</param>
        /// <returns>The attribute value, or <see langword="null"/> if not found.</returns>
        private static string GetAssemblyMetadata(Assembly assembly, string key)
        {
            var attrs = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            foreach (AssemblyMetadataAttribute attr in attrs)
            {
                if (attr.Key == key)
                    return attr.Value;
            }
            return null;
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
