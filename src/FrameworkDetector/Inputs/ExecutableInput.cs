// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
                              WindowsModuleMetadata[] ImportedModules,
                              IReadOnlyDictionary<string, IReadOnlyList<object>> CustomData) 
    : IEquatable<ExecutableInput>,
      IImportedFunctionsDataSource, 
      IExportedFunctionsDataSource,
      IModulesDataSource,
      ICustomDataSource,
      IInputTypeFactory<FileInfo>,
      IInputType<FileInfo>
{
    [JsonIgnore]
    public string InputGroup => "executables";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo executable, bool? isLoaded, CustomDataFactoryCollection<FileInfo>? customDataFactories, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get executable's own metadata (always not loaded)
        var metadata = WindowsModuleMetadata.GetMetadata(executable.FullName, false);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get functions imported by the executable
        var importedFunctions = executable.GetImportedFunctionsMetadata();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Loop over Imported Functions to get ImportedModules
        var importedModules = GetModulesFromImportedFunctionsMetadata(executable, importedFunctions);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get functions exposed by executable
        var exportedFunctions = executable.GetExportedFunctionsMetadata();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Load CustomData
        var customData = customDataFactories is not null ? await customDataFactories.CreateCustomDataAsync(executable, isLoaded, cancellationToken) : new Dictionary<string, IReadOnlyList<object>>(0);

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(metadata,
                                   importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray(),
                                   importedModules.OrderBy(m => m.FileName).ToArray(),
                                   customData);
    }

    protected static IReadOnlySet<WindowsModuleMetadata> GetModulesFromImportedFunctionsMetadata(FileInfo executable, IReadOnlySet<ImportedFunctionsMetadata> importedFunctions)
    {
        var executablePath = Path.GetDirectoryName(executable.FullName) ?? ".\\";

        HashSet<WindowsModuleMetadata> importedModules = new();
        foreach (var function in importedFunctions)
        {
            var moduleName = function.ModuleName;

            // Module names extracted from imported functions do not contain paths, check path of executable
            if (!Path.IsPathFullyQualified(moduleName))
            {
                var moduleFullPath = Path.GetFullPath(moduleName, executablePath);
                if (Path.Exists(moduleFullPath))
                {
                    moduleName = moduleFullPath;
                }
            }

            var moduleMetadata = WindowsModuleMetadata.GetMetadata(moduleName, false);
            importedModules.Add(moduleMetadata);
        }
        return importedModules;
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

    public IEnumerable<object> GetCustomData(string key) => CustomData.TryGetValue(key, out var values) ? values : Enumerable.Empty<object>();
}
