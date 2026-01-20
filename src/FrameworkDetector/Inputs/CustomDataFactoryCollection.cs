// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
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
        var customData = new Dictionary<string, IReadOnlyList<object>>();

        foreach (var factory in customDataFactories)
        {
            await foreach (var result in factory.CreateCustomDataAsync(input, isLoaded, cancellationToken))
            {
                if (result is not null)
                {
                    if (!customData.TryGetValue(factory.Key, out var list) || list is null)
                    {
                        // Create aggregate list for Key if it doesn't exist
                        list = new List<object>();
                        customData[factory.Key] = list;
                    }
                    (list as List<object>)!.AddRange(result);
                }
            }
        }

        return customData;
    }
}
