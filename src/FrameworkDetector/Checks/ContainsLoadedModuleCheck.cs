// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="moduleName">The name of the module to look for.</param>
    /// <param name="checkForNgenModule">Whether or not to look for an Ngened version of the module.</param>
    /// <param name="versionRegex">Regex to match a specific version.</param>
    public readonly struct ContainsLoadedModuleArgs(string moduleName, bool checkForNgenModule, string? versionRegex)
    {
        public string ModuleName { get; } = moduleName;

        public bool CheckForNgenModule { get; } = checkForNgenModule;

        public string? VersionRegex { get; } = versionRegex;

        public override string ToString() => ModuleName;
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
        /// <param name="moduleName">The name of the module to look for.</param>
        /// <param name="checkForNgenModule">Whether or not to look for an Ngened version of the module.</param>
        /// <param name="versionRegex">Regex to match a specific version.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsLoadedModule(string moduleName, bool checkForNgenModule = false, string? versionRegex = null)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(CheckRegistrationInfo, new ContainsLoadedModuleArgs(moduleName, checkForNgenModule, versionRegex)));

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
                nGenModuleName = Path.ChangeExtension(definition.CheckArguments.ModuleName, ".ni" + Path.GetExtension(definition.CheckArguments.ModuleName));
            }

            Regex? versionRegex = null;
            if (definition.CheckArguments.VersionRegex is not null)
            {
                versionRegex = new Regex(definition.CheckArguments.VersionRegex);
            }

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

                        if (module.Filename.Equals(definition.CheckArguments.ModuleName, StringComparison.InvariantCultureIgnoreCase) ||
                            (definition.CheckArguments.CheckForNgenModule && module.Filename.Equals(nGenModuleName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (versionRegex is null ||
                                (module.FileVersion is not null && versionRegex.IsMatch(module.FileVersion)) ||
                                (module.ProductVersion is not null && versionRegex.IsMatch(module.ProductVersion)))
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
