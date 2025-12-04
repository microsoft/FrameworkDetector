// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Windows App SDK (WinAppSDK).
/// Built according to docs/Frameworks/WinAppSDK.md.
/// </summary>
public class WinAppSDKDetector : IDetector
{
    public string Name => nameof(WinAppSDKDetector);

    public string Description => "Windows App SDK";

    public string FrameworkId => "WinAppSDK";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinAppSDKDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            // Use Package Info first if found
            .Required("Dependent Package", checks => checks
                .ContainsPackagedDependency("Microsoft.WindowsAppRuntime").GetVersionFromPackageFullName())
            // Otherwise look for key modules
            .Required("Resources Module", checks => checks
                .ContainsModule("Microsoft.Windows.ApplicationModel.Resources.dll"))
            .Required("Framework Package", checks => checks
                .ContainsModule("Microsoft.WindowsAppRuntime.Bootstrap.dll"))
            // TODO: There's a number of modules here that we could check for...
            .BuildDefinition();
    }
}
