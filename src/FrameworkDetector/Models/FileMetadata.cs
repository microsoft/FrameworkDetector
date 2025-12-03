// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Models;

/// <summary>
/// Information about a file used as part of data sources.
/// </summary>
/// <param name="Filename">Name of the file on disk.</param>
/// <param name="IsLoaded">Specifies, if true, whether this file was discovered as loaded by a running process; otherwise, when flse, if it was only loose on disk in proximity to searched locations.</param>
public record FileMetadata(string Filename, 
                           bool IsLoaded = false)
{
    public static async Task<FileMetadata?> GetMetadataAsync(string filename, bool isLoaded, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentNullException(nameof(filename));
        }

        await Task.Yield();

        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        return new FileMetadata(Path.GetFileName(filename), isLoaded);
    }
}
