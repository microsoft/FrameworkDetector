// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> which represents a loose exectuable of an application binary to analyze.
/// </summary>
public record ExecutableInput(ExecutableImportedFunctionsMetadata[] ImportedFunctions,
                              ExecutableExportedFunctionsMetadata[] ExportedFunctions) 
    : IImportedFunctionsDataSource, IExportedFunctionsDataSource, // TODO: IModulesDataSource
      IInputType<FileInfo>
{
    public string Name => "executables";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo executable, CancellationToken cancellationToken)
    {
        // Get functions from the executable
        ExecutableImportedFunctionsMetadata[] importedFunctions = []; // process.ProcessImportedFunctionsMetadata(); // TODO: Move to new extensions on FileInfo

        ExecutableExportedFunctionsMetadata[] exportedFunctions = []; // process.ProcessExportedFunctionsMetadata();

        // TODO: Loop over Imported Functions to Produce Modules Data Source

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray());
    }
}
