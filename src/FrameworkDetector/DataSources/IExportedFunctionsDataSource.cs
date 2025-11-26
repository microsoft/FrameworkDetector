// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about exported functions from an executable.
/// </summary>
public interface IExportedFunctionsDataSource : IDataSource
{
    ExecutableExportedFunctionsMetadata[] ExportedFunctions { get; }
}
