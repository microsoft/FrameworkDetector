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
    private Command GetDumpProcessSubCommand()
    {
        Option<int?> pidOption = new("--id", "-pid", "-id")
        {
            Description = "The PID of the process to dump.",
        };

        Option<string?> processNameOption = new("--name", "-n", "-name")
        {
            Description = "The name of the process to dump.",
        };

        var command = new Command("process", "Dump the metadata of a running process")
        {
            pidOption,
            processNameOption,
            OutputFileOption,
            IncludeChildrenOption,
            WaitForInputIdleOption,
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
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            if (!TryParseOutputFile(parseResult))
            {
                PrintError("Invalid output file specified");
                return (int)ExitCode.ArgumentParsingError;
            }

            if (processId is not null)
            {
                if (await DumpProcessAsync(Process.GetProcessById(processId.Value), OutputFile, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }
            else if (!string.IsNullOrWhiteSpace(processName) && TryGetSingleProcessByName(processName, out var process) && process is not null)
            {
                if (!await DumpProcessAsync(process, OutputFile, cancellationToken))
                {
                    return (int)ExitCode.DumpFailed;
                }

                return (int)ExitCode.Success;
            }

            return (int)await InvalidArgumentsShowHelpAsync(command);
        });

        return command;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> DumpProcessAsync(Process process, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"process {process.ProcessName}({process.Id}){(IncludeChildren ? " (and children)" : "")}";

        try
        {
            PrintInfo("Preparing to dump {0}...", target);

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

            PrintInfo("Dumping {0}:", target);

            return await RunDumpAsync(target, inputs, outputFilename, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            PrintWarning("Dump canceled.");
            return false;
        }
    }
}
