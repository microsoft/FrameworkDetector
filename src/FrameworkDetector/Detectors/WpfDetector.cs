// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Detectors;

public class WpfDetector : Detector 
{
    public override string Name => nameof(WpfDetector);

    public override string Description => "Windows Presentation Framework";

    public override string FrameworkId => "WPF";

    public WpfDetector()
    {
        _moduleNames.Add("PresentationFramework.dll");
        _moduleNames.Add("PresentationCore.dll");
    }
}
