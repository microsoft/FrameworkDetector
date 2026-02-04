---
id: PluginSupport
title: Plugin Support
description: Extend FrameworkDetector by writing custom plugin modules.
keywords: Framework Detector
ms.date: 02/03/2026
author: jonthysell
---

# Plugin Support

FrameworkDetector (and therefore FrameworkDetector.CLI) is extendable via a simple plugin system. Developers need only build their own .NET library which implements their own `IDetector`s and/or `ICustomDataFactory`s.

## Plugin Project

A FrameworkDetector plugin project is just a .NET library which:

1. Targets `net10.0-windows10.0.19041`
2. Depends on the `FrameworkDetector` NuGet (private and excluding runtime assets)
3. Has `EnableDynamicLoading` enabled

**Example:**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AssemblyName>MyPlugin</AssemblyName>
    <RootNamespace>MyPlugin</RootNamespace>
    <TargetFramework>net10.0-windows10.0.19041</TargetFramework>
    <IsPublishable>false</IsPublishable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="FrameworkDetector" Version="0.10.26013.1530">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

### Custom IDetectors

If you want to extend FrameworkDetector to detect a new framework that it doesn't already support, you'll want to create a new `IDetector`, i.e.:

```cs
public class MyFrameworkDetector : IDetector 
{
  public string Name => nameof(MyFrameworkDetector);
  public string Description => "MyFramework";
  public string FrameworkId => "MyFramework";
  public DetectorCategory Category => DetectorCategory.Framework;

  public DUIDetector() { }
  
  public DetectorDefinition CreateDefinition()
  {
    return this.Create()
        .Required("", checks => checks
            .ContainsModule("MyFramework.dll"))
        .BuildDefinition();
  }
}
```

Any `IDetector`s you implement in a plugin library will be automatically added to the list of detectors run by FrameworkDetector. 

### Custom ICustomDataFactory

If you want to extend Framework Detectors's existing input types with new custom data (to be included in the JSON output, or to be used by a custom check) you'll want to create a new `ICustomDataFactory` for the .NET type the input is based on. Currently, `FileInfo`, `Package`, and `Process` are supported.

For example, if you wanted to extend `ExecutableInput`, which is built from a `FileInfo`, you'd want to create a `ICustomDataFactory<FileInfo>`, i.e.:

```cs
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

using FrameworkDetector.Inputs;

public class MyCustomDataFactory : ICustomDataFactory<FileInfo>
{
  public string Key => "myCustomData"; // Used to group your custom data under customData

  public async IAsyncEnumerable<object> CreateCustomDataAsync(FileInfo input, bool? isLoaded, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var data = new Dictionary<string, string?>();
    data["creationTimeUtc"] = input.CreationTimeUtc.ToString();

    yield return data;
  }
}
```

Any `ICustomDataFactory`s you implement in a plugin library will be automatically be run when FrameworkDetector builds the inputs for a target app. The `Key` property will be used to group your custom data object(s) under the `customData` property for the matching input. In the given example, you'd find something like the following added to the `ExecutableInput`s in the JSON output:

```json
{
  ...
  "inputs": {
    "executables": [
      {
        "executableMetadata": {
          "fileName": "AppUIBasics.exe",
          "originalFileName": "AppUIBasics.exe",
          "fileVersion": "1.0.0.0",
          "productName": "AppUIBasics",
          "productVersion": "1.0.0.0",
          "isLoaded": true
        },
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
        "importedModules": [
          {
            "fileName": "AppUIBasics.dll",
            "originalFileName": "AppUIBasics.exe",
            "fileVersion": "1.0.0.0",
            "productName": "AppUIBasics",
            "productVersion": "1.0.0.0",
            "isLoaded": false
          }
        ],
        "customData": {
          "myCustomData": [
            {
              "creationTimeUtc": "7/23/2025 12:21:58 AM"
            }
          ]
        }
      }
      ...
    ],
  },
  ...
}
```

## Loading your plugin in FrameworkDetector.CLI

To load your plugin in FrameworkDetector.CLI, simply provide the path to the plugin's dll with `--pluginFile`, i.e.:

`FrameworkDetector.CLI.exe inspect process --pluginFile path\to\MyPlugin.dll -pid 100`

**Note:** You can specify `--pluginFile` more than once to load multiple plugins.
