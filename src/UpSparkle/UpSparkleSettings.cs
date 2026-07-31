namespace UpSparkle
{
    /// <summary>
    /// Keys for the Sparkle update framework settings as documented at
    /// https://sparkle-project.org/documentation/customization/
    /// These keys are read from the app's <c>Info.plist</c> on macOS and the
    /// application's user defaults on Windows (WinSparkle).
    /// </summary>
    internal static class UpSparkleSettings
    {
        /// <summary>
        /// The URL of your appcast, e.g. <c>https://example.com/appcast.xml</c>.
        /// </summary>
        public const string SUFeedURL = nameof(SUFeedURL);

        /// <summary>
        /// The base64-encoded public EdDSA key.
        /// </summary>
        public const string SUPublicEDKey = nameof(SUPublicEDKey);

        /// <summary>
        /// Enables or disables automatic checking for updates by default.
        /// </summary>
        public const string SUEnableAutomaticChecks = nameof(SUEnableAutomaticChecks);

        /// <summary>
        /// The number of seconds between automatic update checks.
        /// </summary>
        public const string SUScheduledCheckInterval = nameof(SUScheduledCheckInterval);

        /// <summary>
        /// Enables automatic download and installation of updates by default.
        /// </summary>
        public const string SUAutomaticallyUpdate = nameof(SUAutomaticallyUpdate);

        /// <summary>
        /// The number of seconds between update checks after an update is set to install in the background.
        /// </summary>
        public const string SUScheduledImpatientCheckInterval = nameof(SUScheduledImpatientCheckInterval);

        /// <summary>
        /// Controls whether automatic updates are allowed.
        /// </summary>
        public const string SUAllowsAutomaticUpdates = nameof(SUAllowsAutomaticUpdates);

        /// <summary>
        /// Enables anonymous system profiling.
        /// </summary>
        public const string SUEnableSystemProfiling = nameof(SUEnableSystemProfiling);

        /// <summary>
        /// Controls whether release notes are displayed in the update alert.
        /// </summary>
        public const string SUShowReleaseNotes = nameof(SUShowReleaseNotes);

        /// <summary>
        /// Optional alternative bundle display name.
        /// </summary>
        public const string SUBundleName = nameof(SUBundleName);

        /// <summary>
        /// Optional alternative <c>NSUserDefaults</c> domain name.
        /// </summary>
        public const string SUDefaultsDomain = nameof(SUDefaultsDomain);

        /// <summary>
        /// Re-launches the host targeted bundle instead of the application bundle (for plug-ins).
        /// </summary>
        public const string SURelaunchHostBundle = nameof(SURelaunchHostBundle);

        /// <summary>
        /// Forces verification of updates before Sparkle extracts the downloaded update.
        /// </summary>
        public const string SUVerifyUpdateBeforeExtraction = nameof(SUVerifyUpdateBeforeExtraction);

        /// <summary>
        /// Makes Sparkle validate that appcasts and release notes are signed.
        /// </summary>
        public const string SURequireSignedFeed = nameof(SURequireSignedFeed);

        /// <summary>
        /// The number of seconds it takes for a feed signing validation failure to expire.
        /// </summary>
        public const string SUSignedFeedFailureExpirationInterval = nameof(SUSignedFeedFailureExpirationInterval);

        /// <summary>
        /// Custom URL schemes allowed to be clicked from Sparkle's release notes view.
        /// </summary>
        public const string SUAllowedURLSchemes = nameof(SUAllowedURLSchemes);

        /// <summary>
        /// Allows JavaScript in the release notes.
        /// </summary>
        public const string SUEnableJavaScript = nameof(SUEnableJavaScript);

        /// <summary>
        /// Enables using the Installer Launcher XPC Service (required for sandboxed apps).
        /// </summary>
        public const string SUEnableInstallerLauncherService = nameof(SUEnableInstallerLauncherService);

        /// <summary>
        /// Enables using the Downloader XPC Service.
        /// </summary>
        public const string SUEnableDownloaderService = nameof(SUEnableDownloaderService);

        /// <summary>
        /// Enables using the Installer Connection Service.
        /// </summary>
        public const string SUEnableInstallerConnectionService = nameof(SUEnableInstallerConnectionService);

        /// <summary>
        /// Enables using the Installer Status Service.
        /// </summary>
        public const string SUEnableInstallerStatusService = nameof(SUEnableInstallerStatusService);
    }
}
