// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel;

using Microsoft.Extensions.DependencyInjection;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Factory with methods for taking an input type (Process, Packaged App, etc...) and building a list of inputs available from that input.
/// </summary>
public class InputFactory(IServiceProvider services)
{
    public async Task<IReadOnlyList<IInputType>> GetInputsFromExecutableAsync(FileInfo fileInfo, bool isLoaded, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        List<IInputType> inputs = [];

        var exeInput = await ExecutableInput.CreateAndInitializeDataSourcesAsync(fileInfo, isLoaded, services.GetRequiredService<CustomDataFactoryCollection<FileInfo>>(), cancellationToken);
        if (exeInput is not null)
        {
            inputs.Add(exeInput);
        }

        var dotnetManifestFile = new FileInfo(Path.ChangeExtension(fileInfo.FullName, ".deps.json"));
        if (dotnetManifestFile.Exists)
        {
            var dotnetManifestInput = await DotnetManifestInput.CreateAndInitializeDataSourcesAsync(dotnetManifestFile, isLoaded, services.GetRequiredService<CustomDataFactoryCollection<FileInfo>>(), cancellationToken);
            if (dotnetManifestInput is not null)
            {
                inputs.Add(dotnetManifestInput);
            }
        }

        // TODO: Any other inputs this can provide? Can we get package from exe?
        // Need to be careful about loops though, maybe these should all be independent?

        return inputs;
    }

    public async Task<IReadOnlyList<IInputType>> GetInputsFromPackageAsync(Package package, bool isLoaded, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        List<IInputType> inputs = [];

        var pkgInput = await InstalledPackageInput.CreateAndInitializeDataSourcesAsync(package, isLoaded, services.GetRequiredService<CustomDataFactoryCollection<Package>>(), cancellationToken);
        if (pkgInput is not null)
        {
            inputs.Add(pkgInput);
        }

        try
        {
            // Get Main Package Info
            var path = package.InstalledLocation.Path;

            var manifest = Path.Combine(path, "AppxManifest.xml");
            if (File.Exists(manifest))
            {
                // TODO: Does this work?
                // Read manifest and extract relevant info for manifest data source (shared with MSIX)...
            }
        }
        catch { }

        return inputs;
    }

    public async Task<IReadOnlyList<IInputType>> GetInputsFromProcessAsync(Process process, bool includeChildProcesses, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        List<IInputType> inputs = [];

        // Get Main Process Info
        var processInput = await ProcessInput.CreateAndInitializeDataSourcesAsync(process, true, services.GetRequiredService<CustomDataFactoryCollection<Process>>(), cancellationToken);
        if (processInput is not null)
        {
            inputs.Add(processInput);
        }

        // Get Main Executable Binary Info
        FileInfo? mainModuleFileInfo = process.GetMainModuleFileInfo();
        if (mainModuleFileInfo?.Exists == true)
        {
            var exeInputs = await GetInputsFromExecutableAsync(mainModuleFileInfo, true, cancellationToken);
            foreach (var exeInput in exeInputs)
            {
                inputs.Add(exeInput);
            }
        }

        // Get Installed Packaged App Info
        if (process.TryGetPackageFromProcess(out var package) && package is not null)
        {
            var packageInputs = await GetInputsFromPackageAsync(package, true, cancellationToken);
            foreach (var packageInput in packageInputs)
            {
                inputs.Add(packageInput);
            }
        }

        // Get Child process info
        if (includeChildProcesses)
        {
            foreach (var child in process.GetChildProcesses())
            {
                var childInputs = await GetInputsFromProcessAsync(child, false, cancellationToken);
                foreach (var childInput in childInputs)
                {
                    // For apps that call themselves, make sure we're not getting unecessary inputs
                    if (!inputs.Contains(childInput))
                    {
                        inputs.Add(childInput);
                    }
                }
            }
        }

        return inputs;
    }
}
