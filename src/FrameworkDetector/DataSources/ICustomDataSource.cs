// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides custom data (usually provided by plugins) for a given input.
/// </summary>
public interface ICustomDataSource : IDataSource
{
    IEnumerable<object> GetCustomData(string key);

    virtual async IAsyncEnumerable<object> GetCustomDataAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in GetCustomData(key))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }
}
