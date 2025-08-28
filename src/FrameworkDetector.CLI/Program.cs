// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.CLI;

internal static class Program
{
    private static IServiceProvider Services = ConfigureServices();

    //// Handles main command line parsing through System.CommandLine lib
    //// TODO: Use Spectre.Console throughout for pretty output. See ReportProgress method stub below for more.
    [STAThread]
    public static int Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Option<int> pidOption = new("--processId", "--pid")
        {
            Description = "The process ID to inspect.",
            Required = true,
        };

        RootCommand rootCommand = new("Framework Detector");
        rootCommand.Options.Add(pidOption);

        // TODO: Not familiar enough with System.CommandLine lib yet to understand if we have multiple data sources how to chain them together.
        // Basically each parameter should create it's datasource to add to the list passed into the DetectionEngine.
        //// https://learn.microsoft.com/dotnet/standard/commandline/how-to-parse-and-invoke#asynchronous-actions
        rootCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
        {
            if (parseResult.GetValue(pidOption) is int processId)
            {
                if (await InspectProcess(processId, cancellationToken))
                {
                    return 0;
                }

                // TODO: Define an error code enum somewhere for various error conditions
                return 2;
            }
            else
            {
                // Display any command argument errors
                foreach (ParseError parseError in parseResult?.Errors ?? Array.Empty<ParseError>())
                {
                    Console.Error.WriteLine(parseError.Message);
                }

                return 1;
            }
        });

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    //// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private static async Task<bool> InspectProcess(int processId, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        Console.WriteLine($"Inspecting process with ID: {processId}");

        DataSourceCollection sources = new([new ProcessDataSource(processId)]);

        var progressIndicator = new Progress<int>(ReportProgress);

        DetectionEngine engine = Services.GetRequiredService<DetectionEngine>();

        ToolRunResult result = await engine.DetectAgainstSourcesWithProgressAsync(sources, progressIndicator, cancellationToken);

        Console.WriteLine(result.ToString());

        // TODO: Return false on failure
        return true;
    }

    // TODO: Note this is getting called out of order for some reason, but pretty sure I was using IProgress properly in the threaded context, but needs further investigation...
    private static void ReportProgress(int obj)
    {
        // TODO: Use SpectreConsole Progress here to be fancy
        Console.WriteLine($"Progress: %{obj}");
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine($"Unhandled exception: {e.ExceptionObject}");
    }

    internal static IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        // TODO: Add a Logger here that we can use to report issues or record debug info, etc...

        // TODO: Would be nice if the SG could do this for us, there's a request open: https://github.com/CommunityToolkit/Labs-Windows/discussions/463#discussioncomment-11720493

        // ---- ADD DETECTORS HERE ----
        services.AddSingleton<IDetector, MVVMToolkitDetector>();
        services.AddSingleton<IDetector, UWPXAMLDetector>();
        services.AddSingleton<IDetector, WebView2Detector>();
        services.AddSingleton<IDetector, WpfDetector>();

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

        // I think in the current setup, we could just spawn off multiple calls to InspectProcess above, as even though the DetectionEngine is shared and the static definitions (as we want), we'd be encapsulating
        // each app's actual detection of data sources with the calls to DetectAgainstSourcesWithProgressAsync...
        // So maybe it does work as-is.

        services.AddSingleton<DetectionEngine>();

        return services.BuildServiceProvider();
    }
}
