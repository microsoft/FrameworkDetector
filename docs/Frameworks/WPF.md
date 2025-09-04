# Windows Presentation Foundation (WPF)

## Summary

Windows Presentation Foundation (WPF) is an open-source, graphical user interface for Windows, on .NET.

WPF is resolution-independent and uses a vector-based rendering engine, built to take advantage of modern graphics hardware. WPF provides a comprehensive set of application-development features that include Extensible Application Markup Language (XAML), controls, data binding, layout, 2D and 3D graphics, animation, styles, templates, documents, media, text, and typography. WPF is part of .NET, so you can build applications that incorporate other elements of the .NET API.

**Website:** [WPF Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)

### Languages

**Framework Languages:** C, C++, C#, and XAML

**App Languages:** C# and XAML

### OS Support

**.NET:** Windows 7 SP1, 8.1, 10, and 11 as per .NET version

**.NET Framework 4:** Windows Vista, 7 SP1, 8.1, 10, and 11 as per .NET Framework version

### Dependencies

WPF depends on either:

- [.NET](https://dotnet.microsoft.com/en-us/download/dotnet) or
- [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

For more information on the differences, see [NET implementations](https://learn.microsoft.com/en-us/dotnet/fundamentals/implementations).

### Canonical Apps

**.NET:**

- [WPF Gallery Preview](https://apps.microsoft.com/detail/9ndlx60wx4kq)

**.NET Framework:**

- *TODO*

## How to Detect

**Implementation:** [WPFDetector](/src/FrameworkDetector/Detectors/Frameworks/WPFDetector.cs)

### Runtime Detection

Either of the following modules should be loaded by the running process:

1. `PresentationCore.dll` (or the Ngened[^1] `PresentationCore.ni.dll`)
2. `PresentationFramework.dll` (or the Ngened[^1] `PresentationFramework.ni.dll`)

The specific version of WPF can be gotten by checking the FileVersion of any of these modules.

[^1]: [Ngen on Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator)

### Static Detection

For self-contained .NET apps, any of the aforementioned runtime modules should be present with the app's binaries. The specific version of WPF can be gotten by checking the FileVersion of any these modules.

However, since both framework-dependent .NET apps, and standard .NET Framework apps rely on system-installed versions of .NET, the absence of those modules with the app's binaries is not definitely proof that the app does not use WPF.

## Resources

- [Official Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [GitHub Source](https://github.com/dotnet/wpf)
