// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Windows UI Library for UWP (WinUI).
/// Built according to docs/Frameworks/WinUI.md.
/// </summary>
public class WinUIDetector : IDetector
{
    public string Name => nameof(WinUIDetector);

    public string Description => "Windows UI Library for UWP";

    public string FrameworkId => "WinUI";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WinUIDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("Windows.UI.Xaml.dll"))
            .Optional("Windows", checks => checks
                .ContainsActiveWindow("Windows.UI.Core.CoreWindow")
                .ContainsActiveWindow("ApplicationFrameInputSinkWindow"))
            .BuildDefinition();

        // TODO: Do we want an optional check for UWP for .NET here or as a separate detector? (Not sure if overlap or would be different... needs investigation)
    }
}
