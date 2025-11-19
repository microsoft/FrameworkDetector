---
id: WebView2
title: Microsoft Edge WebView2
description: A framework for embedding the Chromium-based Microsoft Edge browser in other applications.
website: https://developer.microsoft.com/microsoft-edge/webview2
category: Component
keywords: Framework Detector, Web, Embedded, Edge, Chromium
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Microsoft Edge WebView2 (WebView2)

## Summary

Microsoft Edge WebView2 (WebView2) is a framework for embedding the chromium-based Edge browser in other applications.

Developers can use WebView2 to host their entire app written using web technologies, or as a simple UI component for hosting web content in a native app primarily built with other frameworks.

**Website:** [WebView2 Website](https://developer.microsoft.com/microsoft-edge/webview2)

### Languages

**Framework Languages:** C, C++

**Host App Languages:** C++, C#, Visual Basic and other .NET languages

**Hosted App Languages:** Web (HTML, CSS, JavaScript and TypeScript)

### OS Support

Windows 10 and 11

### Dependencies

WebView2 depends on [Microsoft Edge](https://microsoft.com/edge) which in turn depends on [Google Chromium](https://chromium.org/Home).

### Canonical Apps

- [Microsoft Teams](https://apps.microsoft.com/detail/xp8bt8dw290mpq)
- [WinUI 2 Gallery](https://apps.microsoft.com/detail/9msvh128x2zt)
- [WinUI 3 Gallery](https://apps.microsoft.com/detail/9p3jfpwwdzrc)

## How to Detect

**Implementation:** [WebView2Detector](/src/FrameworkDetector/Detectors/Component/WebView/WebView2Detector.cs)

### Runtime Detection

Any of the following modules should be loaded by the running process:

1. `WebView2Loader.dll`
2. `Microsoft.Web.WebView2.Core.dll`
3. `Microsoft.Web.WebView2.Core.Projection.dll`

The specific version of WebView2 can be gotten by checking the FileVersion of the loaded module.

It is also possible to detect WebView2 by checking for the following modules be loaded by the running process:

1. `EmbeddedBrowserWebView.dll`

however it is not possible to determine the specific version of WebView2 used when detecting these modules.

**Note:** It may be necessary to navigate to WebView2 content in the UI of an app before one of these modules is loaded.

### Static Detection

It is not possible to definitively determine the use of WebView2 by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

If the app is packaged, it may register WebView2 activatable classes in its `AppxManifest.xml` as shown below:

```xml
<Extension Category="windows.activatableClass.inProcessServer">
    <InProcessServer>
        <Path>Microsoft.Web.WebView2.Core.dll</Path>
        <ActivatableClass ActivatableClassId="Microsoft.Web.WebView2.Core.CoreWebView2ControllerWindowReference" ThreadingModel="both"/>
        <ActivatableClass ActivatableClassId="Microsoft.Web.WebView2.Core.CoreWebView2Environment" ThreadingModel="both"/>
        <ActivatableClass ActivatableClassId="Microsoft.Web.WebView2.Core.CoreWebView2CompositionController" ThreadingModel="both"/>
        <ActivatableClass ActivatableClassId="Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions" ThreadingModel="both"/>
        <ActivatableClass ActivatableClassId="Microsoft.Web.WebView2.Core.CoreWebView2CustomSchemeRegistration" ThreadingModel="both"/>
        <ActivatableClass ActivatableClassId="Microsoft.Web.WebView2.Core.CoreWebView2Controller" ThreadingModel="both"/>
    </InProcessServer>
</Extension>
```

## Resources

- [WebView2 Website](https://developer.microsoft.com/microsoft-edge/webview2)
- [WebView2 Docs](https://learn.microsoft.com/microsoft-edge/webview2/landing)
- [WebView2 Samples GitHub Source](https://github.com/MicrosoftEdge/WebView2Samples)