# Framework Detector

A library and set of tools for detecting the frameworks and components used to build an application. e.g. Is this app a WPF app or a WinUI app? Does it use WebView2?

## Setup

1. Make sure you have the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed.
2. If using Visual Studio Stable, enable `Use previews of the .NET SDK (requires restart)` under `Tools->Options->Environment->Preview Features`.

## Build

Use the build script to create a self-contained `FrameworkDetector.CLI.exe` tool:

```ps
scripts\build.ps1
```

It will be available in the `bld\` folder.

Otherwise you can manually build the solution at `src\FrameworkDetector.sln` with Visual Studio or on the command-line:

```ps
dotnet msbuild ./src/FrameworkDetector.sln
```

You can also navigate to the `src\FrameworkDetector.CLI` folder and use the dotnet run command:

```ps
dotnet run [parameters]
```

## Usage

### Inspect by Process Id (PID)

```ps
FrameworkDetector.CLI.exe inspect --processId ###
```

*OR*

```ps
FrameworkDetector.CLI.exe inspect --pid ###
```

### Inspect by Process Name

```ps
FrameworkDetector.CLI.exe inspect --processName ###
```

## Project Structure

### FrameworkDetector

A library of detectors and an engine to run them against specific data sources (like processes) and provide a report of results.

Key interfaces are `IDetector`, `IDataSource`, `ICheckDefinition`, and `IDetectorCheckResult`.

A Fluent API surface is used to construct detectors. For instance, a basic `IDetector` implementation for WPF may look something like:

```cs
    // Main WPF Detector method within IDetector implementation
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Presentation Framework", checks => checks
                .ContainsLoadedModule("PresentationFramework.dll")
                .ContainsLoadedModule("PresentationCore.dll"))
            .BuildDefinition();
    }
```

A Detector requires _at least_ one Required check group, but can have many. Any required check group that passes will indicate detection. All checks within a group must pass for that group to pass.

Optional groups can also be defined, they have no impact on detection. They are not aggregated, just tagged with the group name.

### FrameworkDetector.CLI

A commandline tool to initialize and run the library to inspect a specific process, see [Usage](#usage) above.

## Debugging

To debug a specific detector add a conditional breakpoint in the `DetectionEngine` class, specifically in the `DetectAgainstSourcesAsync` method looking at the `detector.Info.Name`:

```
detector.Info.Name == nameof(FrameworkDetector.Detectors.WPFDetector) // Link:DetectionEngine.cs#L66
```

Then look into the calls for `PerformCheckAsync` that get called from there.

## License

MIT
