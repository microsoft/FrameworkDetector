// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Semver;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific loaded module, present within a process.
/// </summary>
public static class ContainsLoadedModuleCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    private static CheckRegistrationInfo<ContainsLoadedModuleArgs, ContainsLoadedModuleData> GetCheckRegistrationInfo(ContainsLoadedModuleArgs args)
    {
        return new(
            Name: nameof(ContainsLoadedModuleCheck),
            Description: $"Find module {args.Filename ?? args.OriginalFilename ?? args.ProductName}{(args.FileVersionRange is not null || args.ProductVersionRange is not null ? $" {args.FileVersionRange ?? args.ProductVersionRange}" : "")}",
            DataSourceIds: [ProcessDataSource.Id],
            PerformCheckAsync);
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="filename">The filename of the module to look for.</param>
    /// <param name="originalFilename">The original filename of the module to look for.</param>
    /// <param name="fileVersionRange">Semver version range sepc to match a specific module file version.</param>
    /// <param name="productName">The product name of the module to look for.</param>
    /// <param name="productVersionRange">Semver version range sepc to match a specific module product version.</param>
    /// <param name="checkForNgenModule">Whether or not to look for an NGENed version of the module.</param>
    public readonly struct ContainsLoadedModuleArgs(string? filename, string? originalFilename, string? fileVersionRange, string? productName, string? productVersionRange, bool? checkForNgenModule)
    {
        public string? Filename { get; } = filename;

        public string? OriginalFilename { get; } = originalFilename;

        public string? FileVersionRange { get; } = fileVersionRange;

        public string? ProductName { get; } = productName;

        public string? ProductVersionRange { get; } = productVersionRange;

        public bool? CheckForNgenModule { get; } = checkForNgenModule;
    }

    /// <summary>
    /// Output data for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="moduleFound">The module found.</param>
    public readonly struct ContainsLoadedModuleData(WindowsBinaryMetadata moduleFound)
    {
        public WindowsBinaryMetadata ModuleFound { get; } = moduleFound;
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for module by name in Process.LoadedModules.
        /// </summary>
        /// <param name="filename">The filename of the module to look for.</param>
        /// <param name="originalFilename">The original filename of the module to look for.</param>
        /// <param name="fileVersionRange">Semver version range sepc to match a specific module file version.</param>
        /// <param name="productName">The product name of the module to look for.</param>
        /// <param name="productVersionRange">Semver version range sepc to match a specific module product version.</param>
        /// <param name="checkForNgenModule">Whether or not to look for an NGENed version of the module.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsLoadedModule(string? filename = null, string? originalFilename = null, string? fileVersionRange = null, string? productName = null, string? productVersionRange = null, bool? checkForNgenModule = null)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.

            // TODO: Maybe make args and then have it run its own validator?
            if (filename is null && originalFilename is null && productName is null)
            {
                throw new ArgumentNullException($"{nameof(ContainsLoadedModule)} requires at least one name to not be null.");
            }

            var args = new ContainsLoadedModuleArgs(filename, originalFilename, fileVersionRange, productName, productVersionRange, checkForNgenModule);
            @this.AddCheck(new CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(GetCheckRegistrationInfo(args), args));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsLoadedModuleArgs, ContainsLoadedModuleData> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            string? nGenModuleName = null;
            string? nGenOriginalModuleName = null;
            bool checkForNgenModule = definition.CheckArguments.CheckForNgenModule ?? false;
            if (checkForNgenModule)
            {
                nGenModuleName = Path.ChangeExtension(definition.CheckArguments.Filename, ".ni" + Path.GetExtension(definition.CheckArguments.Filename));
                nGenOriginalModuleName = Path.ChangeExtension(definition.CheckArguments.OriginalFilename, ".ni" + Path.GetExtension(definition.CheckArguments.OriginalFilename));
            }

            var fileVersionRange = definition.CheckArguments.FileVersionRange is not null ? SemVersionRange.Parse(definition.CheckArguments.FileVersionRange, SemVersionRangeOptions.OptionalPatch) : null;
            var productVersionRange = definition.CheckArguments.ProductVersionRange is not null ? SemVersionRange.Parse(definition.CheckArguments.ProductVersionRange, SemVersionRangeOptions.OptionalPatch) : null;

            foreach (var process in processes)
            {
                var loadedModules = process.ProcessMetadata?.LoadedModules;
                if (loadedModules is not null)
                {
                    foreach (var module in loadedModules)
                    {
                        await Task.Yield();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            result.CheckStatus = DetectorCheckStatus.Canceled;
                            break;
                        }

                        var filenameMatch = definition.CheckArguments.Filename is null || string.Equals(definition.CheckArguments.Filename, module.Filename, StringComparison.InvariantCultureIgnoreCase) || (checkForNgenModule && string.Equals(definition.CheckArguments.Filename, nGenModuleName, StringComparison.InvariantCultureIgnoreCase));
                        var fileVersionMatch = fileVersionRange is null || SemVersion.TryLooseParse(module.FileVersion, out var fileVersion) && fileVersionRange.Contains(fileVersion);

                        var originalFilenameMatch = definition.CheckArguments.OriginalFilename is null || string.Equals(definition.CheckArguments.OriginalFilename, module.OriginalFilename, StringComparison.InvariantCultureIgnoreCase) || (checkForNgenModule && string.Equals(definition.CheckArguments.OriginalFilename, nGenOriginalModuleName, StringComparison.InvariantCultureIgnoreCase));

                        var productNameMatch = definition.CheckArguments.ProductName is null || string.Equals(definition.CheckArguments.ProductName, module.ProductName, StringComparison.InvariantCultureIgnoreCase);
                        var productVersionMatch = productVersionRange is null || SemVersion.TryLooseParse(module.ProductVersion, out var productVersion) && productVersionRange.Contains(productVersion);

                        if (filenameMatch && fileVersionMatch && originalFilenameMatch && productNameMatch && productVersionMatch)
                        {
                            result.OutputData = new ContainsLoadedModuleData(module);
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
