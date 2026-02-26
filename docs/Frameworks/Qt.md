---
id: Qt
title: Qt Framework
description: A cross-platform UI framework written in C++ which supports running apps on Linux, Windows, macOS, iOS, Android, and embedded systems.
website: https://qt.io
source: https://code.qt.io/cgit/qt/qtbase.git
category: Framework
keywords: Framework Detector, UI, Cross-Platform, C++
ms.date: 02/12/2026
author: michael-hawker
status: Experimental
---

# Qt (Qt)

## Summary

The Qt Framework (Qt, pronounced "cute") is a cross-platform UI framework written in C++ which supports running apps on Linux, Windows, macOS, iOS, Android, and embedded systems.

It supports various other languages through various language bindings, like Python, Java, JavaScript, Rust, Go, C#, D, and Haskell.

**Website:** [Qt Website](https://qt.io)

### Languages

**Framework Languages:** C++

**App Languages:** Python, Java, JavaScript, Rust, Go, C#, D, and Haskell.

### OS Support

Windows 7, 8, 10, and 11

### Dependencies

*N/A*

### Canonical Apps

#### Qt4

*TODO*

**Note:** Qt4 support ended in 2015

#### Qt5

- [Krita](https://krita.org) - [Source Code](https://invent.kde.org/graphics/krita)
- [OpenShot Video Editor](https://openshot.org) - [Source Code](https://github.com/OpenShot/openshot-qt) 
- [VLC](https://www.videolan.org/) (using their own fork)
- [WPS Office](https://www.wps.com/office/windows/) (using the Kso fork)

#### Qt6

- [Wireshark](https://wireshark.org) - [Source Code](https://gitlab.com/wireshark/wireshark)

Other well-known applications using Qt:

- [DaVinci Resolve](https://blackmagicdesign.com/products/davinciresolve)
- [Mathematica](https://wolfram.com/mathematica)

## How to Detect

**Implementation:** [QtDetector](/src/FrameworkDetector/Detectors/QtDetector.cs)

### Runtime Detection

A module following the naming pattern `Qt*Core.dll` should be loaded by the app's running process, where `*` is Qt's major version, i.e.:

1. `Qt4Core.dll`
2. `Qt5Core.dll`
3. `Qt6Core.dll`

The specific version of Qt can be gotten by checking the FileVersion of the loaded module.

Kingsoft Corporation maintains a public fork of Qt which is popular with Chinese developers. Their modules follow the naming pattern `Qt*CoreKso.dll`, where `*` is Qt's major version, i.e.:

1. `Qt4CoreKso.dll`
2. `Qt5CoreKso.dll`
3. `Qt6CoreKso.dll`

The specific version of the "Kso" Qt can be gotten by checking the FileVersion of the loaded module.

Furthermore, in some instances, apps may have built their own versions of Qt and therefore load a different module name. For example, the popular VLC desktop app builds Qt into their own `libqt_plugin.dll`. In that case, it may also be possible to detect the presence of Qt by checking an app's active windows for the window class `Qt*QWindowIcon`, where `*` is the Qt version, i.e:

1. `Qt5QWindowIcon`
2. `Qt5152QWindowIcon`
3. `Qt693QWindowIcon`

### Static Detection

It is not possible to definitively determine the use of Qt by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [Qt Website](https://qt.io)
- [Qt Source Repo](https://code.qt.io/cgit/qt/qtbase.git)
- [Qt Wikipedia Page](https://en.wikipedia.org/wiki/Qt_(software))
