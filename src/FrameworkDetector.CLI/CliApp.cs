// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ConsoleTables;
using System.CommandLine;

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    public bool IsRunningAsAdmin { get; } = CheckIfRunningAsAdmin();

    private static bool CheckIfRunningAsAdmin()
    {
        // Check if process running as admin and initialize our property.
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();

        WindowsPrincipal principal = new(identity);

        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public CliApp() { }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var rootCommand = new RootCommand("Framework Detector")
        {
            GetInspectCommand(),
            GetRunCommand(),
        };

        var config = new CommandLineConfiguration(rootCommand);
        config.EnableDefaultExceptionHandler = false;

        return await config.Parse(args).InvokeAsync(cancellationToken);
    }

    private void PrintResult(ToolRunResult result, bool verbose)
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

    private bool TrySaveOutput(ToolRunResult result, string? outputFilename)
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

    private void PrintException(Exception ex, string messageFormat = "Exception: {0}", bool showStackTrace = true)
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

    private void PrintError(string format, params object[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Error.WriteLine("error: " + format, args);

        Console.ForegroundColor = oldColor;
    }

    private void PrintInfo(string format, params object[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.Out.WriteLine("info: " + format, args);

        Console.ForegroundColor = oldColor;
    }

    private void PrintWarning(string format, params object[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.Out.WriteLine("warning: " + format, args);

        Console.ForegroundColor = oldColor;
    }

    internal void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
}
