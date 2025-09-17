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
            .Required("WebView2Loader", checks => checks
                .ContainsLoadedModule("WebView2Loader.dll"))
            // OR
            .Required("WebView2.Core", checks => checks
                .ContainsLoadedModule("Microsoft.Web.WebView2.Core.dll"))
            .Optional(".NET Managed WPF", checks => checks
                .ContainsLoadedModule("Microsoft.Web.WebView2.WPF.dll"))
            .Optional(".NET Managed Winforms", checks => checks
                .ContainsLoadedModule("Microsoft.Web.WebView2.Winforms.dll"))
            .BuildDefinition();
    }
}
