// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class UWPXAMLDetector : IDetector
{
    public string Name => nameof(UWPXAMLDetector);

    public string Description => "Universal Windows Platform XAML";

    public string FrameworkId => "UWPXAML";

    public DetectorCategory Category => DetectorCategory.Framework;

    public UWPXAMLDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Windows UI XAML", checks => checks
                .ContainsLoadedModule("windows.ui.xaml.dll"))
            .Optional("w/ CoreWindow", checks => checks
                .ContainsWindowClass("Windows.UI.Core.CoreWindow"))
            .Optional("w/ ApplicationFrameInputSinkWindow", checks => checks
                .ContainsWindowClass("ApplicationFrameInputSinkWindow"))
            .BuildDefinition();

        // TODO: Do we want an optional check for UWP for .NET here or as a separate detector? (Not sure if overlap or would be different... needs investigation)
    }
}
