---
id: WebView1
title: Microsoft EdgeHTML
description: A framework for embedding the legacy Edge (EdgeHTML) browser engine in other applications.
website: https://learn.microsoft.com/windows/apps/design/controls/web-view
category: Component
keywords: Framework Detector, Web, Embedded, EdgeHTML, Legacy Edge
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Microsoft EdgeHTML (WebView1)

## Summary

Microsoft EdgeHTML (WebView1) is a framework for embedding the legacy Edge browser in other applications.

Developers can use WebView1 to host their entire app written using web technologies, or as a simple UI component for hosting web content in a native app primarily built with other frameworks.

**Website:** [WebView Docs](https://learn.microsoft.com/windows/apps/design/controls/web-view)

### Languages

**Framework Languages:** C, C++

**Host App Languages:** C++, C#, Visual Basic and other .NET languages

**Hosted App Languages:** Web (HTML, CSS, JavaScript and TypeScript)

### OS Support

Windows 10 and 11

### Dependencies

WebView1 depends on [Legacy Microsoft Edge](https://learn.microsoft.com/archive/microsoft-edge/legacy/developer).

### Canonical Apps

- [WinUI 2 Gallery](https://apps.microsoft.com/detail/9msvh128x2zt)

## How to Detect

**Implementation:** [WebView1Detector](/src/FrameworkDetector/Detectors/Component/WebView/WebView1Detector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `edgehtml.dll`

The specific version of WebView1 can be gotten by checking the FileVersion of the loaded module.

**Note:** It may be necessary to navigate to WebView1 content in the UI of an app before the module is loaded.

### Static Detection

It is not possible to definitively determine the use of WebView1 by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [WebView Docs](https://learn.microsoft.com/windows/apps/design/controls/web-view)
- [WebView API Docs](https://learn.microsoft.com/uwp/api/windows.ui.xaml.controls.webview)
- [Legacy Microsoft Edge Developer Docs](https://learn.microsoft.com/archive/microsoft-edge/legacy/developer)
- [What's new in EdgeHTML 18](https://learn.microsoft.com/archive/microsoft-edge/legacy/developer/dev-guide)