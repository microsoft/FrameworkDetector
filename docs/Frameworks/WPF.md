---
id: WPF
title: Windows Presentation Foundation
description: An open-source, vector-based UI framework for building Windows desktop applications on .NET.
website: https://learn.microsoft.com/dotnet/desktop/wpf
source: https://github.com/dotnet/wpf
category: Framework
keywords: Framework Detector, WPF, UI, XAML, .NET, Desktop
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Windows Presentation Foundation (WPF)

## Summary

Windows Presentation Foundation (WPF) is an open-source, graphical user interface for Windows, on .NET.

WPF is resolution-independent and uses a vector-based rendering engine, built to take advantage of modern graphics hardware. WPF provides a comprehensive set of application-development features that include Extensible Application Markup Language (XAML), controls, data binding, layout, 2D and 3D graphics, animation, styles, templates, documents, media, text, and typography. WPF is part of .NET, so you can build applications that incorporate other elements of the .NET API.

**Website:** [WPF Docs](https://learn.microsoft.com/dotnet/desktop/wpf)

### Languages

**Framework Languages:** C, C++, C#, and XAML

**App Languages:** C#, Visual Basic, XAML and other .NET languages

### OS Support

**.NET:** Windows 7 SP1, 8.1, 10, and 11 as per .NET version

**.NET Framework 4:** Windows Vista, 7 SP1, 8.1, 10, and 11 as per .NET Framework version

### Dependencies

WPF depends on either:

- [.NET](https://dotnet.microsoft.com/download/dotnet) or
- [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework)

For more information on the differences, see [NET implementations](https://learn.microsoft.com/dotnet/fundamentals/implementations).

### Canonical Apps

**.NET:**

- [WPF Gallery Preview](https://apps.microsoft.com/detail/9ndlx60wx4kq)

**.NET Framework:**

- *TODO*

## How to Detect

**Implementation:** [WPFDetector](/src/FrameworkDetector/Detectors/WPFDetector.cs)

### Runtime Detection

Either of the following modules should be loaded by the running process:

1. `PresentationCore.dll` (or the Ngened[^1] `PresentationCore.ni.dll`)
2. `PresentationFramework.dll` (or the Ngened[^1] `PresentationFramework.ni.dll`)

The specific version of WPF can be gotten by checking the FileVersion of any of these modules.

[^1]: [Ngen on Microsoft Learn](https://learn.microsoft.com/dotnet/framework/tools/ngen-exe-native-image-generator)

### Static Detection

It is not possible to definitively determine the use of WPF by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

For self-contained .NET apps, the presence of any of the aforementioned module(s) is not definitive proof that the app uses WPF. Without module trimming, self-contained WinForms apps, for example, will also contain the WPF binaries by default.

For both (framework-dependent) .NET apps and standard .NET Framework apps, which rely on system-installed versions of .NET, the absence of the aforementioned module(s) with the app's binaries is also not definitive proof that the app does not use WPF.

## Resources

- [WPF Docs](https://learn.microsoft.com/dotnet/desktop/wpf)
- [WPF GitHub Source](https://github.com/dotnet/wpf)
