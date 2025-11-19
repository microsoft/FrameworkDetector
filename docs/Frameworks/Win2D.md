---
id: Win2D
title: Win2D
description: XAML Behaviors are easy-to-use means of adding common and reusable interactivity to your WinUI applications with minimal code.
website: https://microsoft.github.io/Win2D
source: https://github.com/microsoft/Win2D
category: Library
keywords: Framework Detector, XAML, WinUI, UWP,  Windows App SDK, Direct2D, Win2D
ms.date: 11/19/2025
author: michael-hawker
status: Experimental
---

# Win2D (Win2D)

## Summary

Win2D is an easy-to-use Windows Runtime API for immediate mode 2D graphics rendering with GPU acceleration. It is available to C#, C++ and VB developers writing apps for the Windows Universal Platform (UWP) or Windows App SDK. It utilizes the power of Direct2D, and integrates seamlessly with XAML and CoreWindow.

**Website:** [Win2D Docs](https://microsoft.github.io/Win2D)

### Languages

**Framework Languages:** C++

**App Languages:** C++, C# and other .NET languages

### OS Support

Windows 10, 11

### Dependencies

Win2D depends on either:

- [UWP](https://learn.microsoft.com/windows/uwp)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk)

### Canonical Apps

**.NET:**

- [Win2D Example Gallery](https://apps.microsoft.com/detail/9NBLGGGXWT9F)

## How to Detect

**Implementation:** [Win2DDetector](/src/FrameworkDetector/Detectors/Win2DDetector.cs)

TBD

### Runtime Detection

The following module should be loaded by the running process:

1.`Microsoft.Graphics.Canvas.dll`

(UWP and WindowsAppSDK versions use the same dll name, may use similar versions needs investigation, may not matter as we have other detectors for those)

**Note:** It may be necessary to navigate to Win2D driven content in the UI of an app before one of these modules is loaded.

### Static Detection

It is not possible to definitively determine the use of Win2D by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [Win2D Docs](https://microsoft.github.io/Win2D)
- [Win2D Github Source](https://github.com/microsoft/Win2D)
- [Win2D Example Gallery Source](https://github.com/microsoft/Win2D-Samples)
