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
                              string? OriginalFilename = null,
                              string? FileVersion = null,
                              string? ProductName = null,
                              string? ProductVersion = null,
                              int? ProcessId = null,
                              long? MainWindowHandle = default, // IntPtr is long on 64-bit, int on 32-bit (so use long here)
                              string? MainWindowTitle = null,
                              string? PackageFullName = null,
                              string? ApplicationUserModelId = null,
                              WindowsBinaryMetadata[]? LoadedModules = null,
                              ProcessWindowMetadata[]? ActiveWindows = null,
                              ProcessImportedFunctionsMetadata[]? ImportedFunctions = null,
                              ProcessExportedFunctionsMetadata[]? ExportedFunctions = null,
                              ProcessPackagedAppMetadata? AppPackageMetadata = null) 
    : WindowsBinaryMetadata(Filename, OriginalFilename, FileVersion, ProductName, ProductVersion)
{

    public static async Task<ProcessMetadata?> GetMetadataAsync(Process process, CancellationToken cancellationToken)
    {
        await Task.Yield();

        var fileVersionInfo = process.MainModule?.FileVersionInfo ?? throw new ArgumentNullException(nameof(process.MainModule));

        process.TryGetPackageFullName(out var packageFullName);

        process.TryGetApplicationUserModelId(out var applicationUserModelId);

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

        var importedFunctions = process.ProcessImportedFunctionsMetadata();

        var exportedFunctions = process.ProcessExportedFunctionsMetadata();

        var packageInfo = await process.ProcessPackageMetadataAsync();

        return new ProcessMetadata(Path.GetFileName(fileVersionInfo.FileName),
                                   fileVersionInfo.OriginalFilename,
                                   fileVersionInfo.FileVersion,
                                   fileVersionInfo.ProductName,
                                   fileVersionInfo.ProductVersion,
                                   process.Id,
                                   process.MainWindowHandle,
                                   process.MainWindowTitle,
                                   packageFullName,
                                   applicationUserModelId,
                                   loadedModules.OrderBy(pm => pm.Filename).ToArray(),
                                   activeWindows.OrderBy(aw => aw.ClassName ?? "").ToArray(),
                                   importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray(),
                                   packageInfo);
    }
}
