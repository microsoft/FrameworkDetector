// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ConsoleInk;
using ConsoleTables;
using System.CommandLine;

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    // Value here represents the default verbosity when the -v option is specified without argument; otherwise it is defined below as normal in the DefaultValueFactory.
    private VerbosityLevel Verbosity { get; set; } = VerbosityLevel.Diagnostic;

    private bool IncludeChildren { get; set; }

    private bool WaitForInputIdle { get; set; }

    private string? ArgumentMetadata { get; set; }

    public CliApp() { }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        ArgumentMetadata = string.Join(' ', args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));

        Console.OutputEncoding = Encoding.UTF8;

        // TODO: Figure out how to accept the various shortnames, not specified in help:
        // https://learn.microsoft.com/dotnet/standard/commandline/syntax#parse-errors
        // https://learn.microsoft.com/dotnet/standard/commandline/design-guidance#the---verbosity-option
        Option<string?> verbosityOption = new("--verbosity", "-v")
        {
            Description = "Set the verbosity level of printed output. If no additional value specified after '-v', defaults to 'diagnostic'.",
            DefaultValueFactory = parseResult => "normal", // Note: Default value for standard running mode
            Recursive = true, // Note: Makes this a global command when added to the Root Command
            Arity = ArgumentArity.ZeroOrOne, // Note: Let's us specify -v without a value, uses C# default in that case (default above used when not specified at all)
        };
        verbosityOption.AcceptOnlyFromAmong("quiet", "minimal", "normal", "detailed", "diagnostic");

        Option<bool> includeChildrenOption = new("--includeChildren", "-c")
        {
            Description = "Include the children processes of an inspected process. (May require running with elevation.)",
            Recursive = true, // Note: Makes this a global command when added to the Root Command
            Arity = ArgumentArity.Zero, // Note: Flag only, no value
        };

        Option<bool> waitForInputIdleOption = new("--waitForInputIdle", "-w")
        {
            Description = "Wait for input idle of process before inspecting.",
            Recursive = true, // Note: Makes this a global command when added to the Root Command
            Arity = ArgumentArity.Zero, // Note: Flag only, no value
        };

        var rootCommand = new RootCommand("Framework Detector")
        {
            // Global Options (Recursive = true)
            verbosityOption,
            includeChildrenOption,
            waitForInputIdleOption,
            // Commands
            GetDocsCommand(),
            GetDumpCommand(),
            GetInspectAllCommand(),
            GetInspectCommand(),
            GetListCommand(),
            GetRunCommand(),
        };

        var config = new InvocationConfiguration();
        config.EnableDefaultExceptionHandler = false;

        var result = rootCommand.Parse(args);
        // Note: When "-v" specified without a value we get "null" so our default becomes the default value of the property.
        var verbosityString = result.GetValue(verbosityOption);
        IncludeChildren = result.GetValue(includeChildrenOption);
        WaitForInputIdle = result.GetValue(waitForInputIdleOption);

        if (Enum.TryParse(verbosityString, true, out VerbosityLevel verbosity))
        {
            Verbosity = verbosity;
        }
        else if (!string.IsNullOrWhiteSpace(verbosityString))
        {
            PrintError("Invalid verbosity level specified: {0}", verbosityString!);
            return (int)ExitCode.ArgumentParsingError;
        }

        PrintInfo("Verbosity set to {0} - Running as Admin: {1}", Verbosity, WindowsIdentity.IsRunningAsAdmin);

        return await result.InvokeAsync(config, cancellationToken);
    }

    private void PrintResult(ToolRunResult result)
    {
        if (Verbosity == VerbosityLevel.Quiet)
        {
            return;
        }

        var table = new ConsoleTable("Framework",
                                     "Found",
                                     "Version");

        table.Options.EnableCount = false;

        var results = Verbosity > VerbosityLevel.Normal ? result.DetectorResults : result.DetectorResults.Where(dr => dr.FrameworkFound);

        foreach (var detectorResult in results.OrderByDescending(dr => dr.FrameworkFound).ThenByDescending(dr => dr.HasAnyPassedChecks).ThenBy(dr => dr.DetectorName))
        {
            var detectorResultString = "  🟨";

            if (detectorResult.DetectorStatus == DetectorStatus.Completed)
            {
                detectorResultString = detectorResult.FrameworkFound ? " ✅" : // Green checked box for framework found
                    (detectorResult.HasAnyPassedChecks ?
                    " 🟨" : // Yellow box for at least one check passed (even if detector failed)
                    " 🟥"); // Red box for not checks passed
            }

            table.AddRow($"[{detectorResult.FrameworkId}] {detectorResult.DetectorDescription}",
                         detectorResultString, detectorResult.FrameworkVersion);

            if (Verbosity == VerbosityLevel.Diagnostic)
            {
                foreach (var checkResult in detectorResult.CheckResults)
                {
                    var checkResultString = " 🟨";
                    switch (checkResult.CheckStatus)
                    {
                        case DetectorCheckStatus.CompletedPassed:
                            checkResultString = " ✅";
                            break;
                        case DetectorCheckStatus.CompletedFailed:
                            checkResultString = " 🟥";
                            break;
                    }

                    table.AddRow($"  {checkResult.CheckDefinition}",
                                 checkResultString, "");
                }
            }
        }

        table.SetMaxWidthBasedOnColumn(0);

        Console.WriteLine();
        // Using ConsoleTable's "MarkDown" formatting just for the specific effect, it looks worse piped into ConsoleInk
        table.Write(Format.MarkDown);
    }

    private bool TrySaveOutput(ToolRunResult result, string? outputFilename)
    {
        if (!string.IsNullOrWhiteSpace(outputFilename))
        {
            PrintInfo("Saving output to: \"{0}\".", outputFilename);

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
        if (Verbosity == VerbosityLevel.Quiet)
        {
            return;
        }

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

    private void PrintMarkdown(string s)
    {
        var options = new MarkdownRenderOptions
        {
            UseHyperlinks = true,
            Theme = new ConsoleTheme
            {
                LinkTextStyle = Ansi.Underline + Ansi.FgBrightBlue,
                LinkUrlStyle = Ansi.FgBrightCyan
            },
        };

        Console.WriteLine();
        MarkdownConsole.Render(s, Console.Out, options);
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
