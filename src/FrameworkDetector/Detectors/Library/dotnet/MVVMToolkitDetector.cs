// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class MVVMToolkitDetector : IDetector
{
    public string Name => nameof(MVVMToolkitDetector);

    public string Description => "MVVM Toolkit";

    public string FrameworkId => "MVVM";

    public DetectorCategory Category => DetectorCategory.Library;

    public MVVMToolkitDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        // MVVM Toolkit
        return this.Create()
            .Required("New Version", checks => checks
                .ContainsModule("CommunityToolkit.MVVM.dll"))
            // OR
            .Required("Old Version", checks => checks
                .ContainsModule("Microsoft.Toolkit.MVVM.dll"))
            .BuildDefinition();
    }
}