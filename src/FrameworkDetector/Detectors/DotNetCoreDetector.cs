// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft .NET Core (DotNetCore)
/// Built according to TODO
/// </summary>
public class DotNetCoreDetector : IDetector 
{
    public string Name => nameof(DotNetCoreDetector);

    public string Description => "Microsoft .NET Core";

    public string FrameworkId => "DotNetCore";

    public DetectorCategory Category => DetectorCategory.Framework;

    public DotNetCoreDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("CoreClr.dll", productName: "Microsoft® .NET Core")
                .ContainsLoadedModule("System.Runtime.dll", productName: "Microsoft® .NET Core").GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Optional("Extra Modules", checks => checks
                .ContainsLoadedModule("clrjit.dll", productName: "Microsoft® .NET Core"))
            .BuildDefinition();
    }
}
