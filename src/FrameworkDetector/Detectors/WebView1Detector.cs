// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft EdgeHTML (WebView1).
/// Built according to docs/Frameworks/WebView1.md.
/// </summary>
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
        return this.Create()
            .Required("", checks => checks
                .ContainsModule("edgehtml.dll").GetVersionFromModule())
            // OR
            .Optional("Windows", checks => checks
                .ContainsActiveWindow("XAMLWebViewHostWindowClass"))
            .BuildDefinition();
    }
}
