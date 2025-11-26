// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

//// This is extension/registration information about a check for its definition.

/// <summary>
/// Record of static registration of a CheckDefinition extension. Provides all the details the engine needs to provide in terms of identify within the check results, as well as the required data sources this check needs to operate. Finally, points to the check function the engine will call to perform the check.
/// </summary>
/// <typeparam name="TInput">Type of input arguments struct used by the check when running, e.g. the specific module to search for.</typeparam>
/// <typeparam name="TOutput">Type of output data struct for storing anyout output data from a check.</typeparam>
/// <param name="Name">The name of the check.</param>
/// <param name="Description">A short description of what the check does.</param>
/// <param name="DataSourceInterfaces">Expected data source interfaces</param>
/// <param name="PerformCheckAsync"><see cref="CheckFunction{TInput,TOutput}"/> delegate for signature of function called by the detector engine(tbd?) to perform the check against the provided data source.</param>
public record CheckRegistrationInfo<TInput,TOutput>(
    string Name,
    string Description,
    Type[] DataSourceInterfaces, // TODO: Not used currently, could use this later to auto-filter inputs passed to checks that only implement any of these interfaces...
    CheckFunction<TInput,TOutput> PerformCheckAsync
) where TInput : ICheckArgs
  where TOutput : struct
{
}

/// <summary>
/// The main execution source for a check extension. Called by the <see cref="DetectionEngine"/>.
/// <see cref="CheckDefinition{TInput,TOutput}.CheckArguments"/> can be retrieved for context provided by extension method on <see cref="IDetectorCheckGroup"/> for definition within an <see cref="IDetector"/>.
/// Match the required data source interfaces against the <see cref="IInputType"/> inputs to use for analysis.
/// Update the <see cref="DetectorCheckResult{TInput,TOutput}"/> with the status pass/fail/error (metadata is automatically attached).
/// </summary>
/// <typeparam name="TInput">Type of input arguments struct used by the check when running, e.g. the specific module to search for.</typeparam>
/// <typeparam name="TOutput">Type of output data struct for storing anyout output data from a check.</typeparam>
/// <param name="definition">The definition of the executing check.</param>
/// <param name="dataSources">The data sources the check has access to during execution.</param>
/// <param name="result">The check result to be set during execution.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns></returns>
public delegate Task CheckFunction<TInput,TOutput>(CheckDefinition<TInput,TOutput> definition, IReadOnlyList<IInputType> inputs, DetectorCheckResult<TInput,TOutput> result, CancellationToken cancellationToken) where TInput : ICheckArgs where TOutput : struct;