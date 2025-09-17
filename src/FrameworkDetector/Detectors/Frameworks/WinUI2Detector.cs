// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class WinUI2Detector : IDetector
{
    public string Name => nameof(WinUI2Detector);

    public string Description => "WinUI 2 (for UWP)";

    public string FrameworkId => "WinUI2";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinUI2Detector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Microsoft UI XAML", checks => checks
                .ContainsLoadedModule("microsoft.ui.xaml.dll", versionRegex: @"^2\..*"))
            .Optional("Microsoft UI XAML Controls", checks => checks
                .ContainsLoadedModule("microsoft.ui.xaml.controls.dll", versionRegex: @"^2\..*"))
            .BuildDefinition();
    }
}
