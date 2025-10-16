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
using System.Text;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific loaded module, present within a process.
/// </summary>
public static class ContainsLoadedModuleCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsLoadedModuleArgs, ContainsLoadedModuleData> GetCheckRegistrationInfo(ContainsLoadedModuleArgs args)
    {
        return new(
            Name: nameof(ContainsLoadedModuleCheck),
            Description: args.GetDescription(),
            DataSourceIds: [ProcessDataSource.Id],
            PerformCheckAsync);
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="filename">A loaded module's filename must match this text, if specified.</param>
    /// <param name="originalFilename">A loaded module's original filename must match this text, if specified.</param>
    /// <param name="fileVersionRange">A loaded module's file version must match this semver version range sepc, if specified.</param>
    /// <param name="productName">A loaded module's product name must match this text, if specified.</param>
    /// <param name="productVersionRange">A loaded module's product version must match this semver version range sepc, if specified.</param>
    /// <param name="checkForNgenModule">Whether or not to also match NGENed versions (.ni.dll) of the specified filename and/or original filename.</param>
    public readonly struct ContainsLoadedModuleArgs(string? filename = null, string? originalFilename = null, string? fileVersionRange = null, string? productName = null, string? productVersionRange = null, bool? checkForNgenModule = null) : ICheckArgs
    {
        public string? Filename { get; } = filename;

        public string? OriginalFilename { get; } = originalFilename;

        public string? FileVersionRange { get; } = fileVersionRange;

        public string? ProductName { get; } = productName;

        public string? ProductVersionRange { get; } = productVersionRange;

        public bool? CheckForNgenModule { get; } = checkForNgenModule;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find module ");

            bool nameAdded = false;
            if (Filename is not null)
            {
                descriptionSB.AppendFormat("\"{0}\"", Filename);
                nameAdded = true;
            }

            if (OriginalFilename is not null)
            {
                if (nameAdded)
                {
                    descriptionSB.Append(", ");
                }
                descriptionSB.AppendFormat("originally \"{0}\"", OriginalFilename);
                nameAdded = true;
            }

            if (ProductName is not null)
            {
                if (nameAdded)
                {
                    descriptionSB.Append(", ");
                }
                descriptionSB.AppendFormat("product \"{0}\"", ProductName);
                nameAdded = true;
            }

            if (FileVersionRange is not null || ProductVersionRange is not null)
            {
                if (nameAdded)
                {
                    descriptionSB.Append(' ');
                }

                descriptionSB.Append("with ");

                bool versionAdded = false;
                if (FileVersionRange is not null)
                {
                    descriptionSB.AppendFormat("version \"{0}\"", FileVersionRange);
                    versionAdded = true;
                }

                if (ProductVersionRange is not null)
                {
                    if (versionAdded)
                    {
                        descriptionSB.Append(", ");
                    }
                    descriptionSB.AppendFormat("product version \"{0}\"", ProductVersionRange);
                }
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(Filename ?? OriginalFilename ?? ProductName, nameof(ContainsLoadedModuleArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="moduleFound">The module found.</param>
    public readonly struct ContainsLoadedModuleData(WindowsBinaryMetadata moduleFound)
    {
        public WindowsBinaryMetadata ModuleFound { get; } = moduleFound;
    }

    public interface IContainsLoadedModuleDetectorCheckGroup : IDetectorCheckGroup { };

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for module by name in Process.LoadedModules.
        /// </summary>
        /// <param name="filename">A loaded module's filename must match this text, if specified.</param>
        /// <param name="originalFilename">A loaded module's original filename must match this text, if specified.</param>
        /// <param name="fileVersionRange">A loaded module's file version must match this semver version range sepc, if specified.</param>
        /// <param name="productName">A loaded module's product name must match this text, if specified.</param>
        /// <param name="productVersionRange">A loaded module's product version must match this semver version range sepc, if specified.</param>
        /// <param name="checkForNgenModule">Whether or not to also match NGENed versions (.ni.dll) of the specified filename and/or original filename.</param>
        /// <returns></returns>
        public IContainsLoadedModuleDetectorCheckGroup ContainsLoadedModule(string? filename = null, string? originalFilename = null, string? fileVersionRange = null, string? productName = null, string? productVersionRange = null, bool? checkForNgenModule = null)
        {
            if (@this is not DetectorCheckGroup dcg || @this is not IContainsLoadedModuleDetectorCheckGroup retValue)
            {
                throw new InvalidOperationException();
            }

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.

            var args = new ContainsLoadedModuleArgs(filename, originalFilename, fileVersionRange, productName, productVersionRange, checkForNgenModule);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(GetCheckRegistrationInfo(args), args));

            return retValue;
        }
    }

    public enum ModuleVersionType
    {
        FileVersion = 0,
        ProductVersion,
    }

    extension(IContainsLoadedModuleDetectorCheckGroup @this)
    {
        public IDetectorCheckGroup GetVersionFromModule(ModuleVersionType moduleVersionSource = ModuleVersionType.FileVersion)
        {
            if (@this is not DetectorCheckGroup dcg)
            {
                throw new InvalidOperationException();
            }

            dcg.SetVersionGetter(r => GetVersionFromCheckResult(moduleVersionSource, r as DetectorCheckResult<ContainsLoadedModuleArgs, ContainsLoadedModuleData>));

            return dcg;
        }
    }

    public static string GetVersionFromCheckResult(ModuleVersionType moduleVersionSource, DetectorCheckResult<ContainsLoadedModuleArgs, ContainsLoadedModuleData>? result)
    {
        if (result is not null && result.CheckStatus == DetectorCheckStatus.CompletedPassed)
        {
            switch(moduleVersionSource)
            {
                case ModuleVersionType.FileVersion:
                    return Version.TryParseCleaned(result.OutputData?.ModuleFound.FileVersion, out var fileVer) && fileVer is not null ? fileVer.ToShortString() : string.Empty;
                case ModuleVersionType.ProductVersion:
                    return Version.TryParseCleaned(result.OutputData?.ModuleFound.ProductVersion, out var productVer) && productVer is not null ? productVer.ToShortString() : string.Empty;
            }
        }

        return string.Empty;
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsLoadedModuleArgs, ContainsLoadedModuleData> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out IProcessDataSource[] processes))
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

                        var filenameMatch = definition.CheckArguments.Filename is null || string.Equals(definition.CheckArguments.Filename, module.Filename, StringComparison.InvariantCultureIgnoreCase) || (checkForNgenModule && string.Equals(nGenModuleName, module.Filename, StringComparison.InvariantCultureIgnoreCase));
                        var fileVersionMatch = fileVersionRange is null || SemVersion.TryParseCleaned(module.FileVersion, out var fileVersion) && fileVersionRange.Contains(fileVersion);

                        var originalFilenameMatch = definition.CheckArguments.OriginalFilename is null || string.Equals(definition.CheckArguments.OriginalFilename, module.OriginalFilename, StringComparison.InvariantCultureIgnoreCase) || (checkForNgenModule && string.Equals(nGenOriginalModuleName, module.OriginalFilename, StringComparison.InvariantCultureIgnoreCase));

                        var productNameMatch = definition.CheckArguments.ProductName is null || string.Equals(definition.CheckArguments.ProductName, module.ProductName, StringComparison.InvariantCultureIgnoreCase);
                        var productVersionMatch = productVersionRange is null || SemVersion.TryParseCleaned(module.ProductVersion, out var productVersion) && productVersionRange.Contains(productVersion);

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
