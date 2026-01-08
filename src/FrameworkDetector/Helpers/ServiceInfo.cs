// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;

using Microsoft.Extensions.DependencyInjection;

using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Plugins;

namespace FrameworkDetector;

/// <summary>
/// Dependency Injection helpers.
/// </summary>
public static class ServiceInfo
{
    /// <summary>
    /// Gets the default <see cref="ServiceCollection"/> with all types (particularly <see cref="IDetector"/>s) registered.
    /// </summary>
    /// <param name="plugins">Optional plugins to be registered in the collection.</param>
    /// <returns>The default <see cref="ServiceCollection"/>.</returns>
    public static ServiceCollection GetDefaultServiceCollection(IEnumerable<Plugin>? plugins = null)
    {
        ServiceCollection services = new();

        // TODO: Add a Logger here that we can use to report issues or record debug info, etc...

        // TODO: Would be nice if the SG could do this for us, there's a request open: https://github.com/CommunityToolkit/Labs-Windows/discussions/463#discussioncomment-11720493

        // ---- ADD DETECTORS HERE ----
        services.AddSingleton<IDetector, AvaloniaDetector>();
        services.AddSingleton<IDetector, CEFDetector>();
        services.AddSingleton<IDetector, ComCtlDetector>();
        services.AddSingleton<IDetector, DirectXDetector>();
        services.AddSingleton<IDetector, DotNetDetector>();
        services.AddSingleton<IDetector, DotNetCoreDetector>();
        services.AddSingleton<IDetector, DotNetFrameworkDetector>();
        services.AddSingleton<IDetector, DotNetNativeDetector>();
        services.AddSingleton<IDetector, ElectronDetector>();
        services.AddSingleton<IDetector, MVVMToolkitDetector>();
        services.AddSingleton<IDetector, QtDetector>();
        services.AddSingleton<IDetector, RNWDetector>();
        services.AddSingleton<IDetector, WCTDetector>();
        services.AddSingleton<IDetector, WebView1Detector>();
        services.AddSingleton<IDetector, WebView2Detector>();
        services.AddSingleton<IDetector, WinAppSDKDetector>();
        services.AddSingleton<IDetector, WinFormsDetector>();
        services.AddSingleton<IDetector, Win2DDetector>();
        services.AddSingleton<IDetector, WinUIDetector>();
        services.AddSingleton<IDetector, WinUI2Detector>();
        services.AddSingleton<IDetector, WinUI3Detector>();
        services.AddSingleton<IDetector, WPFDetector>();

        if (plugins is not null)
        {
            foreach (var plugin in plugins)
            {
                // Load detectors
                foreach (var detector in plugin.Detectors)
                {
                    services.AddSingleton(typeof(IDetector), detector.GetType());
                }

                // Load custom data factories
                foreach (var factory in plugin.FileInfoCustomDataFactories)
                {
                    services.AddSingleton(typeof(ICustomDataFactory<FileInfo>), factory);
                }
                foreach (var factory in plugin.PackageCustomDataFactories)
                {
                    services.AddSingleton(typeof(ICustomDataFactory<Package>), factory);
                }
                foreach (var factory in plugin.ProcessCustomDataFactories)
                {
                    services.AddSingleton(typeof(ICustomDataFactory<Process>), factory);
                }
            }
        }

        services.AddSingleton<CustomDataFactoryCollection<FileInfo>>(); // ExecutableInput
        services.AddSingleton<CustomDataFactoryCollection<Package>>(); // InstalledPackageInput
        services.AddSingleton<CustomDataFactoryCollection<Process>>(); // ProcessInput

        services.AddSingleton<InputFactory>();

        // Note: An alternate setup we could have would be to setup each check as a class as well to inject here.
        // Then we could theoretically have datasources be keyed to request in their constructors: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#keyed-services
        // However, the lifetime of the datasources doesn't work too well for this,
        // unless we made a parent datasource container that basically represented the type/info of the data source.
        // (which could be interesting to encapsulate some of the handling/setup to delegate from parsing.)
        // Then those are pulled by the CLI/App to inject and populate the actual data source instances to passthru to the checks at runtime.
        // That may be better long term, and I don't think too hard to modify from current approach.
        // I think the biggest question in this is how we handle multiple apps if we want to run across the
        // running processes. I'm missing how we handle that in the current design and associate all the
        // like data sources to associate with the same app/process.

        // I think in the current setup, we could just spawn off multiple calls to InspectProcessAsync above, as even though the DetectionEngine is shared and the static definitions (as we want), we'd be encapsulating
        // each app's actual detection of data sources with the calls to DetectAgainstSourcesWithProgressAsync...
        // So maybe it does work as-is.

        services.AddSingleton<DetectionEngine>();

        return services;
    }
}
