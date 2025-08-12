// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector;

public abstract class Detector
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string FrameworkId { get; }

    public IReadOnlyList<string> ModuleNames => _moduleNames;
    protected readonly List<string> _moduleNames = new List<string>();

    public virtual async Task<DetectorResult> DetectByProcessAsync(Process process, CancellationToken cancellationToken)
    {
        var result = new DetectorResult()
        {
            DetectorName = Name,
            DetectorVersion = AssemblyInfo.LibraryVersion,
            FrameworkId = FrameworkId,
        };

        if (!cancellationToken.IsCancellationRequested)
        {
            var moduleDetectionResults = new ConcurrentDictionary<string, bool>();

            await Parallel.ForEachAsync(ModuleNames, async (moduleName, token) =>
            {
                bool moduleDetected = false;

                foreach (var processModule in process.Modules.Cast<ProcessModule>())
                {
                    await Task.Yield();

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (processModule.ModuleName.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        moduleDetected = true;
                        break;
                    }
                }

                moduleDetectionResults[moduleName] = moduleDetected;
            });

            if (moduleDetectionResults.Count == ModuleNames.Count)
            {
                // Every target module was detected (or not) without early cancelation
                result.FrameworkFound = moduleDetectionResults.All(kvp => kvp.Value);
                result.Status = DetectorResultStatus.Completed;
            }

            if (moduleDetectionResults.Count > 0)
            {
                // There's at least some data
                result.Data = new JsonObject();
                foreach (var kvp in moduleDetectionResults)
                {
                    result.Data[kvp.Key] = kvp.Value;
                }
            }
        }

        if (cancellationToken.IsCancellationRequested && result.Status != DetectorResultStatus.Completed)
        {
            result.Status = DetectorResultStatus.Canceled;
        }

        return result;
    }
}
