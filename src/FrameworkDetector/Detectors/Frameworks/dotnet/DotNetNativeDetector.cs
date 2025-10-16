// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class DotNetNativeDetector : IDetector 
{
    public string Name => nameof(DotNetNativeDetector);

    public string Description => "Microsoft .NET Native";

    public string FrameworkId => "DotNetNative";

    public DetectorCategory Category => DetectorCategory.Framework;

    public DotNetNativeDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("mrt100_app.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
