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
- [VLC](https://www.videolan.org/)

#### Qt6

- [Wireshark](https://wireshark.org) - [Source Code](https://gitlab.com/wireshark/wireshark)

Other well-known applications using Qt:

- [DaVinci Resolve](https://blackmagicdesign.com/products/davinciresolve)
- [Mathematica](https://wolfram.com/mathematica)

## How to Detect

**Implementation:** [QtDetector](/src/FrameworkDetector/Detectors/QtDetector.cs)

### Runtime Detection

A module following the pattern `Qt*Core.dll` should be loaded by the app's running process, where `*` is Qt's major verion, i.e.:

1. `Qt4Core.dll`
2. `Qt5Core.dll`
3. `Qt6Core.dll`

The specific version of Qt can be gotten by checking the FileVersion of the loaded module.

In some rare instances, apps may have built their own versions of Qt and therefore load a different module name. For example, VLC builds their own `libqt_plugin.dll`. In that case, it may also be possible to detect the presence of Qt by checking an app's active windows for the window class `Qt*QWindowIcon`, where `*` is the Qt version, i.e:

1. `Qt5QWindowIcon`
2. `Qt5152QWindowIcon`
3. `Qt693QWindowIcon`

### Static Detection

While not a definitive method to prove usage, the app's executable's PE headers should include one of the modules listed under [Runtime Detection](#runtime-detection) in its import access table.

## Resources

- [Qt Website](https://qt.io)
- [Qt Source Repo](https://code.qt.io/cgit/qt/qtbase.git)
- [Qt Wikipedia Page](https://en.wikipedia.org/wiki/Qt_(software))
