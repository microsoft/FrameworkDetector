// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using System.CommandLine;

using FrameworkDetector.Plugins;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    internal IReadOnlyList<Plugin> Plugins => _plugins;

    internal List<Plugin> _plugins = new List<Plugin>();

    internal bool TryInitializePlugins(ParseResult parseResult)
    {
        if (!TryParsePluginFiles(parseResult))
        {
            PrintError("Invalid plugin file specified");
            return false;
        }

        bool loaded = true;

        foreach (var pluginFile in PluginFiles)
        {
            loaded = loaded && TryLoadPlugin(pluginFile);
        }

        return loaded;
    }

    internal bool TryLoadPlugin(string pluginFile)
    {
        try
        {
            PrintInfo("Loading plugin \"{0}\"...", Path.GetFileNameWithoutExtension(pluginFile));
            var plugin = Plugin.LoadPluginFromPath(pluginFile);

            _plugins.Add(plugin);

            return true;
        }
        catch (Exception ex)
        {
            PrintException(ex);
        }

        return false;
    }
}
