// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the React Native for Windows (RNW).
/// Built according to docs/Frameworks/RNW.md.
/// </summary>
public class RNWDetector : IDetector 
{
    public string Name => nameof(RNWDetector);

    public string Description => "React Native for Windows";

    public string FrameworkId => "RNW";

    public DetectorCategory Category => DetectorCategory.Framework;

    public RNWDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("Main Module", checks => checks
                .ContainsModule("Microsoft.ReactNative.dll").GetVersionFromModule())
            // OR
            .Required("Win32 Module", checks => checks
                .ContainsModule("react-native-win32.dll").GetVersionFromModule())
            .Optional(".NET Helpers", checks => checks
                .ContainsModule("Microsoft.ReactNative.Managed.dll")
                .ContainsModule("Microsoft.ReactNative.Projection.dll"))
            .Optional("Community Modules", checks => checks
                .ContainsModule("LottieReactNative.dll")
                .ContainsModule("ReactNativeWebView.dll")
                .ContainsModule("ReactNativeXaml.dll")
                .ContainsModule("RNSVG.dll"))
            .BuildDefinition();
    }
}
