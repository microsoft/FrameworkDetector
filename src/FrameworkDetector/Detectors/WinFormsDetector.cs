// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Windows Forms (WinForms).
/// Built according to docs/Frameworks/WinForms.md.
/// </summary>
public class WinFormsDetector : IDetector 
{
    public string Name => nameof(WinFormsDetector);

    public string Description => "Windows Forms";

    public string FrameworkId => "WinForms";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinFormsDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsModule("System.Windows.Forms.dll", checkForNgenModule: true).GetVersionFromModule())
            .BuildDefinition();
    }
}
