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

    extension(DetectorCheckList @this)
    {
        /// <summary>
        /// <see cref="DetectorCheckList"/> extension to provide access to this check.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public DetectorCheckList ContainsModule(string moduleName)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<LoadedModulePresentInfo>(CheckRegistrationInfo, new LoadedModulePresentInfo(moduleName)));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task<DetectorCheckResult> PerformCheckAsync(CheckDefinition<LoadedModulePresentInfo> info, DataSourceCollection dataSources, CancellationToken cancellationToken)
    {
        var result = new DetectorCheckResult(info);        

        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.Status = DetectorCheckStatus.InProgress;

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
            // No Data?
            result.Status = DetectorCheckStatus.CompletedFailed;
        }

        // TODO: DetectorCheckResult extraData?

        return result;
    }
}
