// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Adapted from https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support

using System;
using System.Reflection;
using System.Runtime.Loader;

namespace FrameworkDetector.Plugins;

/// <summary>
/// PluginLoadContext enables <see cref="Plugin"/> assemblies to load their own versions dependencies independent of what other Plugins load.
/// </summary>
/// <param name="pluginPath">Path to the plugin assembly.</param>
public class PluginLoadContext(string pluginPath) : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver = new AssemblyDependencyResolver(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Always resolve the loaded FrameworkDetector assembly
        if (assemblyName.Name == AssemblyInfo.LibraryName)
        {
            return AssemblyInfo.LibraryAssembly;
        }

        // Resolve any other assemblies to the plugin's folder
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath is not null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
