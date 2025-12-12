// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json.Serialization;

namespace FrameworkDetector.Models;

/// <summary>
/// Information about a file used as part of data sources.
/// </summary>
public record FileMetadata
{
    /// <summary>
    /// The full path to the file on disk.
    /// </summary>
    [JsonIgnore] // We don't want the full path in json but we do want it to de-dupe files
    public string FullPath { get; init; }

    /// <summary>
    /// InputGroup of the file on disk.
    /// </summary>
    public string Filename => Path.GetFileName(FullPath);

    /// <summary>
    /// Specifies, if true, whether this file was discovered as loaded by a running process; otherwise, when false, if it was only loose on disk in proximity to searched locations.
    /// </summary>
    public bool IsLoaded { get; init; }

    public FileMetadata(string fullPath, bool isLoaded = false)
    {
        FullPath = fullPath;
        IsLoaded = isLoaded;
    }
}
