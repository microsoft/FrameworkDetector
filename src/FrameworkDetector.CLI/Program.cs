// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using FrameworkDetector.DataSources;
using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

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

        Option<int?> pidOption = new("--processId", "--pid")
        {
            Description = "The PID of the process to inspect.",
        };

        Option<string?> processNameOption = new("--processName")
        {
            Description = "The name of the process to inspect.",
        };

        Option<string?> outputFileOption = new("--outputFile")
        {
            Description = "Save the inspection report as JSON to the given filename.",
        };

        RootCommand rootCommand = new("Framework Detector");
        rootCommand.Options.Add(pidOption);
        rootCommand.Options.Add(processNameOption);
        rootCommand.Options.Add(outputFileOption);

        // TODO: Not familiar enough with System.CommandLine lib yet to understand if we have multiple data sources how to chain them together.
        // Basically each parameter should create it' datasource to add to the list passed into the DetectionEngine.
        //// https://learn.microsoft.com/dotnet/standard/commandline/how-to-parse-and-invoke#asynchronous-actions
        rootCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
        {
            if (parseResult.Errors.Count > 0)
            {
                // Display any command argument errors
                foreach (ParseError parseError in parseResult?.Errors ?? Array.Empty<ParseError>())
                {
                    PrintError(parseError.Message);
                }

                return 1;
            }

            var processId = parseResult.GetValue(pidOption);
            var processName = parseResult.GetValue(processNameOption);
            var outputFilename = parseResult.GetValue(outputFileOption);

            if (processId is not null)
            {
                if (await InspectProcess(Process.GetProcessById(processId.Value), outputFilename, cancellationToken))
                {
                    return 0;
                }

                // TODO: Define an error code enum somewhere for various error conditions
                return 2;
            }
            else if (!string.IsNullOrWhiteSpace(processName))
            {
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    PrintError("Unable to find processes with name \"{0}\".", processName);
                }
                else if (processes.Length > 1)
                {
                    //TODO: figure out how if want to handle inspecting multiple processes and how to output the results.
                    PrintError("More than one process with name \"{0}\":", processName);
                    foreach (var process in processes)
                    {
                        PrintError("  {0}({1})", process.ProcessName, process.Id);
                    }
                    PrintError("Please run again with the PID of the specific process you wish to inspect.");
                }
                else if (await InspectProcess(processes[0], outputFilename, cancellationToken))
                {
                    return 0;
                }

                // TODO: Define an error code enum somewhere for various error conditions
                return 2;
            }

            return 1;
        });

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    //// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private static async Task<bool> InspectProcess(Process process, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        Console.WriteLine($"Inspecting process {process.ProcessName}({process.Id})");

        DataSourceCollection sources = new([new ProcessDataSource(process)]);

        var progressIndicator = new Progress<int>(ReportProgress);

        DetectionEngine engine = Services.GetRequiredService<DetectionEngine>();

        ToolRunResult result = await engine.DetectAgainstSourcesWithProgressAsync(sources, progressIndicator, cancellationToken);

        if (!string.IsNullOrWhiteSpace(outputFilename))
        {
            Console.WriteLine($"Saving output to: \"{outputFilename}\".");

            using var outputWriter = new StreamWriter(outputFilename);
            outputWriter.WriteLine(result.ToString());
        }

        // TODO: Return false on failure
        return true;
    }

    // TODO: Note this is getting called out of order for some reason, but pretty sure I was using IProgress properly in the threaded context, but needs further investigation...
    private static void ReportProgress(int obj)
    {
        // TODO: Use SpectreConsole Progress here to be fancy
        Console.WriteLine($"Progress: %{obj}");
    }

    private static void PrintException(Exception ex, string messageFormat = "Error: {0}", bool showStackTrace = true)
    {
        Console.WriteLine();
        PrintError(messageFormat, ex.Message);
        if (showStackTrace && ex.StackTrace is not null)
        {
            PrintError(ex.StackTrace);
        }

        if (ex.InnerException is not null)
        {
            PrintException(ex.InnerException);
        }
    }

    private static void PrintError(string format, params object[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Error.WriteLine(format, args);

        Console.ForegroundColor = oldColor;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            PrintException(ex, "Unhandled exception: {0}");
        }
        else
        {
            PrintError("Unhandled exception {0}", e.ExceptionObject);
        }
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

        // I think in the current setup, we could just spawn off multiple calls to InspectProcess above, as even though the DetectionEngine is shared and the static definitions (as we want), we'd be encapsulating
        // each app's actual detection of data sources with the calls to DetectAgainstSourcesWithProgressAsync...
        // So maybe it does work as-is.

        services.AddSingleton<DetectionEngine>();

        return services.BuildServiceProvider();
    }
}
