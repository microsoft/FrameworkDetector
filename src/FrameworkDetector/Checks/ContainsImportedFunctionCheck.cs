// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific imported function, present within the PE headers of a process binary.
/// </summary>
public static class ContainsImportedFunctionCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsImportedFunctionCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsImportedFunctionArgs, ContainsImportedFunctionData> GetCheckRegistrationInfo(ContainsImportedFunctionArgs args)
    {
        return new(
            Name: nameof(ContainsImportedFunctionCheck),
            Description: args.GetDescription(),
            DataSourceInterfaces: [typeof(IImportedFunctionsDataSource)],
            PerformCheckAsync);
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsImportedFunctionCheck"/>.
    /// </summary>
    /// <param name="moduleName">An imported function's module name must match this text, if specified.</param>
    /// <param name="functionName">An imported function's function name must contain this text, if specified.</param>
    /// <param name="delayLoaded">An imported function must be delay loaded (or not), if specified.</param>
    public readonly struct ContainsImportedFunctionArgs(string? moduleName = null, string? functionName = null, bool? delayLoaded = null) : ICheckArgs
    {
        public string? ModuleName { get; } = moduleName;

        public string? FunctionName { get; } = functionName;

        public bool? DelayLoaded { get; } = delayLoaded;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find ");

            if (DelayLoaded is not null && DelayLoaded.Value)
            {
                descriptionSB.Append("delay-loaded ");
            }

            descriptionSB.Append("imported function ");

            bool nameAdded = false;
            if (FunctionName is not null)
            {
                descriptionSB.AppendFormat("name has \"{0}\"", FunctionName);
                nameAdded = true;
            }

            if (ModuleName is not null)
            {
                if (nameAdded)
                {
                    descriptionSB.Append(' ');
                }
                descriptionSB.AppendFormat("in module \"{0}\"", ModuleName);
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(ModuleName ?? FunctionName, nameof(ContainsImportedFunctionArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsImportedFunctionCheck"/>.
    /// </summary>
    /// <param name="importedFunctionFound">The imported function found.</param>
    public readonly struct ContainsImportedFunctionData(ImportedFunctionsMetadata importedFunctionFound)
    {
        public ImportedFunctionsMetadata ImportedFunctionFound { get; } = importedFunctionFound;
    }

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an imported function in the PE headers of the process.
        /// </summary>
        /// <param name="moduleName">An imported function's module name must match this text, if specified.</param>
        /// <param name="functionName">An imported function's function name must contain this text, if specified.</param>
        /// <param name="delayLoaded">An imported function must be delay loaded (or not), if specified.</param>
        /// <returns></returns>
        public IDetectorCheckGroup ContainsImportedFunction(string? moduleName = null, string? functionName = null, bool? delayLoaded = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.

            var args = new ContainsImportedFunctionArgs(moduleName, functionName, delayLoaded);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsImportedFunctionArgs, ContainsImportedFunctionData>(GetCheckRegistrationInfo(args), args));

            return dcg;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsImportedFunctionArgs, ContainsImportedFunctionData> definition, IReadOnlyList<IInputType> inputs, DetectorCheckResult<ContainsImportedFunctionArgs, ContainsImportedFunctionData> result, CancellationToken cancellationToken)
    {
        result.CheckStatus = DetectorCheckStatus.InProgress;

        foreach (var input in inputs)
        {
            if (input is IImportedFunctionsDataSource dataSource
                && dataSource.ImportedFunctions is not null)
            {
                foreach (var importedFunction in dataSource.ImportedFunctions)
                {
                    await Task.Yield();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.CheckStatus = DetectorCheckStatus.Canceled;
                        break;
                    }

                    var moduleNameMatch = definition.CheckArguments.ModuleName is null || string.Equals(definition.CheckArguments.ModuleName, importedFunction.ModuleName, StringComparison.InvariantCultureIgnoreCase);

                    if (moduleNameMatch)
                    {
                        var functionMatch = (definition.CheckArguments.FunctionName is null && definition.CheckArguments.DelayLoaded is null)
                                            || (importedFunction.Functions is not null && importedFunction.Functions.Any(f =>
                        {
                            return (definition.CheckArguments.FunctionName is null || f.Name.Contains(definition.CheckArguments.FunctionName, StringComparison.InvariantCultureIgnoreCase))
                                && (definition.CheckArguments.DelayLoaded is null || definition.CheckArguments.DelayLoaded == f.DelayLoaded);
                        }));

                        if (functionMatch)
                        {
                            result.OutputData = new ContainsImportedFunctionData(importedFunction);
                            result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                            break;
                        }
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
