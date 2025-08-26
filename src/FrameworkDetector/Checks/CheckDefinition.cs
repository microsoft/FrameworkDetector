// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using System;

namespace FrameworkDetector.Checks;

public interface ICheckDefinition
{
    public Guid[] DataSourceIds { get; }

    public string Description { get; }
    public string Name { get; }
}

public record CheckDefinition<T>(
    CheckInfo<T> Info,
    T InfoData
) : ICheckDefinition where T : struct
{
    public string Name => Info.Name;

    public string Description => Info.Description;

    public Guid[] DataSourceIds => Info.DataSourceIds;

    public CheckFunction<T> PerformCheckAsync => Info.PerformCheckAsync;

    // TODO: IsRequired and Result here too? Or do we aggregate results separately?
}