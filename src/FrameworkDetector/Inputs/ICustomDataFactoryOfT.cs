// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Interface defining a helper method for creating custom data for a given raw input type T.
/// </summary>
/// <typeparam name="T">The raw input type.</typeparam>
public interface ICustomDataFactory<T>
{
    Task<KeyValuePair<string, IReadOnlyList<object>>> CreateCustomDataAsync(T input, bool? isLoaded, CancellationToken cancellationToken);
}
