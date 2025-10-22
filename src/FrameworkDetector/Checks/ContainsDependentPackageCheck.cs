// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

using static FrameworkDetector.Checks.ContainsDependentPackageCheck;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific dependent package used within a process.
/// </summary>
public static class ContainsDependentPackageCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsDependentPackageCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsDependentPackageArgs, ContainsDependentPackageData> GetCheckRegistrationInfo(ContainsDependentPackageArgs args)
    {
        return new(
            Name: nameof(ContainsDependentPackageCheck),
            Description: args.GetDescription(),
            DataSourceIds: [ProcessDataSource.Id],
            PerformCheckAsync
            );
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsDependentPackageCheck"/>.
    /// </summary>
    /// <param name="packageFullName">The Package Full Name to look for as an immediate dependency (or in part).</param>
    public readonly struct ContainsDependentPackageArgs(string? packageFullName = null) : ICheckArgs
    {
        public string? PackageFullName { get; } = packageFullName;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find package dependency ");

            descriptionSB.AppendFormat(" has \"{0}\"", PackageFullName);

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(PackageFullName, nameof(ContainsDependentPackageArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsDependentPackageCheck"/>.
    /// </summary>
    /// <param name="packageFound">The package found.</param>
    public readonly struct ContainsDependentPackageData(PackageMetadata packageFound)
    {
        public PackageMetadata PackageFound { get; } = packageFound;
    }

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks if the Package's PackageFullName contains the provided text.
        /// </summary>
        /// <param name="packageFullName">All or part of the package full name to search for</param>
        /// <returns></returns>
        public IDetectorCheckGroup ContainsDependentPackage(string? packageFullName = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            var args = new ContainsDependentPackageArgs(packageFullName);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsDependentPackageArgs, ContainsDependentPackageData>(GetCheckRegistrationInfo(args), args));

            return dcg;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsDependentPackageArgs, ContainsDependentPackageData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsDependentPackageArgs, ContainsDependentPackageData> result, CancellationToken cancellationToken)
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
                            result.OutputData = new ContainsDependentPackageData(package);
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
