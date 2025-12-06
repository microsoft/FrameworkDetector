// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Interface definining a common static helper factory method for creating and initializing an <see cref="IInputType"/> implementation from a raw type T.
/// </summary>
/// <typeparam name="T">Raw type wrapped at providing data to data source interfaces implemented by the implementation.</typeparam>
/// <remarks>
/// Any class implementing <see cref="IInputType"/> should also implement this interface to provide a common factory method for creating and initializing the input type from the raw type T.
/// </remarks>
public interface IInputTypeFactory<T>
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
