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
public record ExecutableInput(string Filename,
                              ExecutableImportedFunctionsMetadata[] ImportedFunctions,
                              ExecutableExportedFunctionsMetadata[] ExportedFunctions) 
    : IImportedFunctionsDataSource, IExportedFunctionsDataSource, // TODO: IModulesDataSource
      IInputType<FileInfo>
{
    public string Name => "executables";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo executable, CancellationToken cancellationToken)
    {
        // Get functions from the executable
        ExecutableImportedFunctionsMetadata[] importedFunctions = (ExecutableImportedFunctionsMetadata[])executable.GetImportedFunctionsMetadata();

        ExecutableExportedFunctionsMetadata[] exportedFunctions = (ExecutableExportedFunctionsMetadata[])executable.GetExportedFunctionsMetadata();

        // TODO: Loop over Imported Functions to Produce Modules Data Source

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(executable.FullName,
                                   importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray());
    }
}
