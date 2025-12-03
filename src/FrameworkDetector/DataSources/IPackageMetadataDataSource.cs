// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

/// <summary>
/// Data source interface for information from msix based package metadata, usually retrieved from an installed application package.
/// </summary>
public interface IPackageMetadataDataSource : IDataSource
{
    PackageMetadata PackageMetadata { get; }
}