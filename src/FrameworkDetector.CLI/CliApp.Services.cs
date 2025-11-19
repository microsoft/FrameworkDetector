// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using Microsoft.Extensions.DependencyInjection;

using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    private static IServiceProvider Services = ConfigureServices();

    internal static IServiceProvider ConfigureServices()
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
        services.AddSingleton<IDetector, DUIDetector>();
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

        return services.BuildServiceProvider();
    }
}
