// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Parsing;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which runs the specified executable/package before inspecting it.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetRunCommand()
    {
        Option<string?> pathOption = new("--exePath", "-exe")
        {
            Description = "The full path of the program to run.",
        };

        Option<string?> packageOption = new("--packageFullName", "-pkg")
        {
            Description = "The full name of the package to run. Must be available to the current user (unless process is running as admin).",
        };

        Option<string?> aumidOption = new("--applicationUserModelId", "-aumid")
        {
            Description = "The app user model id of the app to run. Must be available to the current user (unless process is running as admin).",
        };

        Option<int?> waitTimeOption = new("--waitTime", "-wait")
        {
            Description = "The time in milliseconds to wait after starting the program before inspecting it. Default is 2000.",
        };

        Option<string?> outputFileOption = new("--outputFile", "-o")
        {
            Description = "Save the inspection report as JSON to the given filename.",
        };

        Option<string?> processNameOption = new("--processName")
        {
            Description = "The name of the process to inspect after launching (useful if different from the entry process).",
        };

        Option<bool> keepAfterInspectOption = new("--keepAfterInspect", "-keep")
        {
            Description = "Keep the started process running after inpecting.",
            Arity = ArgumentArity.Zero, // Note: Flag only, no value
        };

        var command = new Command("run", "Inspect a process/package provided to run first")
        {
            pathOption,
            packageOption,
            aumidOption,
            waitTimeOption,
            outputFileOption,
            processNameOption,
            keepAfterInspectOption,
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

            var exepath = parseResult.GetValue(pathOption);
            var packageFullName = parseResult.GetValue(packageOption);
            var aumid = parseResult.GetValue(aumidOption);
            var waitTime = parseResult.GetValue(waitTimeOption) ?? 2000;
            var outputFilename = parseResult.GetValue(outputFileOption);
            var processName = parseResult.GetValue(processNameOption);
            var keepAfterInspect = parseResult.GetValue(keepAfterInspectOption);

            if (exepath is not null)
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting program at \"{0}\"...", exepath);
                    process = Process.Start(exepath);
                }
                catch { }

                if (process is null)
                {
                    PrintError("Unable to find/start program at \"{0}\".", exepath);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (!string.IsNullOrEmpty(processName))
                {
                    var newProcess = await TryReattachAsync(processName, waitTime, cancellationToken);
                    if (newProcess is null)
                    {
                        return (int)ExitCode.ArgumentParsingError;
                    }

                    process = newProcess;
                    waitTime = 0; // Don't need to wait twice
                }

                return (int)await InspectStartedProcessAsync(process, waitTime, outputFilename, keepAfterInspect, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(packageFullName))
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting \"{0}\" app of package \"{1}\"...", aumid ?? "default", packageFullName);
                    process = await Process.StartByPackageFullNameAsync(packageFullName, aumid, cancellationToken);
                }
                catch { }

                if (process is null)
                {
                    PrintError("Unable to start \"{0}\" app of package \"{1}\".", aumid ?? "default", packageFullName);
                    return (int)ExitCode.ArgumentParsingError;
                }

                return (int)await InspectStartedProcessAsync(process, waitTime, outputFilename, keepAfterInspect,cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(aumid))
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting \"{0}\" app...", aumid);
                    process = await Process.StartByApplicationModelUserIdAsync(aumid, cancellationToken);
                }
                catch { }

                if (process is null)
                {
                    PrintError("Unable to start \"{0}\" app.", aumid);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (!string.IsNullOrEmpty(processName))
                {
                    var newProcess = await TryReattachAsync(processName, waitTime, cancellationToken);
                    if (newProcess is null)
                    {
                        return (int)ExitCode.ArgumentParsingError;
                    }

                    process = newProcess;
                    waitTime = 0; // Don't need to wait twice
                }

                return (int)await InspectStartedProcessAsync(process, waitTime, outputFilename, keepAfterInspect, cancellationToken);
            }

            PrintError("Missing command arguments.");
            await command.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }

    private async Task<Process?> TryReattachAsync(string processName, int waitTime, CancellationToken cancellationToken)
    {
        if (waitTime > 0)
        {
            PrintInfo("Waiting an additional {0}ms before looking for new process...", waitTime);
            await Task.Delay(waitTime, cancellationToken);
        }

        if (TryGetSingleProcessByName(processName, out var newProcess) && newProcess is not null)
        {
            return newProcess;
        }
        else
        {
            PrintError("Unable to re-attach to process with name \"{0}\".", processName);
        }
        return null;
    }

    private async Task<ExitCode> InspectStartedProcessAsync(Process process, int waitTime, string? outputFilename, bool keepAfterInspect, CancellationToken cancellationToken)
    {
        PrintInfo($"Process {process.ProcessName}({process.Id}) started...");

        if (waitTime > 0)
        {
            PrintInfo("Waiting an additional {0}ms before inspecting...", waitTime);
            await Task.Delay(waitTime, cancellationToken);
        }

        bool inspectResult = await InspectProcessAsync(process, outputFilename, cancellationToken);

        if (!keepAfterInspect)
        {
            try
            {
                PrintInfo("Killing process {0}({1}) after inspect.", process.ProcessName, process.Id);
                process.Kill();
            }
            catch
            {
                PrintError("Unable to kill {0}({1}) after inspect.", process.ProcessName, process.Id);
            }
        }

        return inspectResult ? ExitCode.Success : ExitCode.InspectFailed;
    }
}
