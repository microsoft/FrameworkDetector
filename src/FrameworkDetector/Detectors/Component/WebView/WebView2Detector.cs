// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

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
        // https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution?tabs=dotnetcsharp#files-to-ship-with-the-app
        return this.Create()
            .Required("Loader Module", checks => checks
                .ContainsLoadedModule("WebView2Loader.dll"))
            // OR
            .Required("Core Module", checks => checks
                .ContainsLoadedModule("Microsoft.Web.WebView2.Core.dll"))
            .Optional("Extra Modules", checks => checks
                .ContainsLoadedModule("Microsoft.Web.WebView2.WPF.dll")
                .ContainsLoadedModule("Microsoft.Web.WebView2.Winforms.dll"))
            .BuildDefinition();
    }
}
