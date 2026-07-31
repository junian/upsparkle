<div align="center">

# UpSparkle

Thin .NET wrapper around [WinSparkle](https://winsparkle.org) (Windows) and [Sparkle](https://sparkle-project.org) (macOS). One NuGet package, two platforms.

[![NuGet](https://img.shields.io/nuget/v/UpSparkle.svg?style=for-the-badge)](https://www.nuget.org/packages/UpSparkle/)
[![NuGet](https://img.shields.io/nuget/dt/UpSparkle.svg?style=for-the-badge)](https://www.nuget.org/packages/UpSparkle/)
[![Buy me a coffee](https://img.shields.io/badge/Support-Buy%20Me%20A%20Coffee-FFDD00?logo=buymeacoffee&style=for-the-badge "Buy me a coffee")](https://www.junian.dev/coffee/)

</div>

## About

UpSparkle gives .NET desktop apps a cross-platform auto-update UI without any platform-specific plumbing code. It ships a single `netstandard2.0` NuGet package that automatically picks the right native binary at runtime.

- **Windows** — wraps [WinSparkle](https://winsparkle.org)
- **macOS / Mac Catalyst** — wraps [Sparkle](https://sparkle-project.org)

## Quickstart

### 1. Install the NuGet package

```bash
dotnet add package UpSparkle
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package UpSparkle
```

### 2. Configure your project

`Initialize` reads your app's metadata from the executing assembly, so you need to set those values in your project first.

#### Windows — SDK-style project (`.csproj`)

Add company, product, and version to your `<PropertyGroup>`, then declare the UpSparkle-specific metadata in an `<ItemGroup>`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Company>Wayne Enterprise</Company>
    <Product>BatComputer App</Product>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <!-- Required: URL of your appcast XML feed -->
    <AssemblyMetadata Include="SUFeedURL" Value="https://example.com/appcast.xml" />
    <!-- Required: Base64-encoded EdDSA public key for verifying update signatures -->
    <AssemblyMetadata Include="SUPublicEDKey" Value="<your-base64-eddsa-public-key>" />
  </ItemGroup>
</Project>
```

These map to `AssemblyCompanyAttribute`, `AssemblyProductAttribute`, `AssemblyInformationalVersionAttribute`, and `AssemblyMetadataAttribute` respectively — all of which `Initialize` reads at runtime.

#### Windows — classic .NET Framework project (`AssemblyInfo.cs`)

For non-SDK-style projects (e.g. .NET Framework 4.6.2 targeting `net462` without `<Project Sdk="...">`), the `<ItemGroup>/<AssemblyMetadata>` shorthand is not available. Add the attributes directly to your `Properties\AssemblyInfo.cs` instead:

```csharp
using System.Reflection;

// Standard assembly identity attributes
[assembly: AssemblyCompany("Wayne Enterprise")]
[assembly: AssemblyProduct("BatComputer App")]
[assembly: AssemblyInformationalVersion("1.0.0")]

// UpSparkle-specific metadata
[assembly: AssemblyMetadata("SUFeedURL", "https://example.com/appcast.xml")]
[assembly: AssemblyMetadata("SUPublicEDKey", "<your-base64-eddsa-public-key>")]
```

> `AssemblyMetadata` is in `System.Reflection` and is available in .NET Framework 4.5+.

#### macOS — `Info.plist`

On macOS, Sparkle reads its configuration directly from the app bundle's `Info.plist`. At minimum you need:

```xml
<!-- Required: where Sparkle checks for updates -->
<key>SUFeedURL</key>
<string>https://example.com/appcast.xml</string>

<!-- Required: EdDSA public key for verifying update signatures -->
<key>SUPublicEDKey</key>
<string><your-base64-eddsa-public-key></string>
```

Commonly used optional keys:

```xml
<!-- Skip the automatic-check permission prompt on second launch -->
<key>SUEnableAutomaticChecks</key>
<true/>

<!-- How often to poll for updates, in seconds (default: 86400 = 1 day) -->
<key>SUScheduledCheckInterval</key>
<integer>86400</integer>

<!-- Silently download and install in the background -->
<key>SUAutomaticallyUpdate</key>
<false/>

<!-- Show release notes in the update alert -->
<key>SUShowReleaseNotes</key>
<true/>
```

For the full list of supported keys see the [Sparkle customization docs](https://sparkle-project.org/documentation/customization/).

To generate your EdDSA key pair, use the `generate_keys` tool that ships with Sparkle. See the [EdDSA signatures guide](https://sparkle-project.org/documentation/eddsa-signatures/) for step-by-step instructions.

### 3. Initialize the updater

Create one `UpSparkleUpdater` instance per application (typically at startup) and call `Initialize`. The simplest form reads everything from the executing assembly:

```csharp
using UpSparkle;

var updater = new UpSparkleUpdater();

// Reads SUFeedURL and SUPublicEDKey from AssemblyMetadata,
// and company/product/version from standard assembly attributes.
updater.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
```

You can also pass the appcast URL and/or public key directly — they take precedence over the assembly metadata:

```csharp
updater.Initialize(
    assemblyInfo:   System.Reflection.Assembly.GetExecutingAssembly(),
    appcastUrl:     "https://example.com/appcast.xml",
    edDSAPublicKey: "<your-base64-eddsa-public-key>");
```

After a successful call, the following properties are available:

| Property | Description |
|----------|-------------|
| `IsInitialized` | `true` once `Initialize` has succeeded |
| `AppcastUrl` | The resolved appcast feed URL |
| `EdDSAPublicKey` | The resolved EdDSA public key |
| `CompanyName` | Resolved from `AssemblyCompanyAttribute` |
| `AppName` | Resolved from `AssemblyProductAttribute` |
| `AppVersion` | Resolved from `AssemblyInformationalVersionAttribute` (build metadata suffix stripped) |

### 4. Check for updates

Call `CheckUpdateWithUI()` to open the native update dialog — wire this to a menu item, toolbar button, or call it automatically on startup:

```csharp
updater.CheckUpdateWithUI();
```

> `CheckUpdateWithUI` throws `InvalidOperationException` if called before `Initialize`.

### 5. Clean up on exit

Dispose the updater when the application closes to release native resources:

```csharp
updater.Dispose();
```

After disposal, `IsInitialized` is reset to `false`.

## Full Examples

### WPF

```csharp
using System.Windows;
using UpSparkle;

public partial class MainWindow : Window
{
    private readonly UpSparkleUpdater _updater = new UpSparkleUpdater();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _updater.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
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

### WinForms

```csharp
using System.Windows.Forms;
using UpSparkle;

public partial class Form1 : Form
{
    private readonly UpSparkleUpdater _updater = new UpSparkleUpdater();

    private void Form1_Load(object sender, EventArgs e)
    {
        _updater.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
    }

    private void btnCheckUpdate_Click(object sender, EventArgs e)
    {
        _updater.CheckUpdateWithUI();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        _updater.Dispose();
    }
}
```

### Avalonia (MVVM)

```csharp
using System.Reflection;
using UpSparkle;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly UpSparkleUpdater _updater = new UpSparkleUpdater();

    public void Init()
    {
        _updater.Initialize(Assembly.GetExecutingAssembly());

        // Properties are populated after Initialize
        Console.WriteLine(_updater.CompanyName);
        Console.WriteLine(_updater.AppName);
        Console.WriteLine(_updater.AppVersion);
    }

    public void CheckForUpdates()
    {
        _updater.CheckUpdateWithUI();
    }
}
```

### MAUI

```csharp
using System.Reflection;
using UpSparkle;

public partial class MainPage : ContentPage
{
    private readonly UpSparkleUpdater _updater = new UpSparkleUpdater();

    public MainPage()
    {
        InitializeComponent();

        this.Loaded += (_, _) =>
            _updater.Initialize(Assembly.GetExecutingAssembly());

        this.Disappearing += (_, _) =>
            _updater.Dispose();
    }

    private void OnCheckUpdatesClicked(object sender, EventArgs e)
    {
        _updater.CheckUpdateWithUI();
    }
}
```

## API Reference

### `UpSparkleUpdater`

```csharp
public class UpSparkleUpdater : IDisposable
```

#### Constructor

```csharp
new UpSparkleUpdater()
```

Creates a new updater instance. The native backend is not started until `Initialize` is called.

#### Methods

```csharp
void Initialize(Assembly assemblyInfo, string appcastUrl = null, string edDSAPublicKey = null)
```

Starts the native Sparkle / WinSparkle framework. Reads company name, app name, and version from the assembly's standard attributes. `appcastUrl` and `edDSAPublicKey` are resolved from parameters first, then from `AssemblyMetadata` entries with keys `"SUFeedURL"` and `"SUPublicEDKey"` respectively.

Throws `ArgumentNullException` if `assemblyInfo` is null. Throws `ArgumentException` if a required value cannot be resolved.

```csharp
void CheckUpdateWithUI()
```

Opens the native update UI. Throws `InvalidOperationException` if called before `Initialize`.

```csharp
void Dispose()
```

Shuts down the native updater and releases native resources. Resets `IsInitialized` to `false`.

#### Properties

```csharp
bool IsInitialized { get; }
string AppcastUrl { get; }
string EdDSAPublicKey { get; }
string CompanyName { get; }
string AppName { get; }
string AppVersion { get; }
```

All properties return `null` before `Initialize` is called.

#### Constants

```csharp
const string SUFeedURL = nameof(SUFeedURL);
const string SUPublicEDKey = nameof(SUPublicEDKey);
```

Keys used to look up `AssemblyMetadata` values from the assembly. Match the corresponding `Info.plist` keys on macOS. These live in the internal `UpSparkle.UpSparkleSettings` class.

## Supported Platforms

| Platform | Minimum version |
|----------|-----------------|
| Windows (x86 / x64 / Arm64) | .NET Framework 4.6.2, or .NET 6+ |
| macOS (Apple Silicon / Intel) | .NET 6+ |
| Mac Catalyst (Apple Silicon / Intel) | .NET 6+ |

Tested project types:

**Windows** — WinForms, WPF, WinUI, Avalonia UI, MAUI (WinUI)

**macOS** — .NET macOS, MAUI (Mac Catalyst), Avalonia UI, Uno Platform

## Appcast & Signing

UpSparkle requires an [appcast XML feed](https://sparkle-project.org/documentation/publishing/) hosted at a public URL. Every update package must be signed with an EdDSA key pair.

Generate your keys with the `generate_keys` tool that ships with Sparkle, then follow the [EdDSA signatures guide](https://sparkle-project.org/documentation/eddsa-signatures/) to sign your releases.

## Development

Before starting development, fetch the native dependencies by running the script for your platform.

**macOS / Linux**

```bash
./getlibs.sh
```

**Windows (PowerShell)**

```powershell
.\Get-Libraries.ps1
```

Both scripts download and extract binaries based on `.gitbinmodules` into the `libs` directory. To use a different version of Sparkle or WinSparkle, edit `.gitbinmodules` and re-run the script.

## Credits

- [sparkle-project/Sparkle](https://github.com/sparkle-project/Sparkle) — macOS native framework
- [vslavik/winsparkle](https://github.com/vslavik/winsparkle) — Windows native implementation

## License

[MIT License](https://github.com/junian/upsparkle/blob/master/LICENSE)
