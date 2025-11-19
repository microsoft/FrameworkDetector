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

### Runtime Detection

Apps developed with Electron are essentially custom builds / forks of the Electron codebase, producing (then renaming) a generic `electron.exe` main executable.

The only reliable way of detecting an Electron app is to parse the main executable's PE headers and look for function exports from the `electron` static library (i.e. the presence of `@electron@@` in an exported function's name.)

Other proposed methods for detecting the use of Electron are inconsistent at best, or too likely to produce false-positives at worse. Whether intentionally or not, Electron app developers often (and to differing degrees) obfuscate that their app is Electron by renaming or removing other identifiers.

There are no Electron modules to detect, (i.e. no `electron.dll`) nor is it possible to conclude that an app is Electron-based by the presence of any of Electron's many (transitive) dependencies.

Electron-based main executables can sometimes be detected by checking the "Original Filename" metadata, but that may have been removed by the developer.

Electron uses HWND window class names prefixed by `Electron_` by default, but again, developers can (and do) rename those class names.

Due to the nature of how Electron apps are built, there is no way by runtime inspection to to reliably detect the specific version of Electron used.

### Static Detection

It is not possible to definitively determine the use of Electron by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries.

## Resources

- [Electron Website](https://electronjs.org)
- [Electron GitHub Source](https://github.com/electron/electron)
