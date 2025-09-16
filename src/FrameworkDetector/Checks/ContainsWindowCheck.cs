// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.DetectorChecks;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific window present within a process.
/// </summary>
public static class ContainsWindowCheck
{
    /// <summary>
    /// Static registration information defining <see cref="ContainsWindowCheck"/>.
    /// </summary>
    private static CheckRegistrationInfo<ContainsWindowArgs, ContainsWindowData> CheckRegistrationInfo = new(
        Name: nameof(ContainsWindowCheck),
        Description: "Checks for an active window in the Process by window class name",
        DataSourceIds: [ProcessDataSource.Id],
        PerformCheckAsync
    );

    /// <summary>
    /// Input arguments for <see cref="ContainsWindowCheck"/>.
    /// </summary>
    /// <param name="classNameRegex">The name of the window class to look for.</param>
    public readonly struct ContainsWindowArgs(string classNameRegex)
    {
        public string ClassNameRegex { get; } = classNameRegex;

        public override string ToString() => ClassNameRegex;
    }

    /// <summary>
    /// Output data for <see cref="ContainsWindowCheck"/>.
    /// </summary>
    /// <param name="windowFound">The window found.</param>
    public readonly struct ContainsWindowData(ProcessWindowMetadata windowFound)
    {
        public ProcessWindowMetadata WindowFound { get; } = windowFound;

        public override string ToString() => WindowFound.ToString();
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an active window in the Process by window class name.
        /// </summary>
        /// <param name="classNameRegex">The name of the window class to look for.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsWindow(string classNameRegex)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<ContainsWindowArgs, ContainsWindowData>(CheckRegistrationInfo, new ContainsWindowArgs(classNameRegex)));

            return @this;
        }
    }

    //// Actual check code run by engine
    
    public static async Task PerformCheckAsync(CheckDefinition<ContainsWindowArgs, ContainsWindowData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsWindowArgs, ContainsWindowData> result, CancellationToken cancellationToken)
    {
        Regex? classNameRegex = null;

        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            await Task.Yield();

            classNameRegex ??= new Regex(definition.CheckArguments.ClassNameRegex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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

                        if (window.ClassName is not null && classNameRegex.IsMatch(window.ClassName))
                        {
                            result.OutputData = new ContainsWindowData(window);
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
