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
    private Command GetInspectProcessSubCommand()
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
            OutputFileOption,
            IncludeChildrenOption,
            WaitForInputIdleOption,
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
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            if (!TryParseOutputFile(parseResult))
            {
                PrintError("Invalid output file specified");
                return (int)ExitCode.ArgumentParsingError;
            }

            if (processId is not null)
            {
                if (await InspectProcessAsync(Process.GetProcessById(processId.Value), OutputFile, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }
            else if (!string.IsNullOrWhiteSpace(processName) && TryGetSingleProcessByName(processName, out var process) && process is not null)
            {
                if (!await InspectProcessAsync(process, OutputFile, cancellationToken))
                {
                    return (int)ExitCode.InspectFailed;
                }

                return (int)ExitCode.Success;
            }

            return (int)await InvalidArgumentsShowHelpAsync(processCommand);
        });

        return processCommand;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectProcessAsync(Process process, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"process {process.ProcessName}({process.Id}){(IncludeChildren ? " (and children)" : "")}";

        try
        {
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

            var inputs = await InputHelper.GetInputsFromProcessAsync(process, IncludeChildren, cancellationToken);

            PrintInfo("Inspecting {0}:", target);

            return await RunInspectionAsync(target, inputs, outputFilename, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            PrintWarning("Inspection canceled.");
            return false;
        }
    }
}
