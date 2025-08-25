// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

public record CheckInfo<T>(
    string Name,
    string Description,
    Guid[] DataSourceIds,
    CheckFunction<T> PerformCheckAsync
) where T : struct
{
}

// TODO: Maybe have a helper/wrapper class around the datasource dictionary? i.e. have a helper which takes in the ids and returns the strongly typed datasource (or throws error if mismatch).
// TODO: The index of datasources should be a source generator for better type safety and performance.
public delegate Task<DetectorCheckResult> CheckFunction<T>(T info, Dictionary<Guid, IDataSource> dataSources, CancellationToken ct) where T : struct;