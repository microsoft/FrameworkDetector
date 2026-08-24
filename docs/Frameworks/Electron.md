---
id: Electron
title: Electron
description: An open-source framework for building desktop applications with web technologies using Chromium and Node.js.
website: https://electronjs.org
source: https://github.com/electron/electron
category: Framework
keywords: Framework Detector, Web, Electron, Chromium, Node.js, Desktop
ms.date: 11/19/2025
author: jonthysell
status: Detectable
---

# Electron (Electron)

## Summary

Electron is a free and open-source software framework designed to create desktop applications using web technologies that are rendered using a version of the Chromium browser engine and a back end using the Node.js runtime environment.

Electron also uses various APIs to enable functionality such as native integration with Node.js services and an inter-process communication module.

**Website:** [Electron Website](https://electronjs.org)

### Languages

**Framework Languages:** C++, JavaScript, Obj C++, Obj C

**Host App Languages:** C++

**Hosted App Languages:** Web (HTML, CSS, JavaScript and TypeScript)

### OS Support

Windows 10 and 11

### Dependencies

Electron depends on:

- [Google Chromium](https://chromium.org/Home)
- [Node.js](https://nodejs.org)

### Canonical Apps

- [Discord Desktop App](https://discord.com/download)
- [Figma Desktop App](https://figma.com/downloads)
- [Visual Studio Code](https://code.visualstudio.com/download)

## How to Detect

**Implementation:** [ElectronDetector](/src/FrameworkDetector/Detectors/ElectronDetector.cs)

Apps developed with Electron are essentially custom builds / forks of the Electron codebase, producing (then renaming) a generic `electron.exe` main executable. There are *zero* Electron-exclusive modules that are required by all Electron apps.

Many proposed methods for detecting the use of Electron are inconsistent at best, or too likely to produce false-positives at worse. Whether intentionally or not, Electron app developers often (and to differing degrees) obfuscate that their app is Electron by renaming or removing identifiers.

Due to the nature of how Electron apps are built, there is no way to reliably detect the specific version of Electron that an app is using.

### Runtime Detection

There is no runtime method of detecting the use of Electron in an app that is more reliable than the method described in [Static Detection](#static-detection).

As mentioned before, since there are no required Electron-exclusive modules, it is not possible to reliably determine the presence of Electron by checking the modules loaded by the running app process.

Electron apps *may* have HWND window class names prefixed by `Electron_` by default, but developers can (and do) rename those window class names.

### Static Detection

The *only* reliable way of detecting an Electron app is to parse the main executable's PE headers and look for function exports from the `electron` static library (i.e. the presence of `@electron@@` in an exported function's name).

Electron-based main executables can *sometimes* be detected by checking the "Original Filename" metadata, but that metadata is often removed by the app developer.

## Resources

- [Electron Website](https://electronjs.org)
- [Electron GitHub Source](https://github.com/electron/electron)
