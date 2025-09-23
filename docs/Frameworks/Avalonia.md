# Avalonia UI (Avalonia)

## Summary

Avalonia is a cross-platform UI framework for dotnet, providing a flexible styling system and supporting a wide range of platforms such as Windows, macOS, Linux, iOS, Android and WebAssembly.

**Website:** [Avalonia UI](https://avaloniaui.net/)

### Languages

**Framework Languages:** C#, C++, and Objective C

**App Languages:** C# and other .NET languages

### OS Support

**.NET:** Windows 7 SP1, 8.1, 10, and 11 as per .NET version

**.NET Framework 4:** Windows Vista, 7 SP1, 8.1, 10, and 11 as per .NET Framework version

### Dependencies

Avalonia depends on either:

- [.NET](https://dotnet.microsoft.com/en-us/download/dotnet) or
- [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

For more information on the differences, see [NET implementations](https://learn.microsoft.com/en-us/dotnet/fundamentals/implementations).

### Canonical Apps

**.NET:**

- [Avalonia Control Gallery](https://apps.microsoft.com/detail/9pghn6qnj3rp)

**.NET Framework:**

- *TODO*

## How to Detect

**Implementation:** [AvaloniaDetector](/src/FrameworkDetector/Detectors/Frameworks/AvaloniaDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `Avalonia.Base.dll` (or the Ngened[^1] `System.Windows.Forms.ni.dll`)

The specific version of Avalonia can be gotten by checking the FileVersion of the loaded module.

[^1]: [Ngen on Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator)

### Static Detection

It is not possible to definitively determine the use of Avalonia by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [Official Docs](https://avaloniaui.net/)
- [GitHub Source](https://github.com/AvaloniaUI/Avalonia)
