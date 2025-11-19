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
        // TODO: Need to look at older versions and if this is the most common dll in all apps... (e.g. there's also 'Qt5Gui.dll')
        return this.Create()
            .Required("Qt5", checks => checks
                .ContainsLoadedModule("Qt5Core.dll").GetVersionFromModule())
            .Optional("Gui", checks => checks
                .ContainsLoadedModule("Qt5Gui.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
