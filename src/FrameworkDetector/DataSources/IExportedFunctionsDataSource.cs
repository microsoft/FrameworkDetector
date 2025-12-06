// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about exported functions from an executable.
/// </summary>
public interface IExportedFunctionsDataSource : IDataSource
{
    ExportedFunctionsMetadata[] ExportedFunctions { get; }
}
