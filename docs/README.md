<div align="center">

# UpSparkle

Thin wrapper of native updater framework for .NET desktop apps. It uses Sparkle for macOS and WinSparkle for Windows.

[![NuGet](https://img.shields.io/nuget/v/Upsparkle.svg?style=for-the-badge)](https://www.nuget.org/packages/Upsparkle/)
[![NuGet](https://img.shields.io/nuget/dt/Upsparkle.svg?style=for-the-badge)](https://www.nuget.org/packages/Upsparkle/)

</div>

## About

Cross-platform updater for .NET desktop apps. This library ships as a single NuGet package and uses platform-specific native implementations under the hood:

- On Windows, via wrapping [WinSparkle](https://winsparkle.org).
- On macOS, via wrapping [Sparkle](https://sparkle-project.org).

## Quickstart

### 1. Install the NuGet package

```bash
dotnet add package Upsparkle
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package Upsparkle
```

### 2. Configure your project

The `Init` call reads your app's metadata from the executing assembly, so make sure the relevant fields are populated before calling it.

#### Windows — set assembly metadata in your `.csproj`

```xml
<PropertyGroup>
  <Company>Acme Corp</Company>
  <Product>My App</Product>
  <Version>1.0.0</Version>
</PropertyGroup>
```

These map to `AssemblyCompanyAttribute`, `AssemblyProductAttribute`, and `AssemblyInformationalVersionAttribute` respectively, which UpSparkle reads at runtime.

#### macOS — add Sparkle keys to your `Info.plist`

On macOS, Sparkle reads its configuration directly from the app bundle's `Info.plist`. At minimum you need:

```xml
<!-- Required: where Sparkle checks for updates -->
<key>SUFeedURL</key>
<string>https://example.com/appcast.xml</string>

<!-- Required: EdDSA public key for verifying update signatures -->
<key>SUPublicEDKey</key>
<string>pfIShU4dEXqPd5ObYNfDBiQWcXozk7estwzTnF9BamQ=</string>
```

A few commonly used optional keys:

```xml
<!-- Skip the "can we check automatically?" permission prompt on second launch -->
<key>SUEnableAutomaticChecks</key>
<true/>

<!-- How often to check for updates, in seconds (default: 86400 = 1 day) -->
<key>SUScheduledCheckInterval</key>
<integer>86400</integer>

<!-- Silently download and install updates in the background (default: NO) -->
<key>SUAutomaticallyUpdate</key>
<false/>

<!-- Hide release notes in the update alert (default: YES = shown) -->
<key>SUShowReleaseNotes</key>
<true/>
```

For the full list of supported keys — including security, sandboxing, and system profiling options — see the [Sparkle customization docs](https://sparkle-project.org/documentation/customization/).

To generate your EdDSA key pair, use the `generate_keys` tool that ships with Sparkle. See the [EdDSA signatures guide](https://sparkle-project.org/documentation/eddsa-signatures/) for step-by-step instructions.

### 3. Initialize the updater

Create an `UpSparkleUpdater` instance once (typically at app startup) and call `Init` with your appcast URL, your EdDSA public key, and the executing assembly:

```csharp
using UpSparkle;

// Create the updater (do this once, e.g. in your main window or app startup)
var updater = new UpSparkleUpdater();

updater.Init(
    appCastUrl:   "https://example.com/appcast.xml",
    publicKey:    "<your-base64-eddsa-public-key>",
    assemblyInfo: System.Reflection.Assembly.GetExecutingAssembly());
```

`Init` reads the company name, product name, and version directly from the assembly attributes set in step 2, so you don't need to repeat them in code.

### 4. Check for updates

Trigger the native update UI — for example, from a menu item or button:

```csharp
updater.CheckUpdateWithUI();
```

### 5. Clean up on exit

Dispose the updater when the application closes to release native resources:

```csharp
updater.Dispose();
```

### Full WPF example

```csharp
using System.Windows;
using UpSparkle;

public partial class MainWindow : Window
{
    private readonly UpSparkleUpdater _updater = new UpSparkleUpdater();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _updater.Init(
            appCastUrl:   "https://example.com/appcast.xml",
            publicKey:    "<your-base64-eddsa-public-key>",
            assemblyInfo: System.Reflection.Assembly.GetExecutingAssembly());
    }

    private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        _updater.CheckUpdateWithUI();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _updater.Dispose();
    }
}
```

## Supported Platforms

For macOS or Mac Catalyst:

- `net6.0` or later
- `net6.0-macos` or later
- `net6.0-maccatalyst` or later

For Windows:

- `net462` (.NET Framework 4.6.2) or later
- `net6.0` or later
- `net6.0-windows` or later

Currently, this library supports the following platforms:

1. macOS (Apple Silicon and Intel)
2. Mac Catalyst (Apple Silicon and Intel)
3. Windows 10 and 11 (Arm64, x64, and x86)

Tested with the following .NET project types:

### Windows

Works with .NET Framework 4.6.2 or later and modern .NET:

- WinForms
- WPF
- WinUI
- AvaloniaUI
- MAUI (WinUI)

### macOS

Works with modern .NET only:

- .NET macOS
- MAUI (Mac Catalyst)
- Avalonia UI
- Uno Platform

## Appcast & Public Key

UpSparkle requires an [appcast XML file](https://sparkle-project.org/documentation/publishing/) hosted at a public URL. Updates must be signed with an EdDSA key pair — use the `generate_keys` tool that ships with Sparkle and follow the [EdDSA signatures guide](https://sparkle-project.org/documentation/eddsa-signatures/) to generate your keys and sign your releases.

## Development

Before starting development, install 3rd-party dependencies by running the appropriate script for your platform.

**macOS / Linux**

```bash
$ ./getlibs.sh
```

**Windows (PowerShell)**

```powershell
PS> .\Get-Libraries.ps1
```

Both scripts download and extract files based on `.gitbinmodules` content and place them under the `libs` directory.

To use a different version of Sparkle or WinSparkle binaries, edit `.gitbinmodules` and update the desired version.

## Credits

- [sparkle-project/Sparkle](https://github.com/sparkle-project/Sparkle) for the macOS native framework.
- [vslavik/winsparkle](https://github.com/vslavik/winsparkle) for the Windows native implementation.

## License

This project is licensed under the [MIT License](https://github.com/junian/upsparkle/blob/master/LICENSE).
