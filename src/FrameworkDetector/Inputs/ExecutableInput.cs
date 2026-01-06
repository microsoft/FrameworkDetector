// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> which represents a loose executable of an application binary to analyze.
/// </summary>
public record ExecutableInput(WindowsModuleMetadata ExecutableMetadata,
                              ImportedFunctionsMetadata[] ImportedFunctions,
                              ExportedFunctionsMetadata[] ExportedFunctions,
                              WindowsModuleMetadata[] ImportedModules) 
    : IEquatable<ExecutableInput>,
      IImportedFunctionsDataSource, 
      IExportedFunctionsDataSource,
      IModulesDataSource,
      IInputTypeFactory<FileInfo>,
      IInputType
{
    [JsonIgnore]
    public string InputGroup => "executables";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo executable, bool? isLoaded, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get executable's own metadata
        var metadata = WindowsModuleMetadata.GetMetadata(executable.FullName, isLoaded == true);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get functions imported by the executable
        var importedFunctions = executable.GetImportedFunctionsMetadata();

        // Loop over Imported Functions to produce ModulesDataSource
        HashSet<WindowsModuleMetadata> importedModules = new();
        foreach (var function in importedFunctions)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            var moduleName = function.ModuleName;

            // Module names extracted from imported functions do not contain paths, check path of executable
            if (!Path.IsPathFullyQualified(moduleName))
            {
                var moduleFullPath = Path.GetFullPath(moduleName, Path.GetDirectoryName(executable.FullName) ?? "");
                if (Path.Exists(moduleFullPath))
                {
                    moduleName = moduleFullPath;
                }
            }

            var moduleMetadata = WindowsModuleMetadata.GetMetadata(moduleName, false);
            importedModules.Add(moduleMetadata);
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get functions exposed by executable
        var exportedFunctions = executable.GetExportedFunctionsMetadata();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(metadata,
                                   importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray(),
                                   importedModules.ToArray());
    }

    public override int GetHashCode() => ExecutableMetadata.GetHashCode();

    public virtual bool Equals(ExecutableInput? input)
    {
        if (input is null)
        {
            return false;
        }

        return ExecutableMetadata == input.ExecutableMetadata;
    }

    public IEnumerable<ImportedFunctionsMetadata> GetImportedFunctions() => ImportedFunctions;

    public IEnumerable<ExportedFunctionsMetadata> GetExportedFunctions() => ExportedFunctions;

    public IEnumerable<WindowsModuleMetadata> GetModules() => ImportedModules;
}
