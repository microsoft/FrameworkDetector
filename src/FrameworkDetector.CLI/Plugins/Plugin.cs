// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Adapted from https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

public class Plugin
{
    internal Assembly Assembly { get; init; }

    public IReadOnlyList<IDetector> Detectors => _detectors;

    private List<IDetector> _detectors = new List<IDetector>();

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

        foreach (var type in assembly.GetTypes().Where(t => typeof(IDetector).IsAssignableFrom(t)))
        {
            if (Activator.CreateInstance(type) is IDetector detector)
            {
                plugin._detectors.Add(detector);
            }
        }

        return plugin;
    }
}
