// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> which represents a loose exectuable of an application binary to analyze.
/// </summary>
public record ExecutableInput(WindowsModuleMetadata ExecutableMetadata,
                              ImportedFunctionsMetadata[] ImportedFunctions,
                              ExportedFunctionsMetadata[] ExportedFunctions,
                              WindowsModuleMetadata[] Modules) 
    : IImportedFunctionsDataSource, 
      IExportedFunctionsDataSource,
      IModulesDataSource,
      IInputTypeFactory<FileInfo>,
      IInputType
{
    public string Name => "executables";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo executable, bool? isLoaded, CancellationToken cancellationToken)
    {
        // Get Metadata
        var metadata = WindowsModuleMetadata.GetMetadata(executable.FullName, isLoaded == true);

        await Task.Yield();

        if (cancellationToken.IsCancellationRequested)
        {
            // TODO: Throw OperationCanceledException instead? Feel like we need to figure out how to make handling cancellation during initialization easier for the implementors down here and how that flows back up to the CLI.
            return null!;
        }

        // Get functions from the executable
        var importedFunctions = executable.GetImportedFunctionsMetadata();

        // Loop over Imported Functions to Produce Modules Data Source
        HashSet<WindowsModuleMetadata> modules = new();
        foreach (var function in importedFunctions)
        {
            var moduleMetadata = WindowsModuleMetadata.GetMetadata(function.ModuleName, false);
            modules.Add(moduleMetadata);
        }

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(metadata,
                                   importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   executable.GetExportedFunctionsMetadata().OrderBy(f => f.Name).ToArray(),
                                   modules.ToArray());
    }
}
