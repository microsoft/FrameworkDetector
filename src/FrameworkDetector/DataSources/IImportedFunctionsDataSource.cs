// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about imported functions from an executable.
/// </summary>
public interface IImportedFunctionsDataSource : IDataSource
{
    ImportedFunctionsMetadata[] ImportedFunctions { get; }
}
