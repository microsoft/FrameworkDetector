// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which provides all known details (available to the tool) about a given running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetDumpCommand()
    {
        Option<int?> pidOption = new("--processId", "-pid")
        {
            Description = "The PID of the process to inspect.",
        };

        Option<string?> processNameOption = new("--processName")
        {
            Description = "The name of the process to inspect.",
        };

        Option<string?> outputFileOption = new("--outputFile", "-o")
        {
            Description = "Save the dump report as JSON to the given filename.",
        };

        var command = new Command("dump", "Dump the details of a given process")
        {
            pidOption,
            processNameOption,
            outputFileOption,
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

            if (processId is not null)
            {
                if (await DumpProcessAsync(Process.GetProcessById(processId.Value), outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
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
                        if (await DumpProcessAsync(rootProcess, outputFilename, cancellationToken))
                        {
                            return (int)ExitCode.Success;
                        }
                    }
                    else
                    {
                        PrintError("Please run again with the PID of the specific process you wish to dump.");
                    }
                }
                else if (await DumpProcessAsync(processes[0], outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }

                return (int)ExitCode.InspectFailed;
            }

            PrintError("Missing command arguments.");
            await command.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> DumpProcessAsync(Process process, string? outputFilename, CancellationToken cancellationToken)
    {
        PrintInfo($"Dumping process {process.ProcessName}({process.Id}){(IncludeChildren ? " (and children)" : "")}");

        if (!process.IsAccessible())
        {
            PrintError("Cannot access process {0}({1}) to dump" + (!WindowsIdentity.IsRunningAsAdmin ? ", try running as Administrator." : "."), process.ProcessName, process.Id);
            return false;
        }

        if (WaitForInputIdle)
        {
            PrintInfo("Waiting for input idle for process {0}({1})", process.ProcessName, process.Id);
            if (!await process.TryWaitForIdleAsync(cancellationToken))
            {
                PrintError("Waiting for input idle for process {0}({1}) failed, try running again.", process.ProcessName, process.Id);
                return false;
            }
        }

        var inputs = await InputHelper.GetInputsFromProcessAsync(process, IncludeChildren, cancellationToken);

        DetectionEngine engine = Services.GetRequiredService<DetectionEngine>();

        ToolRunResult result = await engine.DumpAllDataFromInputsAsync(inputs, cancellationToken, ArgumentMetadata);

        Console.WriteLine();

        if (cancellationToken.IsCancellationRequested)
        {
            PrintWarning("Dump was canceled prior to completion.");
            Console.WriteLine();
        }

        if (Verbosity >= VerbosityLevel.Normal)
        {
            // TODO: Pretty print to console in a nice way
            Console.WriteLine(result.ToString());
        }

        TrySaveOutput(result, outputFilename);

        // TODO: Return false on failure
        return true;
    }
}
