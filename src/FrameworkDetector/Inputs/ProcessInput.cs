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

/// <summary>
/// An <see cref="IInputType"/> that represents a running process on the system, including its active windows and loaded modules.
/// </summary>
/// <param name="MainModule">Name of the process' executable.</param>
/// <param name="ActiveWindows"><see cref="ActiveWindowMetadata"/> about Active Windows of the application.</param>
/// <param name="Modules"><see cref="WindowsModuleMetadata"/> about the processes modules loaded in memory (more accurate than <see cref="ExecutableInput"/>'s Modules, TODO: Link directly to that property when we add it</param>
/// <param name="ProcessId"></param>
/// <param name="MainWindowHandle"></param>
/// <param name="MainWindowTitle"></param>
/// <param name="PackageFullName"></param>
/// <param name="ApplicationUserModelId"></param>
public record ProcessInput(FileMetadata MainModule,
                           ActiveWindowMetadata[] ActiveWindows,
                           WindowsModuleMetadata[] Modules,
                           int? ProcessId = null,
                           long? MainWindowHandle = default, // IntPtr is long on 64-bit, int on 32-bit (so use long here)
                           string? MainWindowTitle = null,
                           string? PackageFullName = null,
                           string? ApplicationUserModelId = null)
    : IActiveWindowsDataSource,
      IModulesDataSource, 
      IInputTypeFactory<Process>,
      IInputType
{
    public string Name => "processes";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(Process process, bool? isLoaded, CancellationToken cancellationToken)
    {
        // Get Process Metadata
        var fileVersionInfo = process.MainModule?.FileVersionInfo ?? throw new ArgumentNullException(nameof(process.MainModule));

        process.TryGetPackageFullName(out var packageFullName);

        process.TryGetApplicationUserModelId(out var applicationUserModelId);

        // Get Active Windows
        var activeWindows = process.GetActiveWindowMetadata();

        // Get modules loaded in memory by the process.
        var loadedModules = new HashSet<WindowsModuleMetadata>();
        foreach (var module in process.Modules.Cast<ProcessModule>())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // TODO: Figure out how we want to handle cancellation here in the API contract with the return type...
                return null;
            }

            var moduleMetadata = WindowsModuleMetadata.GetMetadata(module.FileName, isLoaded: true);
            if (moduleMetadata is not null)
            {
                loadedModules.Add(moduleMetadata);
            }
        }

        return new ProcessInput(new FileMetadata(fileVersionInfo.FileName, IsLoaded: true),
                                // Data Sources First so we don't have to make them null.
                                activeWindows.OrderBy(aw => aw.ClassName ?? "").ToArray(),
                                loadedModules.OrderBy(pm => pm.Filename).ToArray(),
                                process.Id,
                                process.MainWindowHandle,
                                process.MainWindowTitle,
                                packageFullName,
                                applicationUserModelId);
    }
}
