---
id: ComCtl
title: Microsoft Common Controls
description: A set of window classes implemented by Comctl32.dll included with Windows.
website: https://learn.microsoft.com/windows/win32/controls/common-controls-intro
category: Library
keywords: Framework Detector, Windows, Common Controls, Comctl32
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Microsoft Common Controls (ComCtl)

## Summary

Microsoft Common Controls (ComCtl) is a set of windows that are implemented by the common control library, Comctl32.dll, which is a DLL included with the Windows operating system.

**Website:** [ComCtl Docs](https://learn.microsoft.com/windows/win32/controls/common-controls-intro)

### Languages

**Framework Languages:** C++

**App Languages:** C++

### OS Support

Windows 2000, XP, Vista, 7 SP1, 8, 8.1, 10, and 11

### Dependencies

*N/A*

### Canonical Apps

- [RegEdit](https://learn.microsoft.com/troubleshoot/windows-server/performance/windows-registry-advanced-users#use-registry-editor)

## How to Detect

**Implementation:** [ComCtlDetector](/src/FrameworkDetector/Detectors/ComCtlDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `comctl32.dll`

The specific version of ComCtl can be gotten by checking the FileVersion of the loaded module.

### Static Detection

It is not possible to definitively determine the use of ComCtl by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries.

## Resources

- [ComCtl Docs](https://learn.microsoft.com/windows/win32/controls/common-controls-intro)
- [Common Control Versions](https://learn.microsoft.com/windows/win32/controls/common-control-versions)
- [The history of the Windows XP common controls](https://devblogs.microsoft.com/oldnewthing/20080129-00/?p=23663)
