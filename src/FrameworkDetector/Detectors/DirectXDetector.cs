// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Microsoft DirectX library.
/// Built according to docs/Frameworks/DirectX.md.
/// </summary>
public class DirectXDetector : IDetector
{
    // TODO: Should this be just scoped to Direct3D only? i.e. we should probably have separate detectors for each DirectX component (e.g. Direct2D, DirectWrite, etc...) and tie to parent detector? https://en.wikipedia.org/wiki/DirectX
    public string Name => nameof(DirectXDetector);

    public string Description => "Microsoft DirectX";

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
                .ContainsModule("D3D12.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("DirectX 11", checks => checks
                .ContainsModule("D3D11.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("DirectX 10", checks => checks
                .ContainsModule("D3D10.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("DirectX 9", checks => checks
                .ContainsModule("D3D9.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .BuildDefinition();
    }
}