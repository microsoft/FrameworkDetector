# MVVM Toolkit (MVVMToolkit)

## Summary

The MVVM Toolkit is a modern, fast, and modular MVVM library. It is part of the .NET Community Toolkit.

**Website:** [MVVM Toolkit Docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

### Languages

**Framework Languages:** C#

**App Languages:** C#

### OS Support

**.NET:** Windows 7 SP1, 8.1, 10, and 11 as per .NET version

**.NET Framework 4:** Windows Vista, 7 SP1, 8.1, 10, and 11 as per .NET Framework version

### Dependencies

The MVVM Toolkit depends on either:

- [.NET](https://dotnet.microsoft.com/en-us/download/dotnet) or
- [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

For more information on the differences, see [NET implementations](https://learn.microsoft.com/en-us/dotnet/fundamentals/implementations).

### Canonical Apps

**.NET:**

- [WPF Gallery Preview](https://apps.microsoft.com/detail/9ndlx60wx4kq)

**.NET Framework:**

- *TODO*

## How to Detect

**Implementation:** [MVVMToolkitDetector](/src/FrameworkDetector/Detectors/Library/dotnet/MVVMToolkitDetector.cs)

### Runtime Detection

Either of the following modules should be loaded by the running process:

1. `CommunityToolkit.MVVM.dll` (or the Ngened[^1] `CommunityToolkit.MVVM.ni.dll`)
2. `Microsoft.Toolkit.MVVM.dll` (or the Ngened[^1] `Microsoft.Toolkit.MVVM.ni.dll`)

The module was originally named `Microsoft.Toolkit.MVVM.dll` but was then renamed to `CommunityToolkit.MVVM.dll` starting with version 7.

The specific version of the MVVM Toolkit can be gotten by checking the FileVersion of either of these modules.

[^1]: [Ngen on Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator)

### Static Detection

For both self-contained and framework-dependent .NET apps, either of the aforementioned runtime modules should be present with the app's binaries. The specific version of the MVVM Toolkit can be gotten by checking the FileVersion of either of these modules.

## Resources

- [Official Docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [GitHub Source](https://github.com/CommunityToolkit/dotnet)
