// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Chromium Embedded Framework (CEF).
/// Built according to docs/Frameworks/CEF.md.
/// </summary>
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
                .ContainsModule("libcef.dll").GetVersionFromModule())
            .Optional("CefGlue (.NET)", checks => checks
                .ContainsModule("Xilium.CefGlue.dll")
                .ContainsModule("Xilium.CefGlue.Avalonia.dll")
                .ContainsModule("Xilium.CefGlue.WPF.dll"))
            .Optional("Window Classes", checks => checks
                .ContainsActiveWindow("CefBrowserWindow"))
            .BuildDefinition();
    }
}
