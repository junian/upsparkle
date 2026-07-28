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

### 2. Initialize the updater

Create an `UpSparkleUpdater` instance once (typically at app startup) and call `Init` with your appcast URL, your EdDSA public key, and the app's assembly:

```csharp
using UpSparkle;

// Create the updater (do this once, e.g. in your main window or app startup)
var updater = new UpSparkleUpdater();

updater.Init(
    appCastUrl: "https://example.com/appcast.xml",
    publicKey:  "MCowBQYDK2VwAyEA<your-eddsa-public-key>",
    assemblyInfo: System.Reflection.Assembly.GetExecutingAssembly());
```

The `assemblyInfo` overload reads the company name, product name, and version directly from your assembly attributes, so you don't need to repeat them manually.

Alternatively, pass the values explicitly:

```csharp
updater.Init(
    appCastUrl:   "https://example.com/appcast.xml",
    publicKey:    "MCowBQYDK2VwAyEA<your-eddsa-public-key>",
    companyName:  "Acme Corp",
    appName:      "My App",
    appVersion:   "1.0.0");
```

### 3. Check for updates

Trigger the native update UI — for example, from a menu item or button:

```csharp
updater.CheckUpdateWithUI();
```

### 4. Clean up on exit

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
            appCastUrl: "https://example.com/appcast.xml",
            publicKey:  "MCowBQYDK2VwAyEA<your-eddsa-public-key>",
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

UpSparkle requires an [appcast XML file](https://sparkle-project.org/documentation/publishing/) hosted at a public URL so the native frameworks can check for new versions. You also need to sign your updates with an EdDSA key pair — see the [Sparkle documentation](https://sparkle-project.org/documentation/eddsa-signatures/) for key generation instructions.

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
