// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;

using Microsoft.Extensions.DependencyInjection;

using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    private IServiceProvider Services => _services ??= ConfigureServices();
    private static IServiceProvider? _services = null;

    internal IServiceProvider ConfigureServices()
    {
        ServiceCollection services = ServiceInfo.GetDefaultServiceCollection(Plugins);

        // Add CLI-specific dependencyt injection here

        return services.BuildServiceProvider();
    }
}
