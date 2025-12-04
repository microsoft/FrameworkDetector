// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft DirectUI (DUI).
/// Built according to docs/Frameworks/DUI.md.
/// </summary>
public class DUIDetector : IDetector 
{
    public string Name => nameof(DUIDetector);

    public string Description => "Microsoft DirectUI";

    public string FrameworkId => "DUI";

    public DetectorCategory Category => DetectorCategory.Framework;

    public DUIDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsModule("dui70.dll"))
            .BuildDefinition();
    }
}
