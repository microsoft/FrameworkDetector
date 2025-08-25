// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class UWPXAMLDetector : IDetector
{
    public string Name => nameof(UWPXAMLDetector);

    public string Description => "Universal Windows Platform XAML";

    public string FrameworkId => "UWP";

    public UWPXAMLDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        // UWP
        return this.Create()
            .Required(checks => checks
                .ContainsModule("windows.ui.xaml.dll"))
            .Optional("w/ CoreWindow", checks => checks
                .ContainsClass("Windows.UI.Core.CoreWindow"))
            .Optional("w/ ApplicationFrameInputSinkWindow", checks => checks
                .ContainsClass("ApplicationFrameInputSinkWindow"))
            .BuildDefinition();

        // TODO: Do we want an optional check for UWP for .NET here or as a separate detector? (Not sure if overlap or would be different... needs investigation)
        // TODO: Do we want an optional check for WinUI 2 (MUXC) here or as a separate detector?
    }
}
