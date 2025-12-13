// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about imported functions for a given input.
/// </summary>
public interface IImportedFunctionsDataSource : IDataSource
{
    IEnumerable<ImportedFunctionsMetadata> GetImportedFunctions();

    virtual async IAsyncEnumerable<ImportedFunctionsMetadata> GetImportedFunctionsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in GetImportedFunctions())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }
}
