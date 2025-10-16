// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Common Controls (ComCtl).
/// Built according to docs/Library/ComCtl.md.
/// </summary>
public class ComCtlDetector : IDetector 
{
    public string Name => nameof(ComCtlDetector);

    public string Description => "Microsoft Common Controls";

    public string FrameworkId => "ComCtl";

    public DetectorCategory Category => DetectorCategory.Library;

    public ComCtlDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("comctl32.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
