# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2026-11-13

### Added

- Automatic update settings on both platforms:
  - `IsAutomaticCheckForUpdates` — enable or disable scheduled update checks.
  - `UpdateCheckInterval` — polling interval in seconds.
  - `LastCheckTime` — UTC timestamp of the last check, or `null` when updates have never been checked.
- `CheckUpdateWithoutUI()` — triggers a background update check without the progress UI.
- `SetHttpHeader(string name, string value)` and `ClearHttpHeaders()` — attach custom HTTP headers to appcast checks, release-note downloads, and update downloads.
- `Error` event — raised when the native updater reports a failure. The normal "no update found" outcome and user-cancelled installations are not reported.
- Internal `UpSparkleSettings` class containing the Sparkle `SU*` configuration keys.
- Eto Forms demo covering the Gtk, macOS, and WPF backends.
- `scripts/update_native_versions.cs` and a daily `check-native-updates.yml` workflow that opens a pull request when a newer Sparkle, libMacSparkle, or WinSparkle release is available.
- NuGet package icon.

### Changed

- Updated native dependencies:
  - Sparkle 2.9.4 → 2.10.1
  - libMacSparkle 1.0.9 → 1.2.0 (adds HTTP header and error-callback support)
  - WinSparkle 0.9.3 → 0.9.4
- Moved the demo projects from `src/Demos/` to the top-level `examples/` directory.
- Expanded the demo applications with automatic-check, interval, last-check, HTTP-header, and background-check controls.
- Expanded the test suite from 30 to 36 tests.
- Updated the README API reference to document the complete public surface.

### Fixed

- The `Error` event no longer fires for "no update found" or user-cancelled installations.
- Native callbacks are safe under Mono AOT / IL2CPP via `MonoPInvokeCallback`.

## [1.0.6] - 2026-07-30

### Added

- `UpSparkleUpdater` — single public class providing a unified API for both Windows and macOS.
- `Initialize(Assembly, string, string)` — resolves company name, app name, and version from standard assembly attributes (`AssemblyCompanyAttribute`, `AssemblyProductAttribute`, `AssemblyInformationalVersionAttribute`). Appcast URL and EdDSA public key are read from `AssemblyMetadataAttribute` entries with keys `SUFeedURL` and `SUPublicEDKey`, with optional parameter overrides.
- `InitializeAsync(Assembly, string, string)` — async counterpart of `Initialize` for use in async startup paths.
- `CheckUpdateWithUI()` — opens the native Sparkle / WinSparkle update dialog.
- `CheckUpdateWithUIAsync()` — async counterpart of `CheckUpdateWithUI`.
- `Dispose()` — shuts down the native updater and releases native resources; resets `IsInitialized` to `false`.
- Properties: `IsInitialized`, `AppcastUrl`, `EdDSAPublicKey`, `CompanyName`, `AppName`, `AppVersion` — all populated after a successful `Initialize` call.
- Constants `AppcastUrlMetadataKey` (`"SUFeedURL"`) and `EdDSAPublicKeyMetadataKey` (`"SUPublicEDKey"`) matching the Sparkle `Info.plist` key names.
- Build-metadata suffix stripping from `AssemblyInformationalVersionAttribute` (e.g. `1.0.0+abc123` → `1.0.0`).
- Windows native backend (`WinSparkle`) wrapping WinSparkle 0.9.3 via dynamic P/Invoke with automatic architecture detection (x86, x64, Arm64).
- macOS / Mac Catalyst native backend (`MacSparkle`) wrapping Sparkle 2.9.4 via `libMacSparkle` 1.0.9 and `dlopen`-based dynamic loading.
- NuGet package targeting `netstandard2.0`, compatible with .NET Framework 4.6.2+ and .NET 6+.
- Bundled native binaries for `win-x86`, `win-x64`, `win-arm64`, and `osx` using the standard NuGet `runtimes/` layout — no manual DLL copying required.
- Demo projects for WPF, WinForms, Avalonia UI, .NET MAUI, macOS (.NET), and Mac Catalyst.
- `UpSparkle.Tests` MSTest project with 30 unit tests covering all public members of `UpSparkleUpdater`.
- PowerShell (`Get-Libraries.ps1`) and shell (`getlibs.sh`) scripts to fetch and extract native binaries from their upstream releases.

### Changed

- Updated project, package, and assembly versioning for the stable `1.0.6` release.
- Refined Sparkle framework extraction and macOS app bundle integration for smoother runtime packaging.
- Improved NuGet packaging metadata and build asset handling for runtime-native Sparkle/WinSparkle assets.
- Ensured package release notes are sourced from `ReleaseNotes.txt` for NuGet distribution.
- Consolidated previously separate `UpSparkle.Mac` and `UpSparkle.Windows` projects into a single `netstandard2.0` library with runtime-conditional platform dispatch.
- Native initialization refactored into discrete configuration steps (`SetAppDetails`, `SetAppcastUrl`, `SetEdDSAPublicKey`, `Initialize`) to align both backends behind a clean `INativeSparkle` interface.
- Native library loading moved to a dynamic `LoadLibrary` / `dlopen` approach, replacing static `[DllImport]` declarations, to support the NuGet `runtimes/` layout without requiring a build-time reference.
- `runtimes/` directory structure adopted to follow standard .NET NuGet native-asset conventions.

[1.1.1]: https://github.com/junian/upsparkle/releases/tag/v1.1.1
[1.0.6]: https://github.com/junian/upsparkle/releases/tag/v1.0.6
