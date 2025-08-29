// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

/// <summary>
/// Check extension for looking for a specific loaded module, present within a process.
/// </summary>
public static class LoadedModulePresentCheck
{
    /// <summary>
    /// Static registration information defining the LoadedModulePresent Check
    /// </summary>
    private static CheckRegistrationInfo<LoadedModulePresentInfo> CheckRegistrationInfo = new(
        Name: nameof(LoadedModulePresentCheck),
        Description: "Checks for {0} in Process.LoadedModules",
        DataSourceIds: [ProcessDataSource.Id],
        PerformCheckAsync
    );

    /// <summary>
    /// Structure for custom metadata provided by a detector (in this case the module name) required to perform the check.
    /// </summary>
    /// <param name="ModuleName"></param>
    public readonly struct LoadedModulePresentInfo(string ModuleName)
    {
        public string ModuleName { get; } = ModuleName;

        public override string ToString() => ModuleName;
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// <see cref="DetectorCheckGroup"/> extension to provide access to this check.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsModule(string moduleName)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<LoadedModulePresentInfo>(CheckRegistrationInfo, new LoadedModulePresentInfo(moduleName)));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<LoadedModulePresentInfo> info, DataSourceCollection dataSources, DetectorCheckResult<LoadedModulePresentInfo> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.Status = DetectorCheckStatus.InProgress;

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
