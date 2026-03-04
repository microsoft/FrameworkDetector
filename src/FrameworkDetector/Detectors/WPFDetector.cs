// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Windows Presentation Foundation (WPF).
/// Built according to docs/Frameworks/WPF.md.
/// </summary>
public class WPFDetector : IDetector 
{
    public string Name => nameof(WPFDetector);

    public string Description => "Windows Presentation Foundation";

    public string FrameworkId => "WPF";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WPFDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Framework Module", checks => checks
                .ContainsModule("PresentationFramework.dll", checkForNgenModule: true).GetVersionFromModule())
            // OR
            .Required("Core Module", checks => checks
                .ContainsModule("PresentationCore.dll", checkForNgenModule: true).GetVersionFromModule())
            // OR
            .Required("PresentationNative (.NET)", checks => checks
                .ContainsModule("PresentationNative_cor3.dll").GetVersionFromModule())
            // OR
            .Required("PresentationNative (.NET Framework)", checks => checks
                .ContainsModule("PresentationNative_v0400.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
