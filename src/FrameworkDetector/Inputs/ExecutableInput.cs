// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> which represents a package retrieved from the installed package store on the system.
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

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray());
    }
}
