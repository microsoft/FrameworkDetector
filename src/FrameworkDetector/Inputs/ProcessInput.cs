// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

public record ProcessInput(string Filename,
                           ProcessWindowMetadata[] ActiveWindows,
                           WindowsBinaryMetadata[] Modules,
                           string? OriginalFilename = null,
                           string? FileVersion = null,
                           string? ProductName = null,
                           string? ProductVersion = null,
                           int? ProcessId = null,
                           long? MainWindowHandle = default, // IntPtr is long on 64-bit, int on 32-bit (so use long here)
                           string? MainWindowTitle = null,
                           string? PackageFullName = null,
                           string? ApplicationUserModelId = null)
    : WindowsBinaryMetadata(Filename, OriginalFilename, FileVersion, ProductName, ProductVersion),
      IActiveWindowsDataSource, IModulesDataSource, 
      IInputType<Process>
{
    public string Name => "processes";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(Process process, CancellationToken cancellationToken)
    {
        // Get Process Metadata
        var fileVersionInfo = process.MainModule?.FileVersionInfo ?? throw new ArgumentNullException(nameof(process.MainModule)); // TODO: Does any of this belong on ExecutableInput?

        process.TryGetPackageFullName(out var packageFullName);

        process.TryGetApplicationUserModelId(out var applicationUserModelId);

        // Get Active Windows
        var activeWindows = process.GetActiveWindowMetadata();

        // Get modules loaded in memory by the process.
        var loadedModules = new HashSet<WindowsBinaryMetadata>();
        foreach (var module in process.Modules.Cast<ProcessModule>())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // TODO: Figure out how we want to handle cancellation here in the API contract with the return type...
                return null;
            }

            var moduleMetadata = await WindowsBinaryMetadata.GetMetadataAsync(module.FileName, isLoaded: true, cancellationToken);
            if (moduleMetadata is not null)
            {
                loadedModules.Add(moduleMetadata);
            }
        }

        return new ProcessInput(Path.GetFileName(fileVersionInfo.FileName),
                                // Data Sources First so we don't have to make them null.
                                activeWindows.OrderBy(aw => aw.ClassName ?? "").ToArray(),
                                loadedModules.OrderBy(pm => pm.Filename).ToArray(),
                                // Extra Metadata
                                fileVersionInfo.OriginalFilename,
                                fileVersionInfo.FileVersion,
                                fileVersionInfo.ProductName,
                                fileVersionInfo.ProductVersion,
                                process.Id,
                                process.MainWindowHandle,
                                process.MainWindowTitle,
                                packageFullName,
                                applicationUserModelId);
    }
}
