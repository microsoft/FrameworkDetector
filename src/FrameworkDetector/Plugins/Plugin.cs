// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Adapted from https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel;

using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;

namespace FrameworkDetector.Plugins;

/// <summary>
/// Plugin enables runtime extensibility by library consumers by enabling them to specify <see cref="IDetector" />s and/or <see cref="ICustomDataFactory{T}" /> classes in their own <see cref="Assembly"/>.
/// </summary>
public class Plugin
{
    internal Assembly Assembly { get; init; }

    public IReadOnlyList<IDetector> Detectors => _detectors;
    private List<IDetector> _detectors = new List<IDetector>();

    public IReadOnlyList<ICustomDataFactory<FileInfo>> FileInfoCustomDataFactories => _fileInfoCustomDataFactories;
    private List<ICustomDataFactory<FileInfo>> _fileInfoCustomDataFactories = new List<ICustomDataFactory<FileInfo>>();

    public IReadOnlyList<ICustomDataFactory<Package>> PackageCustomDataFactories => _packageCustomDataFactories;
    private List<ICustomDataFactory<Package>> _packageCustomDataFactories = new List<ICustomDataFactory<Package>>();

    public IReadOnlyList<ICustomDataFactory<Process>> ProcessCustomDataFactories => _processCustomDataFactories;
    private List<ICustomDataFactory<Process>> _processCustomDataFactories = new List<ICustomDataFactory<Process>>();

    private Plugin(Assembly assembly)
    {
        Assembly = assembly;
    }

    public static Plugin LoadPluginFromPath(string path)
    {
        string fullPath = Path.GetFullPath(path);

        string pluginLocation = fullPath.Replace('\\', Path.DirectorySeparatorChar);

        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));

        return LoadPluginFromAssembly(assembly);
    }

    internal static Plugin LoadPluginFromAssembly(Assembly assembly)
    {
        var plugin = new Plugin(assembly);

        foreach (var type in assembly.GetTypes())
        {
            if (typeof(IDetector).IsAssignableFrom(type) && Activator.CreateInstance(type) is IDetector detector)
            {
                plugin._detectors.Add(detector);
            }
            else if (typeof(ICustomDataFactory<FileInfo>).IsAssignableFrom(type) && Activator.CreateInstance(type) is ICustomDataFactory<FileInfo> fileInfoCustomDataFactory)
            {
                plugin._fileInfoCustomDataFactories.Add(fileInfoCustomDataFactory);
            }
            else if (typeof(ICustomDataFactory<Package>).IsAssignableFrom(type) && Activator.CreateInstance(type) is ICustomDataFactory<Package> packageCustomDataFactory)
            {
                plugin._packageCustomDataFactories.Add(packageCustomDataFactory);
            }
            else if (typeof(ICustomDataFactory<Process>).IsAssignableFrom(type) && Activator.CreateInstance(type) is ICustomDataFactory<Process> processCustomDataFactory)
            {
                plugin._processCustomDataFactories.Add(processCustomDataFactory);
            }
        }

        return plugin;
    }
}
