// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific exported function, present within the PE headers of a process binary.
/// </summary>
public static class ContainsExportedFunctionCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsExportedFunctionCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsExportedFunctionArgs, ContainsExportedFunctionData> GetCheckRegistrationInfo(ContainsExportedFunctionArgs args)
    {
        return new(
            Name: nameof(ContainsExportedFunctionCheck),
            Description: args.GetDescription(),
            DataSourceIds: [ProcessDataSource.Id],
            PerformCheckAsync);
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsExportedFunctionCheck"/>.
    /// </summary>
    /// <param name="name">An exported function's name must contain this text, if specified.</param>
    public readonly struct ContainsExportedFunctionArgs(string? name) : ICheckArgs
    {
        public string? Name { get; } = name;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find exported function containing ");

            if (Name is not null)
            {
                descriptionSB.AppendFormat("{0}", Name);
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(Name, nameof(ContainsExportedFunctionArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsExportedFunctionCheck"/>.
    /// </summary>
    /// <param name="exportedFunctionFound">The exported function found.</param>
    public readonly struct ContainsExportedFunctionData(ProcessExportedFunctionsMetadata exportedFunctionFound)
    {
        public ProcessExportedFunctionsMetadata ExportedFunctionFound { get; } = exportedFunctionFound;
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an exported function by name in the PE headers of the process.
        /// </summary>
        /// <param name="name">An exported function's name must contain this text, if specified.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsExportedFunction(string? name = null)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.

            var args = new ContainsExportedFunctionArgs(name);
            args.Validate();

            @this.AddCheck(new CheckDefinition<ContainsExportedFunctionArgs, ContainsExportedFunctionData>(GetCheckRegistrationInfo(args), args));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsExportedFunctionArgs, ContainsExportedFunctionData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsExportedFunctionArgs, ContainsExportedFunctionData> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out IProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            foreach (var process in processes)
            {
                var exportedFunctions = process.ProcessMetadata?.ExportedFunctions;
                if (exportedFunctions is not null)
                {
                    foreach (var exportedFunction in exportedFunctions)
                    {
                        await Task.Yield();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            result.CheckStatus = DetectorCheckStatus.Canceled;
                            break;
                        }

                        var nameMatch = definition.CheckArguments.Name is null || exportedFunction.Name is null || exportedFunction.Name.Contains(definition.CheckArguments.Name, StringComparison.InvariantCultureIgnoreCase);

                        if (nameMatch)
                        {
                            result.OutputData = new ContainsExportedFunctionData(exportedFunction);
                            result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                            break;
                        }
                    }
                }

                // Stop evaluating other process data sources if we've gotten a pass or cancel
                if (result.CheckStatus != DetectorCheckStatus.InProgress)
                {
                    break;
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
