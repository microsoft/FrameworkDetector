// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Models;

public record ProcessMetadata(string Filename,
                              string? OriginalFilename,
                              string? FileVersion,
                              string? ProductName,
                              string? ProductVersion,
                              int? ProcessId,
                              WindowsBinaryMetadata[]? LoadedModules,
                              ProcessWindowMetadata[]? ActiveWindows) : WindowsBinaryMetadata(Filename, OriginalFilename, FileVersion, ProductName, ProductVersion)
{

    public static async Task<ProcessMetadata?> GetMetadataAsync(Process process, CancellationToken cancellationToken)
    {
        await Task.Yield();

        var fileVersionInfo = process.MainModule?.FileVersionInfo ?? throw new ArgumentNullException(nameof(process.MainModule));

        var loadedModules = new HashSet<WindowsBinaryMetadata>();
        foreach (var module in process.Modules.Cast<ProcessModule>())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var moduleMetadata = await GetMetadataAsync(module.FileName, cancellationToken);
            if (moduleMetadata is not null)
            {
                loadedModules.Add(moduleMetadata);
            }
        }

        var activeWindows = process.GetActiveWindowMetadata();

        return new ProcessMetadata(Path.GetFileName(fileVersionInfo.FileName),
                                   fileVersionInfo.OriginalFilename,
                                   fileVersionInfo.FileVersion,
                                   fileVersionInfo.ProductName,
                                   fileVersionInfo.ProductVersion,
                                   process.Id,
                                   loadedModules.OrderBy(pm => pm.Filename).ToArray(),
                                   activeWindows.OrderBy(aw => aw.ClassName ?? "").ToArray());
    }
}
