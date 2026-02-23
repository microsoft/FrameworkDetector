// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft Edge PWA (EdgePWA).
/// Built according to docs/Frameworks/EdgePWA.md.
/// </summary>
public class EdgePWADetector : IDetector 
{
    public string Name => nameof(EdgePWADetector);

    public string Description => "Microsoft Edge PWA";

    public string FrameworkId => "EdgePWA";

    public DetectorCategory Category => DetectorCategory.Component;

    public EdgePWADetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsModule("pwahelper.exe").GetVersionFromModule())
            // OR
            .Optional("Windows", checks => checks
                .ContainsActiveWindow("pwahelper_wnd"))
            .BuildDefinition();
    }
}
