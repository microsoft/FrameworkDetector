// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft .NET (DotNet)
/// Built according to TODO
/// </summary>
public class DotNetDetector : IDetector 
{
    public string Name => nameof(DotNetDetector);

    public string Description => "Microsoft .NET";

    public string FrameworkId => "DotNet";

    public DetectorCategory Category => DetectorCategory.Framework;

    public DotNetDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsModule("CoreClr.dll", productName: "Microsoft® .NET").GetVersionFromModule())
            .Optional("Extra Modules", checks => checks
                .ContainsModule("clrjit.dll", productName: "Microsoft® .NET"))
            .BuildDefinition();
    }
}
