---
id: WinAppSDK
title: Windows App SDK
description: The Windows App SDK empowers all Windows desktop apps with modern Windows UI, APIs, and platform features, including back-compat support, shipped via NuGet.
website: https://learn.microsoft.com/windows/apps/windows-app-sdk
source: https://github.com/microsoft/WindowsAppSDK
category: Component
keywords: Framework Detector, Windows App SDK, AI
ms.date: 11/19/2025
author: michael-hawker
status: Experimental
---

# Windows App SDK (WinAppSDK)

## Summary

The Windows App SDK (WinAppSDK) is a set of libraries, frameworks, components, and tools that you can use in your apps to access powerful Windows platform functionality from all kinds of apps on many versions of Windows. WinAppSDK combines the powers of Win32 native applications alongside modern API usage techniques, so your apps light up everywhere your users are.

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

**Implementation:** [WindowsAppSDKDetector](/src/FrameworkDetector/Detectors/WindowsAppSDKDetector.cs)

### Runtime Detection

TBD

### Static Detection

If given an app package (MSIX), check the package dependencies in the `AppxManifest.xml` for `Microsoft.WindowsAppRuntime*` to detect the WinAppSDK:

```xml
<PackageDependency Name="Microsoft.WindowsAppRuntime.1.8" MinVersion="8000.616.304.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
```

Full example of a dependency section:

```xml
<Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22621.0"/>
    <PackageDependency Name="Microsoft.WindowsAppRuntime.1.8" MinVersion="8000.616.304.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
    <PackageDependency Name="Microsoft.VCLibs.140.00.UWPDesktop" MinVersion="14.0.33728.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
    <PackageDependency Name="Microsoft.VCLibs.140.00" MinVersion="14.0.33519.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"/>
</Dependencies>
```

## Resources

- [WinAppSDK Docs](https://learn.microsoft.com/windows/apps/windows-app-sdk)
- [WinAppSDK GitHub Source (non-extensive)](https://github.com/microsoft/WindowsAppSDK)
- [WinAppSDK Sample Repo](https://github.com/microsoft/WindowsAppSDK-Samples)
