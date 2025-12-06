// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
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
            DataSourceInterfaces: [typeof(IExportedFunctionsDataSource)],
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
            var descriptionSB = new StringBuilder("Find exported function ");

            if (Name is not null)
            {
                descriptionSB.AppendFormat("name has \"{0}\"", Name);
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(Name, nameof(ContainsExportedFunctionArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsExportedFunctionCheck"/>.
    /// </summary>
    /// <param name="exportedFunctionFound">The exported function found.</param>
    public readonly struct ContainsExportedFunctionData(ExportedFunctionsMetadata exportedFunctionFound)
    {
        public ExportedFunctionsMetadata ExportedFunctionFound { get; } = exportedFunctionFound;
    }

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an exported function by name in the PE headers of the process.
        /// </summary>
        /// <param name="name">An exported function's name must contain this text, if specified.</param>
        /// <returns></returns>
        public IDetectorCheckGroup ContainsExportedFunction(string? name = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.

            var args = new ContainsExportedFunctionArgs(name);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsExportedFunctionArgs, ContainsExportedFunctionData>(GetCheckRegistrationInfo(args), args));

            return dcg;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsExportedFunctionArgs, ContainsExportedFunctionData> definition, IReadOnlyList<IInputType> inputs, DetectorCheckResult<ContainsExportedFunctionArgs, ContainsExportedFunctionData> result, CancellationToken cancellationToken)
    {
        result.CheckStatus = DetectorCheckStatus.InProgress;

        foreach (var input in inputs)
        {
            if (input is IExportedFunctionsDataSource dataSource
                && dataSource.ExportedFunctions is not null)
            {
                foreach (var exportedFunction in dataSource.ExportedFunctions)
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
}
