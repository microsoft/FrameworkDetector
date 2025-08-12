// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace FrameworkDetector.CLI;

internal static class Program
{
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

        rootCommand.SetAction(parseResult =>
        {
            if (parseResult.GetValue(pidOption) is int processId)
            {
                InspectProcess(processId);

                return 0;
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

    private static void InspectProcess(int processId)
    {
        // TODO: Probably have this elsewhere to be called
        Console.WriteLine($"Inspecting process with ID: {processId}");

        var cts = new CancellationTokenSource();

        // TODO: Put into a class that can orchestrate multiple of these detectors in parallel and contruct the combined result

        var process = Process.GetProcessById(processId);

        var metadata = WindowsBinaryMetadata.GetMetadata(process);

        var wpfDetector = new Detectors.WpfDetector();
        var task = wpfDetector.DetectByProcessAsync(process, cts.Token);
        task.Wait();

        var result = new JsonObject();
        result["toolName"] = AssemblyInfo.ToolName;
        result["toolVersion"] = AssemblyInfo.ToolVersion;

        result["target"] = metadata.AsJson();

        if (task.IsCompletedSuccessfully)
        {
            result["target"]!["detectors"] = new JsonArray()
            {
                task.Result.AsJson(),
            };
        }

        Console.WriteLine(result.ToJsonString(new JsonSerializerOptions() { WriteIndented = true }));
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine($"Unhandled exception: {e.ExceptionObject}");
    }
}
