Status: **WORK IN PROGRESS**

[![NuGet](https://img.shields.io/nuget/v/Upsparkle.svg?style=for-the-badge)](https://www.nuget.org/packages/Upsparkle/)
[![NuGet](https://img.shields.io/nuget/dt/Upsparkle.svg?style=for-the-badge)](https://www.nuget.org/packages/Upsparkle/)

## About

Cross-platform updater for .NET desktop apps. This library ships as a single NuGet package and uses platform-specific native implementations under the hood:

- On Windows, via wrapping [Winsparkle](https://winsparkle.org).
- On macOS, via wrapping [Sparkle](https://sparkle-project.org).

## Development

Before starting development, install 3rd party dependencies by running the appropriate script for your platform.

**macOS / Linux**

```bash
$ ./getlibs.sh
```

**Windows (PowerShell)**

```powershell
PS> .\Get-Libraries.ps1
```

Both scripts download and extract files based on `.gitbinmodules` content and place them under the `libs` directory.

To use different version of Sparkle or WinSparkle binaries, you can edit `.gitbinmodules` file and change it with your desired version.

## Credits

- [sparkle-project/Sparkle](https://github.com/sparkle-project/Sparkle) for macOS Native framework.
- [vslavik/winsparkle](https://github.com/vslavik/winsparkle) for Win32 Native implementation.
- [rainycape/SparkleSharp](https://github.com/rainycape/SparkleSharp) for Xamarin.Mac binding starting code.

## License

This project is licensed under [MIT License](https://github.com/junian/upsparkle/blob/master/LICENSE).
