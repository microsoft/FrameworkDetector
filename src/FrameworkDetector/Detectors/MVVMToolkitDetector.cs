// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the MVVM Toolkit (MVVMToolkit).
/// Built according to docs/Frameworks/MVVMToolkit.md.
/// </summary>
public class MVVMToolkitDetector : IDetector
{
    public string Name => nameof(MVVMToolkitDetector);

    public string Description => "MVVM Toolkit";

    public string FrameworkId => "MVVMToolkit";

    public DetectorCategory Category => DetectorCategory.Library;

    public MVVMToolkitDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        // MVVM Toolkit
        return this.Create()
            .Required("New Version", checks => checks
                .ContainsModule("CommunityToolkit.MVVM.dll", checkForNgenModule: true).GetVersionFromModule())
            // OR
            .Required("Old Version", checks => checks
                .ContainsModule("Microsoft.Toolkit.MVVM.dll", checkForNgenModule: true).GetVersionFromModule())
            .BuildDefinition();
    }
}