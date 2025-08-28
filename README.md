# Framework Detector

A library and set of tools for detecting the frameworks and components used to build an application. e.g. Is this app a WPF app or a WinUI app? Does it use WebView2?

## Setup

1. Make sure you have the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed.

2. If using Visual Studio Stable, enable "Use previews of the .NET SDK (requires restart)" under Tools->Options->Environment->Preview Features.

## Usage

```ps
dotnet run --project ./src/FrameworkDetector.CLI -- --pid ###
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
                .ContainsModule("PresentationFramework.dll")
                .ContainsModule("PresentationCore.dll"))
            .BuildDefinition();
    }
```

A Detector requires _at least_ one Required check group, but can have many. Any required check group that passes will indicate detection. All checks within a group must pass for that group to pass.

Optional groups can also be defined, they have no impact on detection. They are not aggregated, just tagged with the group name.

### FrameworkDetector.CLI

A commandline tool to initialize and run the library to detect a specific process, see Usage above.

## Licence

MIT
