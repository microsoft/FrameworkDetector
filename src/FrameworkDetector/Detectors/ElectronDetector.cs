// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Electron (Electron).
/// Built according to docs/Frameworks/Electron.md.
/// </summary>
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
            .Required("Function Exports", checks => checks
                .ContainsExportedFunction("@electron@@"))
            // OR
            .Required("Main Window Class", checks => checks
                .ContainsActiveWindow("Electron_SystemPreferencesHostWindow"))
            .Optional("Other Window Classes", checks => checks
                .ContainsActiveWindow("Electron_PowerMonitorHostWindow")
                .ContainsActiveWindow("Electron_NotifyIconHostWindow"))
            .Optional("Renamed electron.exe", checks => checks
                .ContainsModule(originalFilename: "electron.exe"))
            .BuildDefinition();
    }
}
