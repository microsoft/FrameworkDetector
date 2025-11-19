// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Windows Community Toolkit (WCT).
/// Built according to docs/Frameworks/WindowsCommunityToolkit.md.
/// </summary>
public class WCTDetector : IDetector
{
    public string Name => nameof(WCTDetector);

    public string Description => "Windows Community Toolkit";

    public string FrameworkId => "WCT";

    public DetectorCategory Category => DetectorCategory.Library;

    public WCTDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        // WCT Toolkit
        return this.Create()
            // Note: I don't think this works for .NET Native UWP apps... (or because we build our gallery from source there's no modules?)
            .Required("New Version (WinUI) Animations", checks => checks
                .ContainsLoadedModule("CommunityToolkit.WinUI.Animations.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("New Version (UWP) Animations", checks => checks
                .ContainsLoadedModule("CommunityToolkit.UWP.Animations.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("New Version (WinUI) Primitives", checks => checks
                .ContainsLoadedModule("CommunityToolkit.WinUI.Controls.Primitives.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("New Version (UWP) Primitives", checks => checks
                .ContainsLoadedModule("CommunityToolkit.UWP.Controls.Primitives.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            // OR old versions
            .Required("Old Version (WinUI) Animations", checks => checks
                .ContainsLoadedModule("Microsoft.Toolkit.WinUI.UI.Animations.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("Old Version (UWP) Animations", checks => checks
                .ContainsLoadedModule("Microsoft.Toolkit.Uwp.UI.Animations.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("Old Version (WinUI) Primitives", checks => checks
                .ContainsLoadedModule("Microsoft.Toolkit.WinUI.UI.Controls.Primitives.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .Required("Old Version (UWP) Primitives", checks => checks
                .ContainsLoadedModule("Microsoft.Toolkit.Uwp.UI.Controls.Primitives.dll", checkForNgenModule: true).GetVersionFromModule(ModuleVersionType.ProductVersion))
            .BuildDefinition();
    }
}