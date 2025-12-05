// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Helper methods for taking an input type (Process, Packaged App, etc...) and generating a list of inputs available from that input.
/// </summary>
public static class InputHelper
{
    public static async Task<IReadOnlyList<IInputType>> GetInputsFromExecutableAsync(FileInfo fileInfo, bool isLoaded, CancellationToken cancellationToken)
    {
        // TODO: Any other inputs this can provide? Can we get package from exe?
        // Need to be careful about loops though, maybe these should all be independent?

        return [await ExecutableInput.CreateAndInitializeDataSourcesAsync(fileInfo, isLoaded, cancellationToken)];
    }

    public static async Task<IReadOnlyList<IInputType>> GetInputsFromPackageAsync(Package package, bool isLoaded, CancellationToken cancellationToken)
    {
        // Get Main Package Info
        var path = package.InstalledLocation.Path;

        var manifest = Path.Combine(path, "AppxManifest.xml");
        if (File.Exists(manifest))
        {
            // TODO: Does this work?
            // Read manifest and extract relevant info for manifest data source (shared with MSIX)...
        }

        return [await InstalledPackageInput.CreateAndInitializeDataSourcesAsync(package, isLoaded, cancellationToken)];
    }

    public static async Task<IReadOnlyList<IInputType>> GetInputsFromProcessAsync(Process process, bool includeChildProcesses, CancellationToken cancellationToken)
    {
        List<IInputType> inputs = [];

        // Get Main Process Info
        inputs.Add(await ProcessInput.CreateAndInitializeDataSourcesAsync(process, true, cancellationToken));

        // Get Child process info
        if (includeChildProcesses)
        {
            foreach (var child in process.GetChildProcesses())
            {
                inputs.Add(await ProcessInput.CreateAndInitializeDataSourcesAsync(child, true, cancellationToken));
            }
        }

        // Get Installed Packaged App Info
        if (await process.GetPackageFromProcess() is Package package)
        {
            inputs.AddRange(await GetInputsFromPackageAsync(package, true, cancellationToken));
        }

        // Get Executable Binary Info
        FileInfo? mainModuleFileInfo = process.GetMainModuleFileInfo();
        if (mainModuleFileInfo?.Exists == true)
        {
            inputs.AddRange(await GetInputsFromExecutableAsync(mainModuleFileInfo, true, cancellationToken));
        }

        return inputs;
    }
}
