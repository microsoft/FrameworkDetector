---
id: BuildAndTest
title: Build and Test FrameworkDetector
description: How to build and/or test FrameworkDetector.
keywords: Framework Detector
ms.date: 02/03/2026
author: jonthysell
---

# Build and Test FrameworkDetector

## Setup

1. Install the [.NET 10 SDK](https://aka.ms/dotnet)
2. *Optional:* Install [PowerShell 7](https://aka.ms/powershell) to use the existing build/test scripts
3. *Optional:* Install [Visual Studio 2026](http://aka.ms/vs/download) if you intend to use Visual Studio

## Build

Use the `build.ps1` script to build the project:

```ps
pwsh -Command scripts\build.ps1
```

Otherwise you can manually build the solution at `src\FrameworkDetector.sln` with Visual Studio or on the command-line:

```ps
dotnet build ./src/FrameworkDetector.sln
```

You can also navigate to the `src\FrameworkDetector.CLI` folder and use the dotnet run command:

```ps
dotnet run [parameters]
```

## Test

Use the `test.ps1` script to run all tests:

```ps
pwsh -Command scripts\test.ps1
```

Otherwise you can manually test in Visual Studio or on the command-line:

```ps
dotnet test ./src/FrameworkDetector.sln
```

## Build CLI Release

Use the `build-cli-release.ps1` script to create a self-contained `FrameworkDetector.CLI.exe` tool:

```ps
pwsh -Command scripts\build-cli-release.ps1
```

It will be available in the `bld\` folder.

## Build NuGet Release

Use the `build-nuget-release.ps1` script to create the `FrameworkDetector` NuGet package:

```ps
pwsh -Command scripts\build-nuget-release.ps1
```

It will be available in the `bld\` folder.

## Debugging

To debug a specific detector add a conditional breakpoint in the `DetectionEngine` class, specifically in the `DetectAgainstSourcesAsync` method looking at the `detector.Info.Name`:

```
detector.Info.Name == nameof(FrameworkDetector.Detectors.WPFDetector) // Link:DetectionEngine.cs#L66
```

Then look into the calls for `PerformCheckAsync` that get called from there.