// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Microsoft Edge WebView2 (WebView2).
/// Built according to docs/Frameworks/WebView2.md.
/// </summary>
public class WebView2Detector : IDetector 
{
    public string Name => nameof(WebView2Detector);

    public string Description => "Microsoft Edge WebView2";

    public string FrameworkId => "WebView2";

    public DetectorCategory Category => DetectorCategory.Component;

    public WebView2Detector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Loader Module", checks => checks
                .ContainsModule("WebView2Loader.dll").GetVersionFromModule())
            // OR
            .Required("Core Module", checks => checks
                .ContainsModule("Microsoft.Web.WebView2.Core.dll").GetVersionFromModule())
            // OR
            .Required("CsWinRT Projection Module", checks => checks
                .ContainsModule("Microsoft.Web.WebView2.Core.Projection.dll").GetVersionFromModule())
            // OR
            .Required("Embedded Browser WebView", checks => checks
                .ContainsModule("EmbeddedBrowserWebView.dll"))
            .Optional("Extra Modules", checks => checks
                .ContainsModule("EmbeddedBrowserWebView.dll")
                .ContainsModule("Microsoft.Web.WebView2.WPF.dll")
                .ContainsModule("Microsoft.Web.WebView2.Winforms.dll"))
            .BuildDefinition();
    }
}
