// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

public interface IModulesDataSource : IDataSource
{
    // TODO: Do we want IReadOnlyList here too, or do we want to ensure baked by IInputType ahead of time?
    WindowsModuleMetadata[] Modules { get; }
}
