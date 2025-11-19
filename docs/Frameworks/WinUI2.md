---
id: WinUI2
title: WinUI 2 for UWP
description: An extension library delivering newer controls for UWP apps independent of Windows OS version.
website: https://learn.microsoft.com/windows/uwp/get-started/winui2
source: https://github.com/microsoft/microsoft-ui-xaml
category: Framework
keywords: Framework Detector, UI, WinUI, UWP, MUX, MUXC, XAML
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# WinUI 2 for UWP (WinUI2)

## Summary

WinUI 2 for UWP (WinUI2) is an extension of the Windows UI Library for UWP (WinUI) framework built into Windows 10 and 11. Unlike WinUI, WinUI2 is shipped separately from the Windows OS. WinUI2 provides new and updated controls so that apps running on older versions of Windows 10 (with older base versions of WinUI) can take advantage of the latest functionality.

As many (if not most) WinUI UWP apps use WinUI 2, the term WinUI 2 is often used to describe all WinUI and WinUI 2 apps. To differentiate the two, WinUI2 is often referred to as MUX or MUXC (after the Microsoft.UI.Xaml and Microsoft.UI.Xaml.Controls namespaces, respectively).

**Website:** [WinUI2 Docs](https://learn.microsoft.com/windows/uwp/get-started/winui2)

### Languages

**Framework Languages:** C, C++, C#, and XAML

**App Languages:** C++, C#, Visual Basic, XAML and other .NET languages

### OS Support

Windows 10, and 11

### Dependencies

- [Windows UI Library for UWP](https://learn.microsoft.com/windows/uwp/xaml-platform)

### Canonical Apps

- [WinUI 2 Gallery](https://apps.microsoft.com/detail/9msvh128x2zt)

## How to Detect

**Implementation:** [WinUI2Detector](/src/FrameworkDetector/Detectors/WinUI2Detector.cs)

### Runtime Detection

The following package dependency should be present in the app's package dependency information:

1. `Microsoft.UI.Xaml` prefix.

The specific version of WinUI2 can be determined by extracting the version from the PackageFullName.

Or the following module should be loaded by the running process:

1. `Microsoft.UI.Xaml.dll` with FileVersion ≥ 2.0 and < 3.0

The specific version of WinUI2 can be gotten by checking the FileVersion of this module.

### Static Detection

If given an app package (MSIX), check the package dependencies in the `AppxManifest.xml` for:

```xml
<PackageDependency Name="Microsoft.UI.Xaml.2.8" MinVersion="8.2501.31001.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
```

Full example of a dependency section:

```xml
<Dependencies>
    <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0"/>
    <PackageDependency Name="Microsoft.UI.Xaml.2.8" MinVersion="8.2501.31001.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
    <PackageDependency Name="Microsoft.NET.Native.Framework.2.2" MinVersion="2.2.29512.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
    <PackageDependency Name="Microsoft.NET.Native.Runtime.2.2" MinVersion="2.2.28604.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
    <PackageDependency Name="Microsoft.VCLibs.140.00" MinVersion="14.0.33519.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
    <PackageDependency Name="Microsoft.Services.Store.Engagement" MinVersion="10.0.23012.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
</Dependencies>
```

## Resources

- [WinUI2 Docs](https://learn.microsoft.com/windows/uwp/get-started/winui2)
- [WinUI2 GitHub Source](https://github.com/microsoft/microsoft-ui-xaml/tree/winui2/main)
- [Microsoft.UI.Xaml API Docs](https://learn.microsoft.com/windows/winui/api)
