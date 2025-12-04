// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Inputs wrap and provide Data Sources for the type they wrap only. It may be possible for an input source to reach other types of inputs,
/// but that is handled by the parent application to create multiple base inputs as needed. Data Sources from the input type should be what is
/// only accessible solely from that input type.
/// </summary>
/// <typeparam name="T">Raw type wrapped at providing data to data source interfaces implemented by the implementation.</typeparam>
public interface IInputType<T> : IInputType
{
    /// <summary>
    /// Static factory initialization method used to initialize this input and gather all data sources it can provide.
    /// </summary>
    /// <param name="input">Type of data used to initialize data sources within this input</param>
    /// <param name="isLoaded">Specifies, if known (not null), whether or not the input was loaded in memory when processed.</param>
    /// <param name="cancellationToken">A cancellation token to disrupt initialization.</param>
    /// <returns>An <see cref="IInputType"/> of the provided input with all data sources initialized.</returns>
    /// <exception cref="NotImplementedException">By default this exception is thrown if the implementor did not implement this method as required.</exception>
    public virtual static Task<IInputType> CreateAndInitializeDataSourcesAsync(T input, bool? isLoaded, CancellationToken cancellationToken) => throw new NotImplementedException("Input Type did not implement CreateAsync method.");
}
