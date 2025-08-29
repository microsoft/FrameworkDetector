// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// Check extension for looking for a specific loaded module, present within a process.
/// </summary>
public static class ContainsLoadedModuleCheck
{
    /// <summary>
    /// Static registration information defining the ContainsLoadedModule Check
    /// </summary>
    private static CheckRegistrationInfo<ContainsLoadedModuleInfo> CheckRegistrationInfo = new(
        Name: nameof(ContainsLoadedModuleCheck),
        Description: "Checks for module by name in Process.LoadedModules",
        DataSourceIds: [ProcessDataSource.Id],
        PerformCheckAsync
    );

    /// <summary>
    /// Structure for custom metadata provided by a detector (in this case the module name) required to perform the check.
    /// </summary>
    /// <param name="moduleName">The name of the module to look for.</param>
    /// <param name="checkForNgenModule">Whether or not to look for an Ngened version of the module.</param>
    public readonly struct ContainsLoadedModuleInfo(string moduleName, bool checkForNgenModule)
    {
        public string ModuleName { get; } = moduleName;

        public bool CheckForNgenModule { get; } = checkForNgenModule;

        public override string ToString() => ModuleName;
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// <see cref="DetectorCheckGroup"/> extension to provide access to this check.
        /// </summary>
        /// <param name="moduleName">The name of the module to look for.</param>
        /// <param name="checkForNgenModule">Whether or not to look for an Ngened version of the module.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsLoadedModule(string moduleName, bool checkForNgenModule = false)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<ContainsLoadedModuleInfo>(CheckRegistrationInfo, new ContainsLoadedModuleInfo(moduleName, checkForNgenModule)));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsLoadedModuleInfo> info, DataSourceCollection dataSources, DetectorCheckResult<ContainsLoadedModuleInfo> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.Status = DetectorCheckStatus.InProgress;

            string? nGenModuleName = null;
            if (info.Metadata.CheckForNgenModule)
            {
                nGenModuleName = Path.ChangeExtension(info.Metadata.ModuleName, ".ni" + Path.GetExtension(info.Metadata.ModuleName));
            }

            // TODO: Think about child processes and what that means here for a check...
            foreach (ProcessDataSource process in processes)
            {
                foreach (var module in process.Modules)
                {
                    await Task.Yield();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.Status = DetectorCheckStatus.Canceled;
                        break;
                    }

                    if (module.ModuleName.Equals(info.Metadata.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Status = DetectorCheckStatus.CompletedPassed;
                        break;
                    }
                    else if (info.Metadata.CheckForNgenModule && module.ModuleName.Equals(nGenModuleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Status = DetectorCheckStatus.CompletedPassed;
                        break;
                    }
                }
            }

            if (result.Status == DetectorCheckStatus.InProgress)
            {
                result.Status = DetectorCheckStatus.CompletedFailed;
            }
        }
        else
        {
            // No Data = Error
            result.Status = DetectorCheckStatus.Error;
        }
    }
}
