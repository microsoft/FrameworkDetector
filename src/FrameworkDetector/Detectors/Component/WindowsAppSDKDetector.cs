// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

using static FrameworkDetector.Checks.ContainsPackagedDependencyCheck;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for Windows App SDK (WinAppSDK).
/// Built according to TODO.
/// </summary>
public class WindowsAppSDKDetector : IDetector
{
    public string Name => nameof(WindowsAppSDKDetector);

    public string Description => "Windows App SDK";

    public string FrameworkId => "WinAppSDK";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WindowsAppSDKDetector()
    {
    }

    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            // Use Package Info first if found
            .Required("Dependent Package", checks => checks
                .ContainsPackagedDependency("Microsoft.WindowsAppRuntime").GetSpecialFullNameVersionFromPackageIdentity())
            // Otherwise look for key modules
            .Required("Resources Module", checks => checks
                .ContainsLoadedModule("Microsoft.Windows.ApplicationModel.Resources.dll"))
            .Required("Framework Package", checks => checks
                .ContainsLoadedModule("Microsoft.WindowsAppRuntime.Bootstrap.dll"))
            // TODO: There's a number of modules here that we could check for...
            .BuildDefinition();
    }
}

/// <summary>
/// Special extensions for custom Version information required for packaged 
/// </summary>
public static class WindowsPackagedExtensions
{
    // Extend the special group hook for Packaged Dependencies to insert our custom logic in the API chain.
    extension(ContainsPackagedDependencyDetectorCheckGroup cpddcg)
    {
        public IDetectorCheckGroup GetSpecialFullNameVersionFromPackageIdentity()
        {
            var dcg = cpddcg.Get();

            // Set hook for engine to retrieve version information from positive check results.
            dcg.SetVersionGetter(r => GetSpecialFullNameVersionFromCheckResult(r as DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>));

            return dcg;
        }
    }

    /// <summary>
    /// Performs special extraction on packaged dependency information to retrieve the canonical version information of Windows App SDK (and similar packages)
    /// </summary>
    /// <param name="result">The <see cref="DetectorCheckResult{TInput, TOutput}"/> containing information on the discovered package.</param>
    /// <returns>The version string extracted.</returns>
    public static string GetSpecialFullNameVersionFromCheckResult(DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>? result)
    {
        if (result is not null && result.CheckStatus == DetectorCheckStatus.CompletedPassed)
        {
            //// Note: Mostly for WinUI 2: Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe and Windows App SDK: Microsoft.WindowsAppRuntime.1.8_8000.642.119.0_x64__8wekyb3d8bbwe
            if (result.OutputData is not ContainsPackagedDependencyData output)
            {
                return string.Empty;
            }

            var pfn = output.PackageFound.Id.FullName;

            // Extract each piece to look for version-like sections
            var sections = pfn.Split([".", "_"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var numberSections = sections.Where(s => int.TryParse(s, out _)).ToArray();

            // Account for overlap between 2nd and 3rd sections (e.g. 2.8.8.2501.31001.0)
            if (numberSections.Length > 4
                && numberSections[2].StartsWith(numberSections[1]))
            {
                // TODO: Not sure if we should prioritize shorter number or longer number (e.g. '8000' vs '8' in '1.8_8000.642.119.0')
                numberSections = numberSections[..1].Concat(numberSections[2..]).ToArray();
            }

            return Version.TryParseCleaned(string.Join(".", numberSections), out var productVer) && productVer is not null ? productVer.ToShortString() : string.Empty;
        }

        return string.Empty;
    }
}
