// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Plugins;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    private IServiceProvider Services => _services ?? throw new Exception($"{nameof(Services)} not initialized. Did you forget to call {nameof(TryInitializeFrameworkDetectorServices)}?");
    private static IServiceProvider? _services = null;

    internal bool TryInitializeFrameworkDetectorServices(ParseResult parseResult)
    {

        try
        {
            if (!TryParsePluginFiles(parseResult, out var plugins))
            {
                PrintError("Invalid plugin file specified.");
            }

            var services = ServiceInfo.GetDefaultServiceCollection(plugins);

            // Add CLI-specific dependency injection here

            _services = services.BuildServiceProvider();
            return true;
        }
        catch (Exception ex)
        {
            PrintException(ex);
        }
        return false;
    }
}
