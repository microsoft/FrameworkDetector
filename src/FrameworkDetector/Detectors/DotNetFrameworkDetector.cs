// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft .NET Framework (DotNetFramework)
/// Built according to TODO
/// </summary>
public class DotNetFrameworkDetector : IDetector 
{
    public string Name => nameof(DotNetFrameworkDetector);

    public string Description => "Microsoft .NET Framework";

    public string FrameworkId => "DotNetFramework";

    public DetectorCategory Category => DetectorCategory.Framework;

    public DotNetFrameworkDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("CLR Module", checks => checks
                .ContainsModule("clr.dll", productName: "Microsoft® .NET Framework").GetVersionFromModule())
            // OR
            .Required("mscorlib Module", checks => checks
                .ContainsModule("mscorlib.dll", productName: "Microsoft® .NET Framework", checkForNgenModule: true).GetVersionFromModule())
            .Optional("Extra Modules", checks => checks
                .ContainsModule("clrjit.dll", productName: "Microsoft® .NET Framework")
                .ContainsModule("mscorjit.dll", productName: "Microsoft® .NET Framework"))
            .BuildDefinition();
    }
}
