// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft .NET Native (DotNetNative)
/// Built according to TODO
/// </summary>
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
                .ContainsModule("mrt100_app.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
