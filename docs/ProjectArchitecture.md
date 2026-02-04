---
id: ProjectArchitecture
title: Project Architecture
description: Framework Detector's general project architecture.
keywords: Framework Detector
ms.date: 02/03/2026
author: jonthysell
---

# Project Architecture

At a high-level, the FrameworkDetector library inspects a target app, by taking a .NET type (a [`FileInfo`](https://learn.microsoft.com/dotnet/api/system.io.fileinfo?view=net-10.0) of app's executable file on disk, a [`Package`](https://learn.microsoft.com/uwp/api/windows.applicationmodel.package?view=winrt-26100) from the app
pp's package installed to the system, or a [`Process`](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-10.0) for the app's currently running process) and producing a `ToolRunResult` which contains both all of the useful metadata the library could determine about the target, along with all of the frameworks and dependencies that were detected.

Within FrameworkDetector we have an `InputHelper` class which knows how to construct a collection of `IInputType` objects based on the incoming target app's object for all the inputs that type can provide. For instance, an app's `Process` may be able to provide a `ProcessInput`, an `ExecutableInput` from its MainModule, and an `InstalledPackageInput` from the installed package registered on the system.

Each `IInputType<T>` knows how to extract relevant data from the target object of type `T` via a factory method `CreateAndInitializeDataSourcesAsync`. This method extracts the required information to populate the various `IDataSource` interfaces for that input. It stores the DataSource information in various `*Metadata` records which are **not** dependent on the underlying CLR instances. This allows any `IInputType` object to be serialized easily and enables the FrameworkDetector to re-process detections later without having to have access to the original target of inspection.

Inputs are then passed into the `DetectionEngine` and can be looped through by each type of Detector (one per app framework) and perform that detector's set of checks. Each check can easily identify the data it needs by inspecting if the `IInputType` implements the given `IDataSource` interface it requires. If that data is available, then the check can run and see if input passes. In the end, all of the `DetectorCheckResults`s get rolled up into `DetectorResult`s, which then get rolled up (along with the original `IInputType`s) into a final `ToolRunResult`.

A more verbose end-to-end flow looks like:

1. Take one of several supported .NET types representing the target app, currently `FileInfo`, `Process`, or `Package`
2. Determine the set of one or more `IInputType` types (aka Inputs) that can be created, currently: `ExecutableInput`, `ProcessInput`, and/or `InstalledPackageInput`
3. Create each Input, collecting and caching all useful metadata (de-coupling it from the original .NET type) and providing that metadata through various `IDataSource` interfaces (aka Data Sources)
4. Pass the Inputs to the `DetectionEngine` (aka Engine), which will then try to run the registered `IDetector`s (one per framework, aka Detectors) in parallel against the Inputs
5. Each Detector provides a `DetectorDefinition` (groups of required/optional `ICheckDefinition`s that are indicate the target framework is present, aka Checks)
6. The Engine gives all of the Inputs to each Check
7. The Check pulls out the Data Source(s) it can from each Input and performs its test
8. The results of each Check `DetectorCheckResult` is rolled up into final `DetectorResult` for the Detector, if any of the required Checks groups pass completely, the framework is declared detected
9. All of the Inputs and Results are returned to the caller as a `ToolRunResult`

## Project Structure

This repo is divided into the following projects:

### FrameworkDetector

The library of detectors and an engine to run them against specific target apps (like processes) and provide a report of results.

Key interfaces are `IDetector`, `IDataSource`, `ICheckDefinition`, `IDetectorCheckResult`, and `IInputType`.

#### Detectors

A Fluent API surface is used to construct detectors. For instance, a basic `IDetector` implementation for WPF may look something like:

```cs
    // Main WPF Detector method within IDetector implementation
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Presentation Framework", checks => checks
                .ContainsModule("PresentationFramework.dll")
                .ContainsModule("PresentationCore.dll"))
            .BuildDefinition();
    }
```

A Detector requires _at least_ one Required check group, but can have many. Any required check group that passes will indicate detection. All checks within a group must pass for that group to pass.

Optional groups can also be defined, they have no impact on detection. They are not aggregated, just tagged with the group name.

### FrameworkDetector.CLI

A command-line tool which uses the FrameworkDetector library to inspect target apps, see [FrameworkDetector.CLI Usage](./CliUsage.md).

### FrameworkDetector.Test

Unit and integration tests for the FrameworkDetector library.
