// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Interface defining a helper method for creating custom data for a given raw input type T.
/// This type is primarily of use by Plugin authors who want to extend <see cref="IInputTypeFactory{T}"/> types that also implement <see cref="ICustomDataSource"/>.
/// </summary>
/// <typeparam name="T">The raw input type.</typeparam>
public interface ICustomDataFactory<T>
{
    /// <summary>
    /// Create custom data from the given <paramref name="input"/>. Needs to return a key and list of data objects.
    /// Multiple <see cref="ICustomDataFactory{T}"/> can share the same key, which will be used to group the data objects in the final custom data object.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isLoaded"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The custom data.</returns>
    Task<KeyValuePair<string, IReadOnlyList<object>>> CreateCustomDataAsync(T input, bool? isLoaded, CancellationToken cancellationToken);
}
