// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ConsoleTables;
using FrameworkDetector.DataSources;
using FrameworkDetector.Detectors;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FrameworkDetector.CLI.Program;

namespace FrameworkDetector.CLI;

internal static class Program
{
    private static IServiceProvider Services = ConfigureServices();

    public enum ExitCode
    {
        Success = 0,
        ArgumentParsingError = 1,
        InspectFailed = 2,
    }

    //// Handles main command line parsing through System.CommandLine lib
    //// TODO: Use Spectre.Console throughout for pretty output. See ReportProgress method stub below for more.
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Console.OutputEncoding = Encoding.UTF8;

        var rootCommand = new RootCommand("Framework Detector")
        {
            GetInspectCommand(),
        };

        var cts = new CancellationTokenSource();
        return await rootCommand.Parse(args).InvokeAsync(cts.Token);
    }

    private static Command GetInspectCommand()
    {
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

        Option<bool> verboseOption = new("--verbose", "--v")
        {
            Description = "Print verbose output.",
        };

        var command = new Command("inspect", "Inspect a given process")
        {
            pidOption,
            processNameOption,
            outputFileOption,
            verboseOption,
        };
        command.TreatUnmatchedTokensAsErrors = true;

        command.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
        {
            if (parseResult.Errors.Count > 0)
            {
                // Display any command argument errors
                foreach (ParseError parseError in parseResult?.Errors ?? Array.Empty<ParseError>())
                {
                    PrintError(parseError.Message);
                }

                return (int)ExitCode.ArgumentParsingError;
            }

            var processId = parseResult.GetValue(pidOption);
            var processName = parseResult.GetValue(processNameOption);
            var outputFilename = parseResult.GetValue(outputFileOption);
            var verbose = parseResult.GetValue(verboseOption);

            if (processId is not null)
            {
                if (await InspectProcessAsync(Process.GetProcessById(processId.Value), verbose, outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }

                return (int)ExitCode.InspectFailed;
            }
            else if (!string.IsNullOrWhiteSpace(processName))
            {
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    PrintError("Unable to find process with name \"{0}\".", processName);
                }
                else if (processes.Length > 1)
                {
                    //TODO: figure out how to handle inspecting multiple processes and how to output the results.
                    PrintWarning("More than one process with name \"{0}\":", processName);
                    foreach (var process in processes)
                    {
                        PrintWarning("  {0}({1})", process.ProcessName, process.Id);
                    }

                    if (processes.TryGetRootProcess(out var rootProcess) && rootProcess is not null)
                    {
                        PrintWarning("Determined root process {0}({1}).\n", rootProcess.ProcessName, rootProcess.Id);
                        if (await InspectProcessAsync(rootProcess, verbose, outputFilename, cancellationToken))
                        {
                            return (int)ExitCode.Success;
                        }
                    }

                    PrintError("Please run again with the PID of the specific process you wish to inspect.");
                }
                else if (await InspectProcessAsync(processes[0], verbose, outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }

                return (int)ExitCode.InspectFailed;
            }

            PrintError("Missing command arguments.");
            command.Parse("-h").Invoke();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }
    
    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private static async Task<bool> InspectProcessAsync(Process process, bool verbose, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var message = $"Inspecting process {process.ProcessName}({process.Id})";
        Console.Write($"{message}:");

        DataSourceCollection sources = new([new ProcessDataSource(process)]);

        DetectionEngine engine = Services.GetRequiredService<DetectionEngine>();
        engine.DetectionProgressChanged += (s, e) =>
        {
            Console.Write($"\r{message}: {e.Progress:000.0}%");
        };

        ToolRunResult result = await engine.DetectAgainstSourcesAsync(sources, cancellationToken);

        Console.WriteLine();

        if (cancellationToken.IsCancellationRequested)
        {
            PrintWarning("Inspection was canceled prior to completion.");
            Console.WriteLine();
        }

        PrintResult(result, verbose);

        TrySaveOutput(result, outputFilename);

        // TODO: Return false on failure
        return true;
    }

    private static void PrintResult(ToolRunResult result, bool verbose)
    {
        var table = new ConsoleTable("Framework",
                                     "Result");

        table.Options.EnableCount = false;
        table.MaxWidth = Console.BufferWidth - 10;

        foreach (var detectorResult in result.DetectorResults.OrderByDescending(dr => dr.FrameworkFound).ThenBy(dr => dr.DetectorName))
        {
            var detectorResultString = "  🟨";

            if (detectorResult.DetectorStatus == DetectorStatus.Completed)
            {
                detectorResultString = detectorResult.FrameworkFound ? "  ✅" : "  🟥";
            }

            table.AddRow(detectorResult.DetectorDescription,
                         detectorResultString);

            if (verbose)
            {
                foreach (var checkResult in detectorResult.CheckResults)
                {
                    var checkResultString = "  🟨";
                    switch (checkResult.CheckStatus)
                    {
                        case DetectorCheckStatus.CompletedPassed:
                            checkResultString = "  ✅";
                            break;
                        case DetectorCheckStatus.CompletedFailed:
                            checkResultString = "  🟥";
                            break;
                    }

                    table.AddRow($"  {checkResult.CheckDefinition}",
                                 checkResultString);
                }
            }
        }

        Console.WriteLine();
        table.Write(Format.MarkDown);
    }

    private static bool TrySaveOutput(ToolRunResult result, string? outputFilename)
    {
        if (!string.IsNullOrWhiteSpace(outputFilename))
        {
            Console.WriteLine($"Saving output to: \"{outputFilename}\".");

            using var outputWriter = new StreamWriter(outputFilename);
            outputWriter.WriteLine(result.ToString());
            return true;
        }

        return false;
    }

    private static void PrintException(Exception ex, string messageFormat = "Exception: {0}", bool showStackTrace = true)
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

        Console.Error.WriteLine("error: " + format, args);

        Console.ForegroundColor = oldColor;
    }

    private static void PrintWarning(string format, params object[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.Out.WriteLine("warning: " + format, args);

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
        services.AddSingleton<IDetector, AvaloniaDetector>();
        services.AddSingleton<IDetector, DotNetCoreDetector>();
        services.AddSingleton<IDetector, DotNetFrameworkDetector>();
        services.AddSingleton<IDetector, MVVMToolkitDetector>();
        services.AddSingleton<IDetector, RNWDetector>();
        services.AddSingleton<IDetector, UWPXAMLDetector>();
        services.AddSingleton<IDetector, WebView2Detector>();
        services.AddSingleton<IDetector, WinFormsDetector>();
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
