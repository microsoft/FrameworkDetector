// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Models;

public record WindowsBinaryMetadata(string Filename, 
                                    string? OriginalFilename, 
                                    string? FileVersion, 
                                    string? ProductName, 
                                    string? ProductVersion) : FileMetadata(Filename)
{
    public static new async Task<WindowsBinaryMetadata?> GetMetadataAsync(string? filename, CancellationToken cancellationToken)
    {
        if (filename is null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        await Task.Yield();

        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<WindowsBinaryMetadata?>(cancellationToken);
        }

        if (!Path.Exists(filename))
        {
            // Try to see if we're looking for a binary under C:\Windows\System32 but really it's under C:\Windows\SysWOW64
            var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var syswow64 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);

            var oldPath = filename;
            if (oldPath.StartsWith(system32, StringComparison.InvariantCultureIgnoreCase))
            {
                var newPath = Path.Join(syswow64, Path.GetRelativePath(system32, oldPath));
                if (File.Exists(newPath))
                {
                    filename = newPath;
                }
            }
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return new WindowsBinaryMetadata(Path.GetFileName(fileVersionInfo.FileName), 
            fileVersionInfo.OriginalFilename, 
            fileVersionInfo.FileVersion, 
            fileVersionInfo.ProductName, 
            fileVersionInfo.ProductVersion);
    }
}
