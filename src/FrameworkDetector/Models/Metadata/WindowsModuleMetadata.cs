// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;

namespace FrameworkDetector.Models;

public record WindowsModuleMetadata(string FileName,
                                    string? OriginalFileName = null, 
                                    string? FileVersion = null, 
                                    string? ProductName = null, 
                                    string? ProductVersion = null,
                                    bool IsLoaded = false) : FileMetadata(FileName, IsLoaded)
{
    public static WindowsModuleMetadata GetMetadata(string filename, bool isLoaded)
    {
        if (!Path.Exists(filename))
        {
            // Try to see if we're looking for a binary under a redirected path, i.e. C:\Windows\System32 but really it's under C:\Windows\SysWOW64
            if (Path.TryFindRedirectedFile(filename, Environment.SpecialFolder.System, Environment.SpecialFolder.SystemX86, out var syswow64Path) && syswow64Path is not null)
            {
                filename = syswow64Path;
            }
            else if (Path.TryFindRedirectedFile(filename, Environment.SpecialFolder.CommonProgramFiles, Environment.SpecialFolder.CommonProgramFilesX86, out var commonProgramFilesX86Path) && commonProgramFilesX86Path is not null)
            {
                filename = commonProgramFilesX86Path;
            }
            else if (Path.TryFindRedirectedFile(filename, Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86, out var programFilesX86Path) && programFilesX86Path is not null)
            {
                filename = programFilesX86Path;
            }
            else
            {
                // Give up, just return the filename since we can't find the actual file on disk
                return new WindowsModuleMetadata(filename, IsLoaded: isLoaded);
            }
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return new WindowsModuleMetadata(fileVersionInfo.FileName, 
            fileVersionInfo.OriginalFilename, 
            fileVersionInfo.FileVersion, 
            fileVersionInfo.ProductName, 
            fileVersionInfo.ProductVersion,
            isLoaded);
    }
}
