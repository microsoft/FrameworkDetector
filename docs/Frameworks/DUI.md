---
id: DUI
title: Microsoft DirectUI
description: An internal Windows Shell UI framework included with the Windows operating system.
category: Framework
keywords: Framework Detector, DirectUI, Windows Shell, Internal
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Microsoft DirectUI (DUI)

## Summary

Microsoft DirectUI (DUI) is an internal framework used by the Windows Shell, and is included with the Windows operating system.

**Website:** *TODO*

### Languages

**Framework Languages:** C++

**App Languages:** C++

### OS Support

Windows 7 SP1, 8, 8.1, 10, and 11

### Dependencies

*N/A*

### Canonical Apps

- [MMC](https://learn.microsoft.com/troubleshoot/windows-server/system-management-components/what-is-microsoft-management-console)
- [Windows Task Scheduler](https://learn.microsoft.com/windows/win32/taskschd/about-the-task-scheduler)

## How to Detect

**Implementation:** [DUIDetector](/src/FrameworkDetector/Detectors/DUIDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `dui70.dll`

As a system DLL, the specific version of DUI used will match the version of Windows used when the app is run.

### Static Detection

It is not possible to definitively determine the use of DUI by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries.

## Resources

- *TODO*
