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

## Test

Use the test script to run all tests:

```ps
scripts\test.ps1
```

Otherwise you can manually test in Visual Studio or on the command-line:

```ps
dotnet test ./src/FrameworkDetector.sln
```

## Usage

### Inspect all Applications

By default, filters to processes which have a Main Window Handle or visible child window.

```ps
FrameworkDetector.CLI.exe all
```

You can use `--filterWindowProcesses false` to inspect all processes. Some processes in either case may require admin rights to inspect.

### Inspect by Process Id (PID)

```ps
FrameworkDetector.CLI.exe inspect --processId ###
```

*OR*

```ps
FrameworkDetector.CLI.exe inspect -pid ###
```

### Inspect by Process Name

```ps
FrameworkDetector.CLI.exe inspect --processName ###
```

### Run a Process and Inspect it

```ps
FrameworkDetector.CLI.exe run -exe "C:\Path\To\MyApp.exe"
```

*OR*

Use a full package name, like for the WinUI 3 Gallery:

```ps
FrameworkDetector.CLI.exe run -pkg Microsoft.WinUI3ControlsGallery_2.7.0.0_x64__8wekyb3d8bbwe
```

You can use the PowerShell `Get-AppxPackage` command to find the full package name of installed MSIX packages.

### Output

Use the `-o` parameter to specify the output file (or folder for `all`). This will save detailed results in a JSON file format. E.g.

```ps
FrameworkDetector.CLI.exe inspect -pid ### -o myresults.json
```

When using the `all` command, a file will be created for each process in the folder specified by the `-o` parameter. You can use the `--outputFileTemplate` parameter to customize the resulting file names.

Use `all --help` to see the available replacement options (case-sensitive).

```ps
FrameworkDetector.CLI.exe all -o results\ --outputFileTemplate "{processName}_{processId}.json"
```

### Verbosity

You can control the verbosity of the output with the `--verbosity` (or `-v`) parameter. Options are:

- `quiet`: Only show errors
- `minimal`: Show errors and essential information
- `normal`: Console shows found frameworks only (default when unspecified)
- `detailed`: Console shows found and unfound frameworks
- `diagnostic`: Show all information, including verbose diagnostic output of checks in table

If you specify `-v` without a value, it defaults to `diagnostic`.

### Documentation

You can view the list of available detectors and if detailed documentation is available for them by using the `docs` command:

```ps
FrameworkDetector.CLI.exe docs
```

This will provide a table of detector ids, names, and whether documentation is available. Add the id as an argument to retrieve the detailed specification of how that particular framework is detected (if available).

```ps
FrameworkDetector.CLI.exe docs WinUI3
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

## Dump of data sources

If you're looking to write a new detector or improve one and need to understand the data sources available, you can use the `dump` command to get a comprehensive view of all data available for a specific process:

```ps
FrameworkDetector.CLI.exe dump -pid ###
```

This can also output (with `-o`) a JSON file with all the information available for that process used by detectors without running the detection logic.

## License

MIT
