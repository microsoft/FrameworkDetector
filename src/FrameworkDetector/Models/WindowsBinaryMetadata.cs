// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Models;

public record WindowsBinaryMetadata(string Filename, 
                                    string? OriginalFilename = null, 
                                    string? FileVersion = null, 
                                    string? ProductName = null, 
                                    string? ProductVersion = null) : FileMetadata(Filename)
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
            return null;
        }

        if (!Path.Exists(filename))
        {
            // Try to see if we're looking for a binary under a redirected path, i.e. C:\Windows\System32 but really it's under C:\Windows\SysWOW64
            if (TryFindRedirectedFile(filename,Environment.SpecialFolder.System, Environment.SpecialFolder.SystemX86, out var syswow64Path) && syswow64Path is not null)
            {
                filename = syswow64Path;
            }
            else if (TryFindRedirectedFile(filename, Environment.SpecialFolder.CommonProgramFiles, Environment.SpecialFolder.CommonProgramFilesX86, out var commonProgramFilesX86Path) && commonProgramFilesX86Path is not null)
            {
                filename = commonProgramFilesX86Path;
            }
            else if (TryFindRedirectedFile(filename, Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86, out var programFilesX86Path) && programFilesX86Path is not null)
            {
                filename = programFilesX86Path;
            }
            else
            {
                // Give up, just return the filename since we can't find the actual file on disk
                return new WindowsBinaryMetadata(Path.GetFileName(filename));
            }
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return new WindowsBinaryMetadata(Path.GetFileName(fileVersionInfo.FileName), 
            fileVersionInfo.OriginalFilename, 
            fileVersionInfo.FileVersion, 
            fileVersionInfo.ProductName, 
            fileVersionInfo.ProductVersion);
    }

    private static bool TryFindRedirectedFile(string filename, Environment.SpecialFolder fromRoot, Environment.SpecialFolder toRoot, out string? newFilename)
    {
        return TryFindRedirectedFile(filename, Environment.GetFolderPath(fromRoot), Environment.GetFolderPath(toRoot), out newFilename);
    }

    private static bool TryFindRedirectedFile(string filename, string fromRoot, string toRoot, out string? newFilename)
    {
        var oldPath = filename;
        if (oldPath.StartsWith(fromRoot, StringComparison.InvariantCultureIgnoreCase))
        {
            var newPath = Path.Join(toRoot, Path.GetRelativePath(fromRoot, oldPath));
            if (File.Exists(newPath))
            {
                newFilename = newPath;
                return true;
            }
        }

        newFilename = default;
        return false;
    }
}
