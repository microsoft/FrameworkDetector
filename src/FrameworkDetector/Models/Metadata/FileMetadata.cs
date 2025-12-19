// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

using System.Text.Json.Serialization;

namespace FrameworkDetector.Models;

/// <summary>
/// Information about a file used as part of data sources.
/// </summary>
/// <param name="Filename">The full path to the file on disk.</param>
/// <param name="IsLoaded">Specifies, if true, whether this file was discovered as loaded by a running process; otherwise, when flse, if it was only loose on disk in proximity to searched locations.</param>
public record FileMetadata([property: JsonIgnore()] string FullPath, // We don't want the full path in json but we do want it to de-dupe files
                           bool IsLoaded = false)
{
    /// <summary>
    /// Name of the file on disk.
    /// </summary>
    [JsonPropertyOrder(int.MinValue)] // Derived class or not, always put the filename first
    public string FileName => Path.GetFileName(FullPath);
}
