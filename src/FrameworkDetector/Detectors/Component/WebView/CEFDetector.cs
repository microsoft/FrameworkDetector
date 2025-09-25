// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class CEFDetector : IDetector 
{
    public string Name => nameof(CEFDetector);

    public string Description => "Chromium Embedded Framework";

    public string FrameworkId => "CEF";

    public DetectorCategory Category => DetectorCategory.Component;

    public CEFDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("libcef.dll"))
            .Optional("CefGlue (.NET)", checks => checks
                .ContainsLoadedModule("Xilium.CefGlue.dll")
                .ContainsLoadedModule("Xilium.CefGlue.Avalonia.dll")
                .ContainsLoadedModule("Xilium.CefGlue.WPF.dll"))
            .Optional("Window Classes", checks => checks
                .ContainsActiveWindow("CefBrowserWindow"))
            .BuildDefinition();
    }
}
