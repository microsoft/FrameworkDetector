// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Inputs;

/// <summary>
/// A collection of <see cref="ICustomDataFactory{T}"/> factories for the raw input type T, with a helper function to create their custom data for a given type T.
/// </summary>
/// <typeparam name="T">The raw input type.</typeparam>
/// <param name="customDataFactories">The <see cref="ICustomDataFactory{T}"/> factories.</param>
public class CustomDataFactoryCollection<T>(IEnumerable<ICustomDataFactory<T>> customDataFactories)
{
    public async Task<IReadOnlyDictionary<string, IReadOnlyList<object>>> CreateCustomDataAsync(T input, bool? isLoaded, CancellationToken cancellationToken)
    {
        var customData = new Dictionary<string, List<object>>();

        foreach (var factory in customDataFactories)
        {
            var kvp = await factory.CreateCustomDataAsync(input, isLoaded, cancellationToken);
            if (!customData.TryGetValue(kvp.Key, out var list) || list is null)
            {
                list = new List<object>();
                customData[kvp.Key] = list;
            }
            list.AddRange(kvp.Value);
        }

        return customData.Select(kvp => new KeyValuePair<string, IReadOnlyList<object>>(kvp.Key, kvp.Value)).ToDictionary();
    }
}
