---
id: OutputJsonSchema
title: Output JSON Schema
description: The format of the JSON output files produced by FrameworkDetector.CLI.
keywords: Framework Detector
ms.date: 02/03/2026
author: jonthysell
---

# Output JSON Schema

The goal of the Framework Detector CLI's output JSON file is to be a self-contained data source for easy auditing. Broadly speaking, it includes all of the data that was discovered during an inspection and the results of every check for every detector ran.

## 1. Basic Metadata

The output JSON contains version of the CLI tool used, and with a timestamp for when the inspection occurred:

```json
{
  "toolName": "FrameworkDetector.CLI",
  "toolVersion": "0.10.26013.1530",
  "toolArguments": "run -o .\\output\\WinUI2Gallery.json -aumid Microsoft.XAMLControlsGallery_8wekyb3d8bbwe!App --includeChildren -wait 5000",
  "timestamp": "2026-01-14T22:07:01.1237763Z",
  ...
}
```

From this we can easily compare the inspection results from different versions of the tool.

## 2. Inputs Used

The output JSON contains all of the data about the target app, presented as a set of various "inputs", which will then be available for the various detector checks:

```json
{
  ...
  "inputs": {
    "processes": [
      {
        "processId": 34276,
        "mainModule": {
          "fileName": "AppUIBasics.exe",
          "isLoaded": true
        },
        "activeWindows": [
          ...
          {
            "className": "Windows.UI.Core.CoreWindow",
            "isVisible": true
          }
          ...
        ],
        "loadedModules": [
          ...
          {
            "fileName": "Microsoft.UI.Xaml.dll",
            "originalFileName": "Microsoft.UI.Xaml.dll",
            "fileVersion": "2.8.2501.31001",
            "productName": "Microsoft.UI.Xaml",
            "productVersion": "2.8.2501.31001",
            "isLoaded": true
          },
          ...
        ],
        "customData": {},
        "mainWindowHandle": 0,
        "packageFullName": "Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
        "applicationUserModelId": "Microsoft.XAMLControlsGallery_8wekyb3d8bbwe!App"
      }
    ],
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
        "customData": {}
      }
    ],
    "installedPackages": [
      {
        "displayName": "WinUI 2 Gallery",
        "description": "",
        "familyName": "Microsoft.XAMLControlsGallery_8wekyb3d8bbwe",
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
          "installedPath": "%ProgramW6432%\\WindowsApps\\Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
          "packageEffectivePath": "%ProgramW6432%\\WindowsApps\\Microsoft.XAMLControlsGallery_1.2.26.0_x64__8wekyb3d8bbwe",
          "packageEffectiveExternalPath": "",
          "installedDate": "2025-07-22T17:22:17.2533422-07:00",
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
              "installedPath": "%ProgramW6432%\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
              "packageEffectivePath": "%ProgramW6432%\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
              "packageEffectiveExternalPath": "",
              "installedDate": "2025-03-21T14:48:13.4458722-07:00",
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
        "customData": {}
      }
    ]
  },
  ...
}
```

The primary inputs include metadata about the app's executable (basic metadata, functions imported, etc.), metadata about the running process (loaded modules, active windows, etc.), and metadata about the app's package (package identity, dependencies, etc.).

## 3. Detector Results

The output JSON contains the results from every detector, including the results of the individual checks that contributed to the result:

```json
{
  ...
  "detectorResults": [
    ...
    {
      "detectorName": "WinUI2Detector",
      "detectorDescription": "WinUI 2 for UWP",
      "detectorAssemblyName": "FrameworkDetector",
      "detectorAssemblyVersion": "0.10.26013.1530",
      "frameworkId": "WinUI2",
      "frameworkFound": true,
      "frameworkVersion": "2.8.2501.31001",
      "hasAnyPassedChecks": true,
      "detectorStatus": "completed",
      "checkResults": [
        {
          "checkDefinition": {
            "dataSources": [
              "IPackageDataSource"
            ],
            "description": "Find package dependency  has \"Microsoft.UI.Xaml\"",
            "name": "ContainsPackagedDependencyCheck",
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
              "installedPath": "%ProgramW6432%\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
              "packageEffectivePath": "%ProgramW6432%\\WindowsApps\\Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe",
              "packageEffectiveExternalPath": "",
              "installedDate": "2025-03-21T14:48:13.4458722-07:00",
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
            "dataSources": [
              "IModulesDataSource"
            ],
            "description": "Find module \"Microsoft.UI.Xaml.dll\" with version \">=2.0 <3.0\"",
            "name": "ContainsModuleCheck",
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
              "fileName": "Microsoft.UI.Xaml.dll",
              "originalFileName": "Microsoft.UI.Xaml.dll",
              "fileVersion": "2.8.2501.31001",
              "productName": "Microsoft.UI.Xaml",
              "productVersion": "2.8.2501.31001",
              "isLoaded": true
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