// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Semver;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using FrameworkDetector.Inputs;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific module, present within a input.
/// </summary>
public static class ContainsModuleCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsModuleCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsModuleArgs, ContainsModuleData> GetCheckRegistrationInfo(ContainsModuleArgs args)
    {
        return new(
            Name: nameof(ContainsModuleCheck),
            Description: args.GetDescription(),
            DataSourceInterfaces: [typeof(IModulesDataSource)],
            PerformCheckAsync);
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsModuleCheck"/>.
    /// </summary>
    /// <param name="filename">A module's filename must match this text, if specified.</param>
    /// <param name="originalFilename">A module's original filename must match this text, if specified.</param>
    /// <param name="fileVersionRange">A module's file version must match this semver version range sepc, if specified.</param>
    /// <param name="productName">A module's product name must match this text, if specified.</param>
    /// <param name="productVersionRange">A module's product version must match this semver version range sepc, if specified.</param>
    /// <param name="isLoaded">Whether or not the module is loaded in memory, if specified.</param>
    /// <param name="checkForNgenModule">Whether or not to also match NGENed versions (.ni.dll) of the specified filename and/or original filename.</param>
    public readonly struct ContainsModuleArgs(string? filename = null, string? originalFilename = null, string? fileVersionRange = null, string? productName = null, string? productVersionRange = null, bool? isLoaded = null, bool? checkForNgenModule = null) : ICheckArgs
    {
        public string? Filename { get; } = filename;

        public string? OriginalFilename { get; } = originalFilename;

        public string? FileVersionRange { get; } = fileVersionRange;

        public string? ProductName { get; } = productName;

        public string? ProductVersionRange { get; } = productVersionRange;

        public bool? IsLoaded { get; } = isLoaded;

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

            bool versionAdded = false;
            if (FileVersionRange is not null || ProductVersionRange is not null)
            {
                if (nameAdded)
                {
                    descriptionSB.Append(' ');
                }

                descriptionSB.Append("with ");

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

            if (IsLoaded is not null)
            {
                if (nameAdded || versionAdded)
                {
                    descriptionSB.Append(' ');
                }
                descriptionSB.AppendFormat("where it's \"{0}\"", IsLoaded.Value ? "loaded" : "not loaded");
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(Filename ?? OriginalFilename ?? ProductName, nameof(ContainsModuleArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsModuleCheck"/>.
    /// </summary>
    /// <param name="moduleFound">The module found.</param>
    public readonly struct ContainsModuleData(WindowsModuleMetadata moduleFound)
    {
        public WindowsModuleMetadata ModuleFound { get; } = moduleFound;
    }

    /// <summary>
    /// The type returned by <see cref="ContainsLoadedModule"/> which optionally allows calling <see cref="GetVersionFromModule"/>.
    /// </summary>
    /// <param name="idcg">A base <see cref="IDetectorCheckGroup"/> to wrap.</param>
    public class ContainsModuleDetectorCheckGroup(IDetectorCheckGroup idcg) : DetectorCheckGroupWrapper(idcg)
    {
        public IDetectorCheckGroup GetVersionFromModule(ModuleVersionType moduleVersionSource = ModuleVersionType.FileVersion)
        {
            var dcg = Get();

            dcg.SetVersionGetter(r => GetVersionFromCheckResult(moduleVersionSource, r as DetectorCheckResult<ContainsModuleArgs, ContainsModuleData>));

            return dcg;
        }
    }

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for module by name found either in Process.LoadedModules or by ImportedFunctions, use the isLoaded parameter to enforce checking that the module was in use (e.g. by a process).
        /// </summary>
        /// <param name="filename">A loaded module's filename must match this text, if specified.</param>
        /// <param name="originalFilename">A loaded module's original filename must match this text, if specified.</param>
        /// <param name="fileVersionRange">A loaded module's file version must match this semver version range sepc, if specified.</param>
        /// <param name="productName">A loaded module's product name must match this text, if specified.</param>
        /// <param name="productVersionRange">A loaded module's product version must match this semver version range sepc, if specified.</param>
        /// <param name="isLoaded"></param>Whether to explicitly check if the module was discovered in use or not. By default null where the module will match regardless of if it was in use or not.</param>
        /// <param name="checkForNgenModule">Whether or not to also match NGENed versions (.ni.dll) of the specified filename and/or original filename.</param>
        /// <returns><see cref="ContainsModuleDetectorCheckGroup"/> wrapper to allow for version specification point.</returns>
        public ContainsModuleDetectorCheckGroup ContainsModule(string? filename = null, string? originalFilename = null, string? fileVersionRange = null, string? productName = null, string? productVersionRange = null, bool? isLoaded = null, bool? checkForNgenModule = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.

            var args = new ContainsModuleArgs(filename, originalFilename, fileVersionRange, productName, productVersionRange, checkForNgenModule, isLoaded);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsModuleArgs, ContainsModuleData>(GetCheckRegistrationInfo(args), args));

            return new ContainsModuleDetectorCheckGroup(dcg);
        }
    }

    public static string GetVersionFromCheckResult(ModuleVersionType moduleVersionSource, DetectorCheckResult<ContainsModuleArgs, ContainsModuleData>? result)
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

    public static async Task PerformCheckAsync(CheckDefinition<ContainsModuleArgs, ContainsModuleData> definition, IEnumerable<IInputType> inputs, DetectorCheckResult<ContainsModuleArgs, ContainsModuleData> result, CancellationToken cancellationToken)
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

        foreach (var input in inputs)
        {
            // Stop evaluating inputs if we've gotten a cancellation or a result
            if (cancellationToken.IsCancellationRequested || result.CheckStatus != DetectorCheckStatus.InProgress) break;

            if (input is IModulesDataSource dataSource)
            { 
                await foreach (var module in dataSource.GetModulesAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var filenameMatch = definition.CheckArguments.Filename is null || string.Equals(definition.CheckArguments.Filename, module.FileName, StringComparison.InvariantCultureIgnoreCase) || (checkForNgenModule && string.Equals(nGenModuleName, module.FileName, StringComparison.InvariantCultureIgnoreCase));
                    var fileVersionMatch = fileVersionRange is null || SemVersion.TryParseCleaned(module.FileVersion, out var fileVersion) && fileVersionRange.Contains(fileVersion);

                    var originalFilenameMatch = definition.CheckArguments.OriginalFilename is null || string.Equals(definition.CheckArguments.OriginalFilename, module.OriginalFileName, StringComparison.InvariantCultureIgnoreCase) || (checkForNgenModule && string.Equals(nGenOriginalModuleName, module.OriginalFileName, StringComparison.InvariantCultureIgnoreCase));

                    var productNameMatch = definition.CheckArguments.ProductName is null || string.Equals(definition.CheckArguments.ProductName, module.ProductName, StringComparison.InvariantCultureIgnoreCase);
                    var productVersionMatch = productVersionRange is null || SemVersion.TryParseCleaned(module.ProductVersion, out var productVersion) && productVersionRange.Contains(productVersion);

                    var isLoadedMatch = definition.CheckArguments.IsLoaded is null || definition.CheckArguments.IsLoaded == module.IsLoaded;

                    if (filenameMatch && fileVersionMatch && originalFilenameMatch && productNameMatch && productVersionMatch && isLoadedMatch)
                    {
                        result.OutputData = new ContainsModuleData(module);
                        result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                        break;
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

public enum ModuleVersionType
{
    FileVersion = 0,
    ProductVersion,
}
