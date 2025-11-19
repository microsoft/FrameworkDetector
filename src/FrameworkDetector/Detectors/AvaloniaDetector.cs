// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Avalonia UI (Avalonia).
/// Built according to docs/Frameworks/Avalonia.md.
/// </summary>
public class AvaloniaDetector : IDetector 
{
    public string Name => nameof(AvaloniaDetector);

    public string Description => "Avalonia UI";

    public string FrameworkId => "Avalonia";

    public DetectorCategory Category => DetectorCategory.Framework;

    public AvaloniaDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("Avalonia.Base.dll").GetVersionFromModule())
            .BuildDefinition();
    }
}
