// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

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
            .Required("CoreClr", checks => checks
                .ContainsLoadedModule(@"coreclr\.dll"))
            .Optional("clrjit", checks => checks
                .ContainsLoadedModule(@"clrjit\.dll"))
            .BuildDefinition();
    }
}
