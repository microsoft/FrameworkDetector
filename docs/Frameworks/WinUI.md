---
id: WinUI
title: Windows UI Library for UWP
description: The built-in Windows Runtime XAML UI framework for UWP applications.
website: https://learn.microsoft.com/windows/apps/winui
category: Framework
keywords: Framework Detector, UI, WinUI, UWP, XAML, WinRT
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Windows UI Library for UWP (WinUI)

## Summary

The Windows UI Library for UWP (WinUI) is a user interface API that is part of the Windows Runtime programming model powering Universal Windows Platform apps. It enables declaring user interfaces using Extensible Application Markup Language (XAML) technology.

WinUI is included with the Windows operating system, and is the primary UI framework for the Windows Runtime (WinRT). WinUI is often synonymous with UWP itself, and is also known as System XAML, UWP XAML, and/or WUX (after the Windows.UI.Xaml namespace).

**Website:** [WinUI Docs](https://learn.microsoft.com/windows/apps/winui)

### Languages

**Framework Languages:** C, C++, C#, and XAML

**App Languages:** C++, C#, Visual Basic, XAML and other .NET languages

### OS Support

Windows 8.1, 10, and 11

### Dependencies

*N/A*

### Canonical Apps

- [WinUI 2 Gallery](https://apps.microsoft.com/detail/9msvh128x2zt)

## How to Detect

**Implementation:** [WinUIDetector](/src/FrameworkDetector/Detectors/WinUIDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `Windows.UI.Xaml.dll`

The specific version of WinUI can be gotten by checking the FileVersion of this module.

### Static Detection

It is not possible to definitively determine the use of WinUI by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries.

## Resources

- [WinUI Docs](https://learn.microsoft.com/windows/apps/winui)
- [Universal Windows Platform Docs](https://learn.microsoft.com/en-us/windows/uwp)
- [Develop UWP Apps](https://learn.microsoft.com/windows/uwp/develop)
- [UWP XAML Docs](https://learn.microsoft.com/windows/uwp/xaml-platform)
- [Windows.UI.Xaml Namespace Docs](https://learn.microsoft.com/uwp/api/windows.ui.xaml)
