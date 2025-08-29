// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;

namespace FrameworkDetector.Models;

public record WindowsBinaryMetadata(string Filename, 
                                    string? OriginalFilename, 
                                    string? FileVersion, 
                                    string? ProductName, 
                                    string? ProductVersion)
{    
    public static WindowsBinaryMetadata GetMetadata(Process process) => GetMetadata(process.MainModule?.FileName);

    public static WindowsBinaryMetadata GetMetadata(string? filename)
    {
        if (filename == null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return new WindowsBinaryMetadata(Path.GetFileName(fileVersionInfo.FileName), 
            fileVersionInfo.OriginalFilename, 
            fileVersionInfo.FileVersion, 
            fileVersionInfo.ProductName, 
            fileVersionInfo.ProductVersion);
    }
}
