// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

public static class LoadedModulePresentDetectorCheck
{
    private static CheckInfo<LoadedModulePresentInfo> CheckInfo = new(
        Name: nameof(LoadedModulePresentDetectorCheck),
        Description: "Detect moduleName in Process.LoadedModules",
        DataSourceIds: [ProcessDataSource.Id],
        PerformCheckAsync
    );

    public readonly struct LoadedModulePresentInfo(string ModuleName)
    {
        public string ModuleName { get; } = ModuleName;
    }

    extension(DetectorDefinition @this)
    {
        public DetectorCheckList ContainsModule(string moduleName)
        {
            var cd = new CheckDefinition<LoadedModulePresentInfo>(CheckInfo, new LoadedModulePresentInfo(moduleName));

            // TODO: How to store these and return them to the same DetectorDefinition...
        }
    }

    public static Task<DetectorCheckResult> PerformCheckAsync(LoadedModulePresentInfo info, Dictionary<Guid, IDataSource> dataSources, CancellationToken ct)
    {
        var result = new DetectorCheckResult();

        // TODO: DetectorCheckResult extraData
        if (Process is null)
        {
            throw new ArgumentNullException(nameof(Process));
        }

        result.Status = DetectorCheckStatus.InProgress;

        foreach (var processModule in Process.Modules.Cast<ProcessModule>())
        {
            await Task.Yield();

            if (cancellationToken.IsCancellationRequested)
            {
                result.Status = DetectorCheckStatus.Canceled;
                break;
            }

            if (processModule.ModuleName.Equals(ModuleName, StringComparison.InvariantCultureIgnoreCase))
            {
                result.Status = DetectorCheckStatus.CompletedPassed;
                break;
            }
        }

        if (result.Status == DetectorCheckStatus.InProgress)
        {
            result.Status = DetectorCheckStatus.CompletedFailed;
        }

        return result.Status;
    }
}
