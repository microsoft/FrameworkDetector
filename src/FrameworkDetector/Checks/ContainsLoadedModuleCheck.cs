// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific loaded module, present within a process.
/// </summary>
public static class ContainsLoadedModuleCheck
{
    /// <summary>
    /// Static registration information defining <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    private static CheckRegistrationInfo<ContainsLoadedModuleArgs, ContainsLoadedModuleData> CheckRegistrationInfo = new(
        Name: nameof(ContainsLoadedModuleCheck),
        Description: "Checks for module by name in Process.LoadedModules",
        DataSourceIds: [ProcessDataSource.Id],
        PerformCheckAsync
    );

    /// <summary>
    /// Input arguments for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="filenameRegex">Regex to match the filename of the module.</param>
    /// <param name="fileVersionRegex">Regex to match the file version of the module.</param>
    public readonly struct ContainsLoadedModuleArgs(string filenameRegex, string? fileVersionRegex)
    {
        public string FilenameRegex { get; } = filenameRegex;

        public string? FileVersionRegex { get; } = fileVersionRegex;

        public override string ToString() => FilenameRegex;
    }

    /// <summary>
    /// Output data for <see cref="ContainsLoadedModuleCheck"/>.
    /// </summary>
    /// <param name="moduleFound">The module found.</param>
    public readonly struct ContainsLoadedModuleData(WindowsBinaryMetadata moduleFound)
    {
        public WindowsBinaryMetadata ModuleFound { get; } = moduleFound;

        public override string ToString() => ModuleFound.ToString();
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for module by name in Process.LoadedModules.
        /// </summary>
        /// <param name="filenameRegex">Regex to match the filename of the module.</param>
        /// <param name="fileVersionRegex">Regex to match the file version of the module.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsLoadedModule(string filenameRegex, string? fileVersionRegex = null)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            @this.AddCheck(new CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(CheckRegistrationInfo, new ContainsLoadedModuleArgs(filenameRegex, fileVersionRegex)));

            return @this;
        }
    }

    //// Actual check code run by engine
    
    public static async Task PerformCheckAsync(CheckDefinition<ContainsLoadedModuleArgs, ContainsLoadedModuleData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsLoadedModuleArgs, ContainsLoadedModuleData> result, CancellationToken cancellationToken)
    {
        Regex? filenameRegex = null;
        Regex? fileVersionRegex = null;

        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            await Task.Yield();

            filenameRegex ??= new Regex(definition.CheckArguments.FilenameRegex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            if (fileVersionRegex is null && definition.CheckArguments.FileVersionRegex is not null)
            {
                fileVersionRegex = new Regex(definition.CheckArguments.FileVersionRegex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }

            // TODO: Think about child processes and what that means here for a check...
            foreach (ProcessDataSource process in processes)
            {
                var loadedModules = process.ProcessMetadata?.LoadedModules;
                if (loadedModules is not null)
                {
                    foreach (var module in loadedModules)
                    {
                        await Task.Yield();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            result.CheckStatus = DetectorCheckStatus.Canceled;
                            break;
                        }

                        if (filenameRegex.IsMatch(module.Filename))
                        {
                            if (fileVersionRegex is null ||
                                (module.FileVersion is not null && fileVersionRegex.IsMatch(module.FileVersion)))
                            {
                                result.OutputData = new ContainsLoadedModuleData(module);
                                result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                            }
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
