// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Win2D library.
/// Built according to docs/Frameworks/Win2D.md.
/// </summary>
public class Win2DDetector : IDetector
{
    public string Name => nameof(Win2DDetector);

    public string Description => "Win2D";

    public string FrameworkId => "Win2D";

    public DetectorCategory Category => DetectorCategory.Library;

    public Win2DDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        // Win2D
        return this.Create()
            // Note: Like a WebView this can be delay loaded, so app may need to be exercised to detect it.
            .Required("", checks => checks
                .ContainsModule("Microsoft.Graphics.Canvas.dll", checkForNgenModule: true).GetVersionFromModule())
            .BuildDefinition();
    }
}