// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about the active windows for a given input.
/// </summary>
public interface IActiveWindowsDataSource : IDataSource
{
    IEnumerable<ActiveWindowMetadata> GetActiveWindows();

    virtual async IAsyncEnumerable<ActiveWindowMetadata> GetActiveWindowsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in GetActiveWindows())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }
}
