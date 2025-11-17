# Framework Detector

A library and set of tools for detecting the frameworks and components used to build an application. e.g. Is this app a WPF app or a WinUI app? Does it use WebView2?

The Windows app eco-system is extraordinarily diverse with a long history, and developers often find very creative ways of using and combining the available app frameworks to meet their needs. As such, it can be a very challenging problem to programmatically detect what frameworks any particular app is using. Existing existing data sources are often plagued with inaccuracies, and lack detailed information as to how they made their determinations.

As such, the primary design goals of Framework Detector are to ensure the most specific, highest-quality, and openly auditable detections of frameworks or component used by applications. This includes:

1. Documenting the expected detection process for each supported framework (see the [docs](./docs/) folder)
2. Implementing framework detector definitions which compose common checks using an easy-to-verify, fluent API (i.e. `ContainsLoadedModule("TargetFramework.dll")`)
3. Capturing all relevant metadata (input data and checks run) alongside results, to ensure a clear paper-trail

Framework Detector is meant to support collaborative, iterative development. There are simply too many frameworks and components for a small team to catch every framework, let alone edge-cases with how those frameworks appear in any given app. Hence the decision to make both the documentation and code open-source, and to elicit the expertise of framework experts toward increasing the number of frameworks detected and making existing detectors more robust.

## Setup

1. Make sure you have the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed.
2. Note, if you intend to use Visual Studio, you'll need Visual Studio 2026 or greater.

## Build

Use the `build.ps1` script to build the project:

```ps
pwsh scripts\build.ps1
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
pwsh scripts\test.ps1
```

Otherwise you can manually test in Visual Studio or on the command-line:

```ps
dotnet test ./src/FrameworkDetector.sln
```

## Build CLI Release

Use the `build-cli-release.ps1` script to create a self-contained `FrameworkDetector.CLI.exe` tool:

```ps
pwsh scripts\build-cli-release.ps1
```

