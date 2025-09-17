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
                .ContainsLoadedModule("Microsoft.ReactNative.dll"))
            // OR
            .Required("Win32 Module", checks => checks
                .ContainsLoadedModule("react-native-win32.dll"))
            .Optional(".NET Helpers", checks => checks
                .ContainsLoadedModule("Microsoft.ReactNative.Managed.dll")
                .ContainsLoadedModule("Microsoft.ReactNative.Projection.dll"))
            .BuildDefinition();
    }
}
