---
id: WinUI3
title: WinUI 3 for Windows App SDK
description: The native UI platform component shipped with Windows App SDK, extracted from Windows.
website: https://learn.microsoft.com/windows/apps/winui/winui3
source: https://github.com/microsoft/microsoft-ui-xaml
category: Framework
keywords: Framework Detector, UI, WinUI, Windows App SDK, XAML
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# WinUI 3 for Windows App SDK (WinUI3)

## Summary

WinUI 3 for Windows App SDK (WinUI3) is the native UI platform component that ships with the Windows App SDK. It is a refactored version of WinUI that was extracted from the Windows OS.

**Website:** [WinUI3 Docs](https://learn.microsoft.com/windows/apps/winui/winui3)

### Languages

**Framework Languages:** C, C++, C#, and XAML

**App Languages:** C++, C#, Visual Basic, XAML and other .NET languages

### OS Support

Windows 10, and 11

### Dependencies

- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk)

### Canonical Apps

- [WinUI 3 Gallery](https://apps.microsoft.com/detail/9p3jfpwwdzrc)

## How to Detect

**Implementation:** [WinUI3Detector](/src/FrameworkDetector/Detectors/WinUI3Detector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `Microsoft.UI.Xaml.dll` with FileVersion ≥ 3.0

The specific version of WinUI3 can be gotten by checking the FileVersion of this module.

### Static Detection

It is not possible to definitively determine the use of WinUI3 by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries.

The presense of the WinAppSDK as a package dependency may indicate usage of WinUI3, but it is not definitive as WinAppSDK can be used without WinUI3.

## Resources

- [WinUI3 Docs](https://learn.microsoft.com/windows/apps/winui/winui3)
- [WinUI3 GitHub Source](https://github.com/microsoft/microsoft-ui-xaml)
- [Microsoft.UI.Xaml API Docs](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml)
