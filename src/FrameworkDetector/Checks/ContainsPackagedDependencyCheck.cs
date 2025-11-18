// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific dependent package used within a process.
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
            DataSourceIds: [ProcessDataSource.Id],
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
        public IDetectorCheckGroup GetVersionFromPackageIdentity(PackageVersionType packageVersionSource = PackageVersionType.Version)
        {
            var dcg = this.Get();

            dcg.SetVersionGetter(r => GetVersionFromCheckResult(packageVersionSource, r as DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>));

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
    public static string GetVersionFromCheckResult(PackageVersionType moduleVersionSource, DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData>? result)
    {
        if (result is not null && result.CheckStatus == DetectorCheckStatus.CompletedPassed)
        {
            switch (moduleVersionSource)
            {
                case PackageVersionType.Version:
                    return Version.TryParseCleaned(result.OutputData?.PackageFound.Id.Version, out var fileVer) && fileVer is not null ? fileVer.ToShortString() : string.Empty;

                //// Note: Mostly for WinUI 2: Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe and Windows App SDK: Microsoft.WindowsAppRuntime.1.8_8000.642.119.0_x64__8wekyb3d8bbwe
                case PackageVersionType.FullNameSpecial:
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
        }

        return string.Empty;
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsPackagedDependencyArgs, ContainsPackagedDependencyData> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out IProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            foreach (var process in processes)
            {
                var packageMetadata = process.ProcessMetadata?.AppPackageMetadata;
                if (packageMetadata is not null && packageMetadata.PackageMetadata is not null)
                {
                    var dependentPackages = packageMetadata.PackageMetadata.Dependencies;
                    foreach (var package in dependentPackages)
                    {
                        await Task.Yield();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            result.CheckStatus = DetectorCheckStatus.Canceled;
                            break;
                        }

                        var packageNameMatch = definition.CheckArguments.PackageFullName is null || package.PackageDisplayName is null || package.Id.FullName.Contains(definition.CheckArguments.PackageFullName, StringComparison.InvariantCultureIgnoreCase);

                        if (packageNameMatch)
                        {
                            result.OutputData = new ContainsPackagedDependencyData(package);
                            result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                            break;
                        }
                    }
                }

                // Stop evaluating other process data sources if we've gotten a pass or cancel
                if (result.CheckStatus != DetectorCheckStatus.InProgress)
                {
                    break;
                }
            }

            if (result.CheckStatus == DetectorCheckStatus.InProgress)
            {
                result.CheckStatus = DetectorCheckStatus.CompletedFailed;
            }
        }
        else
        {
            // No CheckInput = Error
            result.CheckStatus = DetectorCheckStatus.Error;
        }
    }
}

public enum PackageVersionType
{
    /// <summary>
    /// Grabs the version from the <see cref="PackageIdentity.Version"/> property.
    /// </summary>
    Version = 0,

    //// Note: Mostly for WinUI 2: Microsoft.UI.Xaml.2.8_8.2501.31001.0_x64__8wekyb3d8bbwe and Windows App SDK: Microsoft.WindowsAppRuntime.1.8_8000.642.119.0_x64__8wekyb3d8bbwe
    /// <summary>
    /// Grabs the version from the <see cref="PackageIdentity.FullName"/> property with anything version looking around it... (non-standard)
    /// </summary>
    FullNameSpecial,
}