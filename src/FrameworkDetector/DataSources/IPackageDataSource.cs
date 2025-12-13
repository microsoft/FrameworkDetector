// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about the packages for a given input.
/// </summary>
public interface IPackageDataSource : IDataSource
{
    IEnumerable<PackageMetadata> GetPackages();

    virtual async IAsyncEnumerable<PackageMetadata> GetPackagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in GetPackages())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }
}
