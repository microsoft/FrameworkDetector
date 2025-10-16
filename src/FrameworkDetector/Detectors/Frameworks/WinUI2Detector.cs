// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for WinUI 2 for UWP (WinUI2).
/// Built according to docs/Frameworks/WinUI2.md.
/// </summary>
public class WinUI2Detector : IDetector
{
    public string Name => nameof(WinUI2Detector);

    public string Description => "WinUI 2 for UWP";

    public string FrameworkId => "WinUI2";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinUI2Detector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("Microsoft.UI.Xaml.dll", fileVersionRange: ">=2.0 <3.0").GetVersionFromModule())
            .BuildDefinition();
    }
}
