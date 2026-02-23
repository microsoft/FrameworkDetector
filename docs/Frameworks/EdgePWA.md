---
id: EdgePWA
title: Microsoft Edge PWA
description: A framework for installing Progressive Web Apps to the desktop using Microsoft Edge.
website: https://learn.microsoft.com/microsoft-edge/progressive-web-apps
category: Framework
keywords: Framework Detector, Web, Edge, Progressive Web App
ms.date: 02/23/2026
author: jonthysell
status: Detectable
---

# Microsoft Edge PWA (EdgePWA)

## Summary

Microsoft Edge PWA (EdgePWA) is a framework for installing Progressive Web Apps (PWAs) to the desktop using Microsoft Edge.

Edge users can install any web app to their Windows desktop via the browser, however many PWA developers also choose to publish their apps to the Microsoft Store, often using tools such as [PWABuilder](https://pwabuilder.com).

**Website:** [EdgePWA Website](https://learn.microsoft.com/microsoft-edge/progressive-web-apps)

### Languages

**Framework Languages:** C, C++

**Hosted App Languages:** Web (HTML, CSS, JavaScript and TypeScript)

### OS Support

Windows 10 and 11

### Dependencies

EdgePWA depends on [Microsoft Edge](https://microsoft.com/edge) which in turn depends on [Google Chromium](https://chromium.org/Home).

### Canonical Apps

- [Adobe Express](https://apps.microsoft.com/detail/9p94lh3q1cp5)

## How to Detect

**Implementation:** [EdgePWADetector](/src/FrameworkDetector/Detectors/EdgePWADetector.cs)

### Runtime Detection

EdgePWA apps are web apps launched via a specialized version of Edge named `pwahelper.exe`. So it is then possible to detect EdgePWA by detecting that the running process's main module is actually just `pwahelper.exe`.

**Note:** It is equally valid to detect `pwahelper.exe` in the loaded modules of the process.

The specific version of EdgePWA can be gotten by checking the FileVersion of the module.

### Static Detection

It is not possible to definitively determine the use of EdgePWA by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [EdgePWA Website](https://learn.microsoft.com/microsoft-edge/progressive-web-apps)
- [EdgePWA Docs](https://learn.microsoft.com/microsoft-edge/progressive-web-apps/landing)
- [PWABuilder Website](https://pwabuilder.com)
