---
id: WinForms
title: Windows Forms
description: A GUI class library for building Windows desktop applications on .NET.
website: https://learn.microsoft.com/dotnet/desktop/winforms
source: https://github.com/dotnet/winforms
category: Framework
keywords: Framework Detector, UI, WinForms, Windows, .NET, Desktop
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Windows Forms (WinForms)

## Summary

Windows Forms (WinForms) is a free, open-source graphical user interface (GUI) class library for building Windows desktop applications on .NET.

Windows Forms provides access to native Windows User Interface Common Controls by wrapping the existent Windows API in managed code.

**Website:** [WinForms Docs](https://learn.microsoft.com/dotnet/desktop/winforms)

### Languages

**Framework Languages:** C# and Visual Basic

**App Languages:** C#, Visual Basic and other .NET languages

### OS Support

**.NET:** Windows 7 SP1, 8.1, 10, and 11 as per .NET version

**.NET Framework 4:** Windows Vista, 7 SP1, 8.1, 10, and 11 as per .NET Framework version

### Dependencies

WinForms depends on [Common Controls](https://learn.microsoft.com/windows/win32/controls/common-controls-intro) and either:

- [.NET](https://dotnet.microsoft.com/download/dotnet) or
- [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework)

For more information on the differences, see [NET implementations](https://learn.microsoft.com/dotnet/fundamentals/implementations).

### Canonical Apps

**.NET:**

- *TODO*

**.NET Framework:**

- [Telerik UI for WinForms Examples](https://apps.microsoft.com/detail/9PG9BR4D400B)

## How to Detect

**Implementation:** [WinFormsDetector](/src/FrameworkDetector/Detectors/WinFormsDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `System.Windows.Forms.dll` (or the Ngened[^1] `System.Windows.Forms.ni.dll`)

The specific version of WinForms can be gotten by checking the FileVersion of the loaded module.

[^1]: [Ngen on Microsoft Learn](https://learn.microsoft.com/dotnet/framework/tools/ngen-exe-native-image-generator)

### Static Detection

It is not possible to definitively determine the use of WinForms by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

For self-contained .NET apps, the presence of any of the aforementioned module(s) is not definitive proof that the app uses WinForms. Without module trimming, self-contained WPF apps, for example, will also contain the WinForms binaries by default.

For both (framework-dependent) .NET apps and standard .NET Framework apps, which rely on system-installed versions of .NET, the absence of the aforementioned module(s) with the app's binaries is also not definitive proof that the app does not use WinForms.

## Resources

- [WinForms Docs](https://learn.microsoft.com/dotnet/desktop/winforms)
- [WinForms GitHub Source](https://github.com/dotnet/winforms)
