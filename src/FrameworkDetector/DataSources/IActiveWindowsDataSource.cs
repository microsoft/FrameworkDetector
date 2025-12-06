// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Provides metadata about the currently active windows for a given process.
/// </summary>
public interface IActiveWindowsDataSource : IDataSource
{
    ActiveWindowMetadata[] ActiveWindows { get; }
}
