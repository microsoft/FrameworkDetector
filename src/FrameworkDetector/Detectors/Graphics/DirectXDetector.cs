// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Win2D library.
/// Built according to docs/Library/Win2D.md.
/// </summary>
public class DirectXDetector : IDetector
{
    public string Name => nameof(DirectXDetector);

    public string Description => "DirectX";

    public string FrameworkId => "DirectX";

    public DetectorCategory Category => DetectorCategory.Library;

    public DirectXDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        // DirectX
        return this.Create()
            // Note: Version is just host OS version?
            .Required("DirectX 12", checks => checks
                .ContainsLoadedModule("D3D12.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .BuildDefinition();
    }
}