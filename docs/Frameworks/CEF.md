---
id: CEF
title: Chromium Embedded Framework
description: A framework for embedding Chromium-based browsers in other applications.
source: https://bitbucket.org/chromiumembedded/cef
category: Component
keywords: Framework Detector, Web, Embedded, Chrome
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Chromium Embedded Framework (CEF)

## Summary

The Chromium Embedded Framework (CEF) is a framework for embedding Chromium-based browsers in other applications.

Developers can use CEF to host their entire app written using web technologies, or as a simple UI component for hosting web content in a native app primarily built with other frameworks.

**Website:** [CEF Website](https://bitbucket.org/chromiumembedded/cef)

### Languages

**Framework Languages:** C, C++

**Host App Languages:** C, C++, Delphi, Go, Java, Python, C#, Visual Basic and other .NET languages

**Hosted App Languages:** Web (HTML, CSS, JavaScript and TypeScript)

### OS Support

Windows, macOS, Linux

### Dependencies

CEF depends on [Google Chromium](https://chromium.org/Home).

### Canonical Apps

- [GOG Galaxy](https://gog.com/galaxy)
- [Steam Client](https://store.steampowered.com/about/download)

## How to Detect

**Implementation:** [CEFDetector](/src/FrameworkDetector/Detectors/Component/WebView/CEFDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `libecef.dll`

The specific version of CEF can be gotten by checking the FileVersion of the loaded module.

### Static Detection

It is not possible to definitively determine the use of CEF by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [CEF Website](https://bitbucket.org/chromiumembedded/cef)
- [CefSharp GitHub Source](https://github.com/cefsharp/CefSharp)
