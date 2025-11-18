// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Windows App SDK (WinAppSDK).
/// Built according to TODO.
/// </summary>
public class WindowsAppSDKDetector : IDetector
{
    public string Name => nameof(WindowsAppSDKDetector);

    public string Description => "Windows App SDK";

    public string FrameworkId => "WinAppSDK";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WindowsAppSDKDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            // Use Package Info first if found
            .Required("Dependent Package", checks => checks
                .ContainsPackagedDependency("Microsoft.WindowsAppRuntime").GetVersionFromPackageIdentity(PackageVersionType.FullNameSpecial))
            // Otherwise look for key modules
            .Required("Resources Module", checks => checks
                .ContainsLoadedModule("Microsoft.Windows.ApplicationModel.Resources.dll"))
            .Required("Framework Package", checks => checks
                .ContainsLoadedModule("Microsoft.WindowsAppRuntime.Bootstrap.dll"))
            // TODO: There's a number of modules here that we could check for...
            .BuildDefinition();
    }
}
