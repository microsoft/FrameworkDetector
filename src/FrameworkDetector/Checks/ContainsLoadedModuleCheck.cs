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
    /// Static registration information defining <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    private static CheckRegistrationInfo<ContainsLoadedModuleArgs, ContainsLoadedModuleData> CheckRegistrationInfo = new(
        Name: nameof(ContainsLoadedModuleCheck),
        Description: "Checks for module by name in Process.LoadedModules",
        DataSourceIds: [ProcessDataSource.Id],
        PerformCheckAsync
    );

    /// <summary>
    /// Input arguments for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="filename">The filename of the module to look for.</param>
    /// <param name="checkForNgenModule">Whether or not to look for an NGENed version of the module.</param>
    /// <param name="fileVersionRange">Semver version range sepc to match a specific module version.</param>
    public readonly struct ContainsLoadedModuleArgs(string filename, bool checkForNgenModule, string? fileVersionRange)
    {
        public string Filename { get; } = filename;

        public bool CheckForNgenModule { get; } = checkForNgenModule;

        public string? FileVersionRange { get; } = fileVersionRange;

        public override string ToString() => Filename;
    }

    /// <summary>
    /// Output data for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="moduleFound">The module found.</param>
    public readonly struct ContainsLoadedModuleData(WindowsBinaryMetadata moduleFound)
    {
        public WindowsBinaryMetadata ModuleFound { get; } = moduleFound;

        public override string ToString() => ModuleFound.ToString();
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for module by name in Process.LoadedModules.
        /// </summary>
        /// <param name="filename">The filename of the module to look for.</param>
        /// <param name="checkForNgenModule">Whether or not to look for an NGENed version of the module.</param>
        /// <param name="fileVersionRange">Semver version range sepc to match a specific module version.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsLoadedModule(string filename, bool checkForNgenModule = false, string? fileVersionRange = null)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(CheckRegistrationInfo, new ContainsLoadedModuleArgs(filename, checkForNgenModule, fileVersionRange)));

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
            if (definition.CheckArguments.CheckForNgenModule)
            {
                nGenModuleName = Path.ChangeExtension(definition.CheckArguments.Filename, ".ni" + Path.GetExtension(definition.CheckArguments.Filename));
            }

            var fileVersionRange = definition.CheckArguments.FileVersionRange is not null ? SemVersionRange.Parse(definition.CheckArguments.FileVersionRange) : null;

            // TODO: Think about child processes and what that means here for a check...
            foreach (ProcessDataSource process in processes)
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

                        if (module.Filename.Equals(definition.CheckArguments.Filename, StringComparison.InvariantCultureIgnoreCase) ||
                            (definition.CheckArguments.CheckForNgenModule && module.Filename.Equals(nGenModuleName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (fileVersionRange is null || SemVersion.TryLooseParse(module.FileVersion, out var fileVersion) && fileVersionRange.Contains(fileVersion))
                            {
                                result.OutputData = new ContainsLoadedModuleData(module);
                                result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                            }
                            break;
                        }
                    }
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
