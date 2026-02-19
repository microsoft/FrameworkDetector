// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> that represents a running process on the system, including its active windows and loaded modules.
/// </summary>
/// <param name="ProcessId">The process's ID (PID).</param>
/// <param name="MainModule">Metadata of the process' executable.</param>
/// <param name="ActiveWindows"><see cref="ActiveWindowMetadata"/> about Active Windows of the application.</param>
/// <param name="LoadedModules"><see cref="WindowsModuleMetadata"/> about the processes modules loaded in memory (more accurate/useful than <see cref="ExecutableInput"/>'s <see cref="ExecutableInput.ImportedModules">.</param>
/// <param name="CustomData">Custom data.</param>
/// <param name="MainWindowHandle">Handle ID to the MainWindow, if available.</param>
/// <param name="PackageFullName">The PackageFullName (PFN) of the process, if available.</param>
/// <param name="ApplicationUserModelId">The ApplicationUserModelId (AUMID) of the process, if available.</param>
public record ProcessInput(int ProcessId,
                           FileMetadata MainModule,
                           ActiveWindowMetadata[] ActiveWindows,
                           WindowsModuleMetadata[] LoadedModules,
                           IReadOnlyDictionary<string, IReadOnlyList<object>> CustomData,
                           long? MainWindowHandle = default, // IntPtr is long on 64-bit, int on 32-bit (so use long here)
                           string? PackageFullName = null,
                           string? ApplicationUserModelId = null)
    : IEquatable<ProcessInput>,
      IActiveWindowsDataSource,
      IModulesDataSource,
      ICustomDataSource,
      IInputTypeFactory<Process>,
      IInputType<Process>
{
    [JsonIgnore]
    public string InputGroup => "processes";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(Process process, bool? isLoaded, CustomDataFactoryCollection<Process>? customDataFactories, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get process filename (main module is null if we're attaching too soon)
        string filename = process.MainModule?.FileName ?? throw new ArgumentNullException(nameof(process.MainModule));

        process.TryGetPackageFullName(out var packageFullName);

        process.TryGetApplicationUserModelId(out var applicationUserModelId);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get Active Windows
        var activeWindows = process.GetActiveWindowMetadata();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get modules loaded in memory by the process.
        var loadedModules = process.GetLoadedModuleMetadata();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Load CustomData
        var customData = customDataFactories is not null ? await customDataFactories.CreateCustomDataAsync(process, isLoaded, cancellationToken) : new Dictionary<string, IReadOnlyList<object>>(0);

        return new ProcessInput(process.Id,
                                new FileMetadata(filename, IsLoaded: true),
                                activeWindows.OrderBy(aw => aw.ClassName).ToArray(),
                                loadedModules.OrderBy(pm => pm.FileName).ToArray(),
                                customData,
                                process.MainWindowHandle,
                                packageFullName,
                                applicationUserModelId);
    }

    public override int GetHashCode() => HashCode.Combine(MainModule, ProcessId);

    public virtual bool Equals(ProcessInput? input)
    {
        if (input is null)
        {
            return false;
        }

        return MainModule == input.MainModule && ProcessId == input.ProcessId;
    }

    public IEnumerable<ActiveWindowMetadata> GetActiveWindows() => ActiveWindows;

    public IEnumerable<WindowsModuleMetadata> GetModules() => LoadedModules;

    public IEnumerable<object> GetCustomData(string key) => CustomData.TryGetValue(key, out var values) ? values : Enumerable.Empty<object>();
}
