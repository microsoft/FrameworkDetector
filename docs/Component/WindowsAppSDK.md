---
id: WASDK
title: Windows App SDK (WinAppSDK)
description: The Windows App SDK empowers all Windows desktop apps with modern Windows UI, APIs, and platform features, including back-compat support, shipped via NuGet.
website: https://learn.microsoft.com/windows/apps/windows-app-sdk
source: https://github.com/microsoft/WindowsAppSDK
category: Component
keywords: Framework Detector, Windows App SDK, AI
ms.date: 10/21/2025
author: michael-hawker
status: Experimental
---

# Windows App SDK (WinAppSDK)

## Summary

Windows App SDK (formerly Project Reunion) is a set of libraries, frameworks, components, and tools that you can use in your apps to access powerful Windows platform functionality from all kinds of apps on many versions of Windows. The Windows App SDK combines the powers of Win32 native applications alongside modern API usage techniques, so your apps light up everywhere your users are.

**Website:** [WinAppSDK Docs](https://learn.microsoft.com/windows/apps/windows-app-sdk)

### Languages

**Framework Languages:** C, C++

**App Languages:** C++, C#, Visual Basic, and other .NET languages

### OS Support

Windows 10, and 11

### Dependencies

N/A

### Canonical Apps

- [WinUI 3 Gallery](https://apps.microsoft.com/detail/9p3jfpwwdzrc)

## How to Detect

**Implementation:** [WindowsAppSDKDetector](/src/FrameworkDetector/Detectors/Component/WindowsAppSDKDetector.cs)

### Runtime Detection

TBD

### Static Detection

It is not possible to definitively determine the use of WinUI3 by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries.

## Resources

- [WinAppSDK Docs](https://learn.microsoft.com/windows/apps/windows-app-sdk)
- [WinAppSDK GitHub Source (non-extensive)](https://github.com/microsoft/WindowsAppSDK)
- [WinAppSDK Sample Repo](https://github.com/microsoft/WindowsAppSDK-Samples)
