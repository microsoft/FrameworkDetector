// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Models;

public record FileMetadata(string Filename)
{
    public static async Task<FileMetadata?> GetMetadataAsync(string? filename, CancellationToken cancellationToken)
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

        return new FileMetadata(Path.GetFileName(filename));
    }
}
