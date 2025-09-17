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
            .Required("Microsoft.ReactNative", checks => checks
                .ContainsLoadedModule("Microsoft.ReactNative.dll", true))
            // OR
            .Required("React Native Win32", checks => checks
                .ContainsLoadedModule("react-native-win32.dll", true))
            .Optional(".NET Managed Helpers", checks => checks
                .ContainsLoadedModule("Microsoft.ReactNative.Managed.dll"))
            .Optional(".NET CsWinRT .Projection", checks => checks
                .ContainsLoadedModule("Microsoft.ReactNative.Projection.dll"))
            .BuildDefinition();
    }
}
