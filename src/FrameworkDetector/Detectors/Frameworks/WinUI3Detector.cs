// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class WinUI3Detector : IDetector
{
    public string Name => nameof(WinUI3Detector);

    public string Description => "WinUI 3 (for WinAppSDK)";

    public string FrameworkId => "WinUI3";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinUI3Detector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Microsoft UI XAML", checks => checks
                .ContainsLoadedModule("microsoft.ui.xaml.dll", versionRegex: @"^3\..*"))
            .Optional("Microsoft UI XAML Controls", checks => checks
                .ContainsLoadedModule("microsoft.ui.xaml.controls.dll", versionRegex: @"^3\..*"))
            .BuildDefinition();
    }
}
