// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class ElectronDetector : IDetector 
{
    public string Name => nameof(ElectronDetector);

    public string Description => "Electron";

    public string FrameworkId => "Electron";

    public DetectorCategory Category => DetectorCategory.Framework;

    public ElectronDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Main Window Class", checks => checks
                .ContainsWindowClass("Electron_SystemPreferencesHostWindow"))
            .Optional("Other Window Classes", checks => checks
                .ContainsWindowClass("Electron_PowerMonitorHostWindow")
                .ContainsWindowClass("Electron_NotifyIconHostWindow"))
            .BuildDefinition();
    }
}
