# React Native for Windows (RNW)

## Summary

React Native for Windows (RNW) enables React Native developers to create native Windows apps.

React Native is an open-source framework developed by Meta that enables you to build world-class application experiences on native platforms using a consistent developer experience based on JavaScript and React.

**Website:** [RNW Docs](https://microsoft.github.io/react-native-windows/)

### Languages

**Framework Languages:** C++, C#, XAML, JS and TS

**App Languages:** C++, C#, XAML, JS and TS

### OS Support

Windows 10 and 11

### Dependencies

React Native for Windows has two parallel implementations: the *Old Architecture* targeting UWP and the *New Architecture* targeting the Windows App SDK.

**Old Architecture:**

- [UWP XAML](https://learn.microsoft.com/en-us/windows/uwp/xaml-platform/)
- [WinUI 2](https://learn.microsoft.com/en-us/windows/uwp/get-started/winui2/)

**New Architecture:**

- [Windows App SDK Visual Layer](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/composition)

### Canonical Apps

**Old Architecture:**

- [React Native Gallery](https://apps.microsoft.com/detail/9npg0b292h4r)

**New Architecture:**

- [React Native Gallery - Preview](https://apps.microsoft.com/detail/9nsqt9wccmbd)

## How to Detect

**Implementation:** [RNWDetector](/src/FrameworkDetector/Detectors/Frameworks/RNWDetector.cs)

### Runtime Detection

Either of the following modules should be loaded by the running process:

1. `Microsoft.ReactNative.dll`
2. `react-native-win32.dll`

The specific version of RNW can be gotten by checking the FileVersion of either of these modules.

Other optional modules that may be loaded by the running process include:

1. `Microsoft.ReactNative.Managed.dll` used by *Old Architecture* C# apps
2. `Microsoft.ReactNative.Projection.dll"` used by *New Architecture* C# apps

### Static Detection

Either of the aforementioned required runtime modules should be present with the app's binaries. The specific version of RNW can be gotten by checking the FileVersion of any these modules.

## Resources

- [Official Docs](https://microsoft.github.io/react-native-windows/)
- [GitHub Source](https://github.com/microsoft/react-native-windows)
- [New vs. Old Architecture](https://microsoft.github.io/react-native-windows/docs/new-architecture)
- [Windows OS Compatibility](https://microsoft.github.io/react-native-windows/docs/win10-compat)
