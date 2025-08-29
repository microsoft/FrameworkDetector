// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

//// This is extension/registration information about a check for its definition.

/// <summary>
/// Record of static registration of a Check extension. Provides all the details the engine needs to provide in terms of identify within the check results, as well as the required data sources this check needs to operate. Finally, points to the check function the engine will call to perform the check.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="DataSourceIds"><see cref="IDataSource.Id"/> static ids to identify the required data source info needed to perform this check (TODO: this probably should be a source generated registry in the future maybe?)</param>
/// <param name="PerformCheckAsync"><see cref="CheckFunction{T}"/> delegate for signature of function called by the detector engine(tbd?) to perform the check against the provided data source.</param>
public record CheckRegistrationInfo<T>(
    string Name,
    string Description,
    Guid[] DataSourceIds,
    CheckFunction<T> PerformCheckAsync
) where T : struct
{
}

// TODO: The index of datasources should be a source generator for better type safety and performance.

/// <summary>
/// The main execution source for a check extension. Called by the <see cref="DetectionEngine"/>.
/// <see cref="CheckDefinition{T}.Metadata"/> can be retrieved for context provided by extension method on <see cref="DetectorCheckGroup"/> for definition within an <see cref="IDetector"/>.
/// Lookup the required data in <see cref="DataSourceCollection"/> to match against the metadata.
/// Update the <see cref="DetectorCheckResult{T}"/> with the status pass/fail/error (metadata is automatically attached).
/// </summary>
/// <typeparam name="T"><see cref="CheckDefinition{T}.Metadata"/></typeparam>
/// <param name="definition"></param>
/// <param name="dataSources"></param>
/// <param name="result"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task CheckFunction<T>(CheckDefinition<T> definition, DataSourceCollection dataSources, DetectorCheckResult<T> result, CancellationToken cancellationToken) where T : struct;