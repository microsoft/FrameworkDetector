// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;

namespace FrameworkDetector;

public static class AppListEntryExtensions
{
    /// <summary>
    /// Launches the given <see cref="AppListEntry"/> and returns the corresponding <see cref="Process"/>.
    /// </summary>
    /// <param name="appListEntry">The target app list entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The corresponsing process.</returns>
    public static async Task<Process?> LaunchAndGetProcessAsync(this AppListEntry appListEntry, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if (await appListEntry.LaunchAsync())
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        if (process.TryGetApplicationUserModelId(out var aumid) && aumid == appListEntry.AppUserModelId)
                        {
                            return process;
                        }
                    }
                    catch { } // Ignore processes we can't access the start time for.
                }

                // Wait before querying the process list again
                try
                {
                    await Task.Delay(500, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }

        }

        return null;
    }
}
