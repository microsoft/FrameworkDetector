// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class WebView1Detector : IDetector 
{
    public string Name => nameof(WebView1Detector);

    public string Description => "Microsoft EdgeHTML";

    public string FrameworkId => "WebView1";

    public DetectorCategory Category => DetectorCategory.Component;

    public WebView1Detector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        // https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution?tabs=dotnetcsharp#files-to-ship-with-the-app
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("edgehtml.dll"))
            // OR
            .Optional("Windows", checks => checks
                .ContainsActiveWindow("XAMLWebViewHostWindowClass"))
            .BuildDefinition();
    }
}