It will be available in the `bld\` folder.

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

## Output JSON Schema

The goal of the Framework Detector CLI's output JSON file is to be a self-contained data source for easy auditing. Broadly speaking, it includes all of the data that was discovered during an inspection and the results of every check for every detector ran.

### 1. Basic Metadata

The output JSON contains version of the CLI tool used, and with a timestamp for when the inspection occurred:

```json
{
  "toolName": "FrameworkDetector.CLI",
  "toolVersion": "0.9.0.25302",
  "timestamp": "2025-10-30T00:39:10.1711276Z",
  ...
}
```

From this we can easily compare the inspection results from different versions of the tool.

### 2. Data Sources Used

The output JSON contains all of the data about the app that was captured for use by the various detector checks:

```json
{
  ...
  "dataSources": {
    "processes": [
      {
        "processId": 20096,
        "mainWindowHandle": 529282,
        "mainWindowTitle": "WinUI 2 Gallery",
        "packageFullName": "Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
        "applicationUserModelId": "Microsoft.XAMLControlsGallery_8wekyb3d8bbwe!App",
        "loadedModules": [
          ...
          {
            "originalFilename": "Microsoft.UI.Xaml.dll",
            "fileVersion": "2.8.2501.31001",
            "productName": "Microsoft.UI.Xaml",
            "productVersion": "2.8.2501.31001",
            "filename": "Microsoft.UI.Xaml.dll"
          },
          ...
        ],
        "activeWindows": [
          ...
          {
            "className": "Windows.UI.Core.CoreWindow",
            "text": "WinUI 2 Gallery",
            "isVisible": true
          }
          ...
        ],
        "importedFunctions": [
            {
            "moduleName": "AppUIBasics.dll",
            "functions": [
              {
                "name": "RHBinder__ShimExeMain",
                "delayLoaded": false
              }
            ]
          }
        ],
        "exportedFunctions": [],
        "appPackageMetadata": {
          "appDisplayName": "WinUI 2 Gallery",
          "appDescription": "WinUI 2 Gallery",
          "appPackageFamilyName": "Microsoft.XAMLControlsGallery_8wekyb3d8bbwe",
          "packageMetadata": {
            "id": {
              "architecture": "X64",
              "name": "Microsoft.XAMLControlsGallery",
              "familyName": "Microsoft.XAMLControlsGallery_8wekyb3d8bbwe",
              "fullName": "Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
              "publisher": "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US",
              "publisherId": "8wekyb3d8bbwe",
              "resourceId": "",
              "version": "1.2.26.0"
            },
            "packagePublisherDisplayName": "Microsoft Corporation",
            "packageDisplayName": "WinUI 2 Gallery",
            "packageDescription": "",
            "installedPath": "C:\\Program Files\\WindowsApps\\Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
            "packageEffectiveExternalPath": "",
            "packageEffectivePath": "C:\\Program Files\\WindowsApps\\Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
            "installedDate": "2025-10-29T17:39:01.0756403-07:00",
            "flags": {
              "isBundle": false,
              "isDevelopmentMode": false,
              "isFramework": false,
              "isOptional": false,
              "isResourcePackage": false,
              "isStub": false
            },
            "dependencies": [
              ...
              {
                "id": {
                  "architecture": "X64",
                  "name": "Microsoft.UI.Xaml.2.8",
                  "familyName": "Microsoft.UI.Xaml.2.8_8wekyb3d8bbwe",
                  "fullName": "Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
                  "publisher": "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US",
                  "publisherId": "8wekyb3d8bbwe",
                  "resourceId": "",
                  "version": "8.2501.31001.0"
                },
                "packagePublisherDisplayName": "Microsoft Platform Extensions",
                "packageDisplayName": "Microsoft.UI.Xaml.2.8",
                "packageDescription": "Microsoft.UI.Xaml",
                "installedPath": "C:\\Program Files\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
                "packageEffectiveExternalPath": "",
                "packageEffectivePath": "C:\\Program Files\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
                "installedDate": "2025-02-11T12:56:26.5967232-08:00",
                "flags": {
                  "isBundle": false,
                  "isDevelopmentMode": false,
                  "isFramework": true,
                  "isOptional": false,
                  "isResourcePackage": false,
                  "isStub": false
                },
                "dependencies": []
              },
              ...
            ]
        },
        "originalFilename": "AppUIBasics.exe",
        "fileVersion": "1.0.0.0",
        "productName": "AppUIBasics",
        "productVersion": "1.0.0.0",
        "filename": "AppUIBasics.exe"
      }
    ]
  },
  ...
}
```

The primary data source is the inspected process itself, including basic file info of the process, the modules that were loaded by the process at runtime, functions imported from those modules, the active windows, the functions exported by the process, and any APPX metadata.

### 3. Detector Results

The output JSON contains the results from every detector, including the results of the individual checks that contributed to the result:

```json
{
  ...
  "detectorResults": [
    ...
    {
      "detectorName": "WinUI2Detector",
      "detectorDescription": "WinUI 2 for UWP",
      "detectorVersion": "0.9.0.25302",
      "frameworkId": "WinUI2",
      "frameworkFound": true,
      "frameworkVersion": "2.8.2501.31001",
      "hasAnyPassedChecks": true,
      "detectorStatus": "completed",
      "checkResults": [
        {
          "checkDefinition": {
            "dataSourceIds": [
              "processes"
            ],
            "description": "Find package dependency  has \"Microsoft.UI.Xaml\"",
            "name": "ContainsDependentPackageCheck",
            "isRequired": true,
            "groupName": "Dependent Package"
          },
          "checkStatus": "completedPassed",
          "checkInput": {
            "packageFullName": "Microsoft.UI.Xaml"
          },
          "checkOutput": {
            "packageFound": {
              "id": {
                "architecture": "X64",
                "name": "Microsoft.UI.Xaml.2.8",
                "familyName": "Microsoft.UI.Xaml.2.8_8wekyb3d8bbwe",
                "fullName": "Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
                "publisher": "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US",
                "publisherId": "8wekyb3d8bbwe",
                "resourceId": "",
                "version": "8.2501.31001.0"
              },
              "packagePublisherDisplayName": "Microsoft Platform Extensions",
              "packageDisplayName": "Microsoft.UI.Xaml.2.8",
              "packageDescription": "Microsoft.UI.Xaml",
              "installedPath": "C:\\Program Files\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
              "packageEffectiveExternalPath": "",
              "packageEffectivePath": "C:\\Program Files\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
              "installedDate": "2025-02-11T12:56:26.5967232-08:00",
              "flags": {
                "isBundle": false,
                "isDevelopmentMode": false,
                "isFramework": true,
                "isOptional": false,
                "isResourcePackage": false,
                "isStub": false
              },
              "dependencies": []
            }
          }
        },
        {
          "checkDefinition": {
            "dataSourceIds": [
              "processes"
            ],
            "description": "Find module \"Microsoft.UI.Xaml.dll\" with version \">=2.0 <3.0\"",
            "name": "ContainsLoadedModuleCheck",
            "isRequired": true,
            "groupName": "Main Module"
          },
          "checkStatus": "completedPassed",
          "checkInput": {
            "filename": "Microsoft.UI.Xaml.dll",
            "fileVersionRange": ">=2.0 <3.0"
          },
          "checkOutput": {
            "moduleFound": {
              "originalFilename": "Microsoft.UI.Xaml.dll",
              "fileVersion": "2.8.2501.31001",
              "productName": "Microsoft.UI.Xaml",
              "productVersion": "2.8.2501.31001",
              "filename": "Microsoft.UI.Xaml.dll"
            }
          }
        }
      ]
    },
    ...
  ]
  ...
}
```

## License

MIT
