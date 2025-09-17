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
            .Required("", checks => checks
                .ContainsLoadedModule("Microsoft.UI.Xaml.dll", fileVersionRange: ">= 3.0"))
            .Optional("Extra Modules", checks => checks
                .ContainsLoadedModule("Microsoft.UI.Xaml.Controls.dll", fileVersionRange: ">= 3.0"))
            .BuildDefinition();
    }
}
