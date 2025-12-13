// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about exported functions for a given input.
/// </summary>
public interface IExportedFunctionsDataSource : IDataSource
{
    IEnumerable<ExportedFunctionsMetadata> GetExportedFunctions();

    virtual async IAsyncEnumerable<ExportedFunctionsMetadata> GetExportedFunctionsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in GetExportedFunctions())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }
}
