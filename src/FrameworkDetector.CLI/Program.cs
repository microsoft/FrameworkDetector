// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;

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

        ParseResult? parseResult = null;
        try
        {
            parseResult = rootCommand.Parse(args);
            if (parseResult.GetValue(pidOption) is int processId)
            {
                InspectProcess(processId);
            }
        } 
        catch (InvalidOperationException)
        {
            // Display any command argument errors
            foreach (ParseError parseError in parseResult?.Errors ?? Array.Empty<ParseError>())
            {
                Console.Error.WriteLine(parseError.Message);
            }

            return 1;
        }

        return 0;
    }

    private static void InspectProcess(int processId)
    {
        // TODO: Probably have this elsewhere to be called
        Console.WriteLine($"Inspecting process with ID: {processId}");
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine($"Unhandled exception: {e.ExceptionObject}");
    }
}
