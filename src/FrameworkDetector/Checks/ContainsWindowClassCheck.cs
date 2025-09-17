// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific window class present within a process.
/// </summary>
public static class ContainsWindowClassCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsWindowClassCheck"/>.
    /// </summary>
    private static CheckRegistrationInfo<ContainsWindowClassArgs, ContainsWindowClassData> GetCheckRegistrationInfo(ContainsWindowClassArgs args)
    {
        return new(
            Name: nameof(ContainsWindowClassCheck),
            Description: $"Find window {args.WindowClassName}",
            DataSourceIds: [ProcessDataSource.Id],
            PerformCheckAsync
            );
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsWindowClassCheck"/>.
    /// </summary>
    /// <param name="windowClassName">The name of the window class to look for.</param>
    public readonly struct ContainsWindowClassArgs(string windowClassName)
    {
        public string WindowClassName { get; } = windowClassName;
    }

    /// <summary>
    /// Output data for <see cref="ContainsWindowClassCheck"/>.
    /// </summary>
    /// <param name="windowFound">The window found.</param>
    public readonly struct ContainsWindowClassData(ProcessWindowMetadata windowFound)
    {
        public ProcessWindowMetadata WindowFound { get; } = windowFound;
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an active window in the Process by window class name.
        /// </summary>
        /// <param name="windowClassName">The name of the window class to look for.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsWindowClass(string windowClassName)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            var args = new ContainsWindowClassArgs(windowClassName);
            @this.AddCheck(new CheckDefinition<ContainsWindowClassArgs, ContainsWindowClassData>(GetCheckRegistrationInfo(args), args));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsWindowClassArgs, ContainsWindowClassData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsWindowClassArgs, ContainsWindowClassData> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            // TODO: Think about child processes and what that means here for a check...
            foreach (ProcessDataSource process in processes)
            {
                var activeWindows = process.ProcessMetadata?.ActiveWindows;
                if (activeWindows is not null)
                {
                    foreach (var window in activeWindows)
                    {
                        await Task.Yield();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            result.CheckStatus = DetectorCheckStatus.Canceled;
                            break;
                        }

                        if (window.ClassName is not null && window.ClassName.Contains(definition.CheckArguments.WindowClassName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.OutputData = new ContainsWindowClassData(window);
                            result.CheckStatus = DetectorCheckStatus.CompletedPassed;
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
