// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace FrameworkDetector.Checks;

public record CheckDefinition<T>(
    CheckInfo<T> Info,
    T InfoData
) where T : struct
{
    public string Name => Info.Name;

    public string Description => Info.Description;

    public Guid[] DataSourceIds => Info.DataSourceIds;

    public CheckFunction<T> PerformCheckAsync => Info.PerformCheckAsync;
}