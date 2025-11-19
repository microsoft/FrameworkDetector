---
id: MVVMToolkit
title: MVVM Toolkit
description: A modern, fast, and modular MVVM library that is part of the .NET Community Toolkit.
website: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm
source: https://github.com/CommunityToolkit/dotnet
category: Library
keywords: Framework Detector, MVVM, .NET, Community Toolkit
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# MVVM Toolkit (MVVMToolkit)

## Summary

The MVVM Toolkit is a modern, fast, and modular MVVM library. It is part of the .NET Community Toolkit.

**Website:** [MVVM Toolkit Docs](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm)

### Languages

**Framework Languages:** C#

**App Languages:** C# and other .NET languages

### OS Support

**.NET:** Windows 7 SP1, 8.1, 10, and 11 as per .NET version

**.NET Framework 4:** Windows Vista, 7 SP1, 8.1, 10, and 11 as per .NET Framework version

### Dependencies

The MVVM Toolkit depends on either:

- [.NET](https://dotnet.microsoft.com/download/dotnet) or
- [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework)

For more information on the differences, see [NET implementations](https://learn.microsoft.com/dotnet/fundamentals/implementations).

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

[^1]: [Ngen on Microsoft Learn](https://learn.microsoft.com/dotnet/framework/tools/ngen-exe-native-image-generator)

### Static Detection

It is not possible to definitively determine the use of the MVVM Toolkit by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [MVVM Toolkit Docs](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm)
- [Community Toolkit Github Source](https://github.com/CommunityToolkit/dotnet)
