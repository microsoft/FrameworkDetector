// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for WinUI 3 for Windows App SDK (WinUI3).
/// Built according to docs/Frameworks/WinUI3.md.
/// </summary>
public class WinUI3Detector : IDetector
{
    public string Name => nameof(WinUI3Detector);

    public string Description => "WinUI 3 for Windows App SDK";

    public string FrameworkId => "WinUI3";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinUI3Detector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsModule("Microsoft.UI.Xaml.dll", fileVersionRange: ">= 3.0").GetVersionFromModule())
            .BuildDefinition();
    }
}
