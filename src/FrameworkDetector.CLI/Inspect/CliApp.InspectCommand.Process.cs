// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectProcessSubCommand(Option<string?> outputFileOption)
    {
        Option<int?> pidOption = new("--id", "-pid", "-id")
        {
            Description = "The PID of the process to inspect.",
        };

        Option<string?> processNameOption = new("--name", "-n", "-name")
        {
            Description = "The name of the process to inspect.",
        };

        Command processCommand = new("process", "Inspect a running process")
        {
            pidOption,
            processNameOption,
        };

        processCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
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
                if (await InspectProcessAsync(Process.GetProcessById(processId.Value), outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }
            else if (!string.IsNullOrWhiteSpace(processName) && TryGetSingleProcessByName(processName, out var process) && process is not null)
            {
                if (await InspectProcessAsync(process, outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }

            PrintError("Missing command arguments.");
            await processCommand.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return processCommand;
    }

    private bool TryGetSingleProcessByName(string processName, out Process? result)
    {
        PrintInfo("Searching for process named \"{0}\"...", processName);

        var processes = Process.GetProcessesByName(processName);

        if (processes.Length == 0)
        {
            PrintError("Unable to find process with name \"{0}\".", processName);
        }
        else if (processes.Length == 1)
        {
            PrintInfo("Found process {0}({1}).\n", processes[0].ProcessName, processes[0].Id);
            result = processes[0];
            return true;
        }
        else
        {
            PrintWarning("More than one process with name \"{0}\":", processName);
            foreach (var process in processes)
            {
                PrintWarning("  {0}({1})", process.ProcessName, process.Id);
            }

            if (processes.TryGetRootProcess(out var rootProcess) && rootProcess is not null)
            {
                PrintInfo("Determined root process {0}({1}).\n", rootProcess.ProcessName, rootProcess.Id);
                result = rootProcess;
                return true;
            }
            else
            {
                PrintWarning("Unable to determine a root process, defaulting to the lowest PID.");
                result = processes.OrderBy(p => p.Id).First();
                return true;
            }
        }

        result = default;
        return false;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectProcessAsync(Process process, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"process {process.ProcessName}({process.Id}){(IncludeChildren ? " (and children)" : "")}";

        PrintInfo("Preparing to inspect {0}...", target);

        if (!process.IsAccessible())
        {
            PrintError("Cannot access {0} to inspect" + (!WindowsIdentity.IsRunningAsAdmin ? ", try running as Administrator." : "."), target);
            return false;
        }

        if (WaitForInputIdle)
        {
            PrintInfo("Waiting for input idle for {0}", target);
            if (!await process.TryWaitForIdleAsync(cancellationToken))
            {
                PrintError("Waiting for input idle for {0} failed, try running again.", target);
                return false;
            }
        }

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.Write($"Inspecting {target}:");
        }

        var inputs = await InputHelper.GetInputsFromProcessAsync(process, IncludeChildren, cancellationToken);

        return await RunInspectionAsync(target, inputs, outputFilename, cancellationToken);
    }
}
