// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Interface defining a helper method for creating custom data for a given raw input type <see cref="T"/>.
/// This type is primarily of use by Plugin authors who want to extend <see cref="IInputType{T}"/> types because they also implement <see cref="ICustomDataSource"/>.
/// </summary>
/// <typeparam name="T">The raw input type.</typeparam>
public interface ICustomDataFactory<T>
{
    /// <summary>
    /// Create custom data from the given <paramref name="input"/>. Needs to return a key and list of data objects.
    /// Multiple <see cref="ICustomDataFactory{T}"/> can share the same key, which will be used to group the data objects in the final custom data object.
    /// </summary>
    /// <param name="input">The input <see cref="T"/> instance.</param>
    /// <param name="isLoaded">Specifies, if known (not null), whether or not the input was loaded in memory when processed.</param>
    /// <param name="cancellationToken">A cancellation token to disrupt initialization.</param>
    /// <returns>The custom data.</returns>
    Task<KeyValuePair<string, IReadOnlyList<object>>> CreateCustomDataAsync(T input, bool? isLoaded, CancellationToken cancellationToken);
}
