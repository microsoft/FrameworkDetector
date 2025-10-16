// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Windows App SDK.
/// Built according to TODO.
/// </summary>
public class WindowsAppSDKDetector : IDetector
{
    public string Name => nameof(WindowsAppSDKDetector);

    public string Description => "Windows App SDK";

    public string FrameworkId => "WASDK";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WindowsAppSDKDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("", checks => checks
                .ContainsLoadedModule("Microsoft.Windows.ApplicationModel.Resources.dll"))
            .Required("Framework Package", checks => checks
                // TODO: There's a number of modules here that we need to check for.
                .ContainsLoadedModule("Microsoft.WindowsAppRuntime.Bootstrap.dll"))
            .BuildDefinition();
    }
}
