// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific dependent package used by an input.
/// </summary>
public static class ContainsPackagedDependencyCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsPackagedDependencyCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData> GetCheckRegistrationInfo(ContainsPackagedDependencyArgs args)
    {
        return new(
            Name: nameof(ContainsPackagedDependencyCheck),
            Description: args.GetDescription(),
            DataSourceInterfaces: [typeof(IPackageDataSource)],
            PerformCheckAsync
            );
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsPackagedDependencyCheck"/>.
    /// </summary>
    /// <param name="packageFullName">The Package Full Name to look for as an immediate dependency (or in part).</param>
    public readonly struct ContainsPackagedDependencyArgs(string? packageFullName = null) : ICheckArgs
    {
        public string? PackageFullName { get; } = packageFullName;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find package dependency ");

            descriptionSB.AppendFormat(" has \"{0}\"", PackageFullName);

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(PackageFullName, nameof(ContainsPackagedDependencyArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsPackagedDependencyCheck"/>.
    /// </summary>
    /// <param name="packageFound">The package found.</param>
    public readonly struct ContainsPackagedDependencyData(PackageMetadata packageFound)
    {
        public PackageMetadata PackageFound { get; } = packageFound;
    }

    /// <summary>
    /// The type returned by <see cref="ContainsDependentPackage"/> which optionally allows calling <see cref="GetVersionFromPackageIdentity"/>.
    /// </summary>
    /// <param name="idcg">A base <see cref="IDetectorCheckGroup"/> to wrap.</param>
    public class ContainsPackagedDependencyDetectorCheckGroup(IDetectorCheckGroup idcg) : DetectorCheckGroupWrapper(idcg)
    {
        /// <summary>
        /// Gets the version from the <see cref="PackageIdentity.Version"/> property.
        /// </summary>
        /// <returns>Standard <see cref="IDetectorCheckGroup"/> to continue definitions.</returns>
        public IDetectorCheckGroup GetVersionFromPackageIdentity()
        {
            var dcg = this.Get();

            dcg.SetVersionGetter(r => GetPackageIdentityVersionFromCheckResult(r as DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>));

            return dcg;
        }

        /// <summary>
        /// Gets the version combined from the <see cref="PackageIdentity.FullName"/> and <see cref="PackageIdentity.Version"/> properties.
        /// </summary>
        /// <remarks>This is a more specialized function to deal with a few Microsoft packages that inlude part of the canonical version number of
        /// a library within the name of the package vs. as the root of the version number of the package. It will de-duplicate shared numbers and zeros
        /// to form a more readible version that better maps to what is acknowledged as the major.minor version of the library.</remarks>
        /// <returns>Standard <see cref="IDetectorCheckGroup"/> to continue definitions.</returns>
        public IDetectorCheckGroup GetVersionFromPackageFullName()
        {
            var dcg = this.Get();

            // Set hook for engine to retrieve version information from positive check results.
            dcg.SetVersionGetter(r => GetMicrosoftFullNameVersionFromCheckResult(r as DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>));

            return dcg;
        }
    }

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks if the Package's PackageFullName contains the provided text.
        /// </summary>
        /// <param name="packageFullName">All or part of the package full name to search for</param>
        /// <returns></returns>
        public ContainsPackagedDependencyDetectorCheckGroup ContainsPackagedDependency(string? packageFullName = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            var args = new ContainsPackagedDependencyArgs(packageFullName);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>(GetCheckRegistrationInfo(args), args));

            return new ContainsPackagedDependencyDetectorCheckGroup(dcg);
        }
    }

    /// <summary>
    /// Helper for extracting the version information from the Package identity. Used by <see cref="ContainsPackagedDependencyDetectorCheckGroup.GetVersionFromPackageIdentity(PackageVersionType)"/>.
    /// </summary>
    /// <param name="moduleVersionSource"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static string GetPackageIdentityVersionFromCheckResult(DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>? result)
    {
        if (result is not null && result.CheckStatus == DetectorCheckStatus.CompletedPassed)
        {
            return Version.TryParseCleaned(result.OutputData?.PackageFound.Id.Version, out var fileVer) && fileVer is not null ? fileVer.ToShortString() : string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Performs special extraction on packaged dependency information to retrieve the canonical version information of Windows App SDK (and similar packages)
    /// </summary>
    /// <param name="result">The <see cref="DetectorCheckResult{TInput, TOutput}"/> containing information on the discovered package.</param>
    /// <returns>The version string extracted.</returns>
    private static string GetMicrosoftFullNameVersionFromCheckResult(DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>? result)
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

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData> definition, IEnumerable<IInputType> inputs, DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData> result, CancellationToken cancellationToken)
    {
        result.CheckStatus = DetectorCheckStatus.InProgress;

        foreach (var input in inputs)
        {
            // Stop evaluating inputs if we've gotten a cancellation or a result
            if (cancellationToken.IsCancellationRequested || result.CheckStatus != DetectorCheckStatus.InProgress) break;

            if (input is IPackageDataSource dataSource)
            {
                await foreach (var package in dataSource.GetPackagesAsync(cancellationToken))
                {
                    // Stop evaluating packages if we've gotten a cancellation or a result
                    if (cancellationToken.IsCancellationRequested || result.CheckStatus != DetectorCheckStatus.InProgress) break;

                    foreach (var dependentPackage in package.Dependencies)
                    {
                        await Task.Yield();

                        if (cancellationToken.IsCancellationRequested) break;

                        var packageNameMatch = definition.CheckArguments.PackageFullName is null || dependentPackage.PackageDisplayName is null || dependentPackage.Id.FullName.Contains(definition.CheckArguments.PackageFullName, StringComparison.InvariantCultureIgnoreCase);

                        if (packageNameMatch)
                        {
                            result.OutputData = new ContainsPackagedDependencyData(dependentPackage);
                            result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                            break;
                        }
                    }
                }
            }
        }

        if (result.CheckStatus == DetectorCheckStatus.InProgress)
        {
            result.CheckStatus = cancellationToken.IsCancellationRequested ? DetectorCheckStatus.Canceled : DetectorCheckStatus.CompletedFailed;
        }
    }
}
