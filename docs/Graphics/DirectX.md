---
id: DirectX
title: DirectX
description: Microsoft DirectX graphics provides a set of APIs that you can use to create games and other high-performance multimedia applications.
website: https://learn.microsoft.com/windows/win32/directx
category: Library
keywords: Framework Detector, DirectX, Win32, UWP, Direct3D, Direct2D
ms.date: 10/24/2025
author: michael-hawker
status: Experimental
---

# DirectX

## Summary

Microsoft DirectX graphics provides a set of APIs that you can use to create games and other high-performance multimedia applications. DirectX graphics includes support for high-performance 2-D and 3-D graphics.

**Website:** [DirectX Win32 Docs](https://learn.microsoft.com/windows/win32/directx)

### Languages

**Framework Languages:** C++

**App Languages:** C++, C# and other .NET languages

### OS Support

Windows 10, 11

### Dependencies

DirectX depends on either:

TBD

### Canonical Apps

- [Samples Releases on GitHub (Source)](https://github.com/microsoft/DirectX-Graphics-Samples/releases/tag/MicrosoftDocs-Samples)

## How to Detect

TBD

### Runtime Detection

The following module should be loaded by the running process:

1.`D3D12.dll`

(For Win32 DirectX 12, need to investigate other scenarios like "11 on 12" from samples repo and about other versions.)

### Static Detection

It is not possible to definitively determine the use of Win2D by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [DirectX Win32 Docs](https://learn.microsoft.com/windows/win32/directx)
- [DirectX UWP Docs](https://learn.microsoft.com/en-us/windows/uwp/gaming/e2e)
- [DirectX 12 Samples Repo](https://github.com/microsoft/DirectX-Graphics-Samples)
