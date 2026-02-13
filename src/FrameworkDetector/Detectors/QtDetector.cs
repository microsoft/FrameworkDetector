// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Qt Framework (Qt).
/// Built according to docs/Frameworks/Qt.md.
/// </summary>
public class QtDetector : IDetector 
{
    public string Name => nameof(QtDetector);

    public string Description => "Qt Framework";

    public string FrameworkId => "Qt";

    public DetectorCategory Category => DetectorCategory.Framework;

    public QtDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Qt4", checks => checks
                .ContainsModule("Qt4Core.dll").GetVersionFromModule())
            // OR
            .Required("Qt5", checks => checks
                .ContainsModule("Qt5Core.dll").GetVersionFromModule())
            // OR
            .Required("Qt6", checks => checks
                .ContainsModule("Qt6Core.dll").GetVersionFromModule())
            // OR
            .Required("Qt5*QWindow Window Class", checks => checks
                .ContainsActiveWindow("Qt5")
                .ContainsActiveWindow("QWindowIcon"))
            // OR
            .Required("Qt6*QWindow Window Class", checks => checks
                .ContainsActiveWindow("Qt6")
                .ContainsActiveWindow("QWindowIcon"))
            .Optional("Gui Module", checks => checks
                .ContainsModule("Qt4Gui.dll").GetVersionFromModule()
                .ContainsModule("Qt5Gui.dll").GetVersionFromModule()
                .ContainsModule("Qt6Gui.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
