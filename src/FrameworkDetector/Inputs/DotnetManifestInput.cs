// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

public record DotnetManifestInput(FileMetadata FileMetadata,
                                  WindowsModuleMetadata[] RuntimeModules,
                                  WindowsModuleMetadata[] NativeModules,
                                  IReadOnlyDictionary<string, IReadOnlyList<object>> CustomData)
    : IEquatable<DotnetManifestInput>,
      IModulesDataSource,
      ICustomDataSource,
      IInputTypeFactory<FileInfo>,
      IInputType<FileInfo>
{
    [JsonIgnore]
    public string InputGroup => "dotnetManifests";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo manifest, bool? isLoaded, CustomDataFactoryCollection<FileInfo>? customDataFactories, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get manifests's own metadata
        var metadata = new FileMetadata(manifest.FullName, false);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Parse the manifest JSON
        var manifestJson = File.ReadAllText(manifest.FullName);
        var manifestJsonDoc = JsonDocument.Parse(manifestJson);

        // Get both  runtime and native modules
        string manifestJsonDir = Path.GetDirectoryName(manifest.FullName) ?? ".\\";
        var runtimeModules = GetModulesFromManifestJson(manifestJsonDoc, manifestJsonDir, ModuleType.Runtime);
        var nativeModules = GetModulesFromManifestJson(manifestJsonDoc, manifestJsonDir, ModuleType.Native);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Load CustomData
        var customData = customDataFactories is not null ? await customDataFactories.CreateCustomDataAsync(manifest, isLoaded, cancellationToken) : new Dictionary<string, IReadOnlyList<object>>(0);

        // No async initialization needed here yet, so just construct
        return new DotnetManifestInput(metadata,
                                   runtimeModules.OrderBy(m => m.FileName).ToArray(),
                                   nativeModules.OrderBy(m => m.FileName).ToArray(),
                                   customData);
    }

    protected static IReadOnlySet<WindowsModuleMetadata> GetModulesFromManifestJson(JsonDocument manifestJsonDoc, string manifestJsonDir, ModuleType moduleType)
    {
        HashSet<WindowsModuleMetadata> modules = new();

        // Loop over targets in .deps.json file to produce modules
        if (manifestJsonDoc.RootElement.TryGetProperty("targets", out var targets) && targets.ValueKind == JsonValueKind.Object)
        {
            var moduleTypeStr = moduleType.ToString().ToLowerInvariant();

            foreach (var target in targets.EnumerateObject())
            {
                if (target.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var item in target.Value.EnumerateObject())
                    {
                        if (item.Value.ValueKind == JsonValueKind.Object)
                        {
                            // Get dotnet modules from modulesObj
                            if (item.Value.TryGetProperty(moduleTypeStr, out var modulesObj) && modulesObj.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var moduleDep in modulesObj.EnumerateObject())
                                {
                                    var filename = Path.GetFileName(moduleDep.Name);
                                    var fileVersion = moduleDep.Value.ValueKind == JsonValueKind.Object && moduleDep.Value.TryGetProperty("fileVersion", out var fileVersionElement) ? fileVersionElement.ToString() : null;

                                    WindowsModuleMetadata? module = null;

                                    // Try to look for local file relative to manifest
                                    var localPath = Path.Combine(manifestJsonDir, filename);
                                    module = WindowsModuleMetadata.GetMetadata(localPath, false);

                                    // Fallback with just data from .deps.json
                                    module ??= new WindowsModuleMetadata(filename, FileVersion: fileVersion, IsLoaded: false);

                                    modules.Add(module);
                                }
                            }
                        }
                    }
                }
            }
        }

        return modules;
    }

    protected enum ModuleType
    {
        Runtime,
        Native
    }

    public IEnumerable<WindowsModuleMetadata> GetModules() => RuntimeModules.Union(NativeModules);

    public IEnumerable<object> GetCustomData(string key) => CustomData.TryGetValue(key, out var values) ? values : Enumerable.Empty<object>();
}
