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
            Description = "The full path of the executable to run.",
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

        Option<string?> processNameOption = new("--processName")
        {
            Description = "The name of the process to inspect after launching (useful if different from the entry process).",
        };

        Option<bool> keepAfterInspectOption = new("--keepAfterInspect", "-keep")
        {
            Description = "Keep the started process running after inpecting.",
            Arity = ArgumentArity.Zero, // Note: Flag only, no value
        };

        var command = new Command("run", "Run a given a process/package and inspect it")
        {
            pathOption,
            packageOption,
            aumidOption,
            waitTimeOption,
            OutputFileOption,
            processNameOption,
            keepAfterInspectOption,
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

            var exepath = parseResult.GetValue(pathOption);
            var packageFullName = parseResult.GetValue(packageOption);
            var aumid = parseResult.GetValue(aumidOption);
            var waitTime = parseResult.GetValue(waitTimeOption) ?? 2000;
            var processName = parseResult.GetValue(processNameOption);
            var keepAfterInspect = parseResult.GetValue(keepAfterInspectOption);
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            if (!TryParseOutputFile(parseResult))
            {
                PrintError("Invalid output file specified");
                return (int)ExitCode.ArgumentParsingError;
            }

            if (exepath is not null)
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting program at \"{0}\"...", exepath);
                    process = Process.Start(exepath);
                }
                catch { }

                try
                {
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

                    return (int)await InspectStartedProcessAsync(process, waitTime, OutputFile, keepAfterInspect, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    PrintWarning("Run cancelled.");
                    return (int)ExitCode.RunFailed;
                }
            }
            else if (!string.IsNullOrWhiteSpace(packageFullName))
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting \"{0}\" app of package \"{1}\"...", aumid ?? "default", packageFullName);
                    process = await Process.StartByPackageFullNameAsync(packageFullName, aumid, cancellationToken);
                    if (aumid is null && process is not null && process.TryGetApplicationUserModelId(out var defaultAumid) && defaultAumid is not null)
                    {
                        PrintInfo("Started \"{0}\" app of package \"{1}\".", defaultAumid, packageFullName);
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

                    if (process is null)
                    {
                        PrintError("Unable to start \"{0}\" app of package \"{1}\".", aumid ?? "default", packageFullName);
                        return (int)ExitCode.ArgumentParsingError;
                    }

                    return (int)await InspectStartedProcessAsync(process, waitTime, OutputFile, keepAfterInspect, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    PrintWarning("Run cancelled.");
                    return (int)ExitCode.RunFailed;
                }
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

                try
                {
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

                    return (int)await InspectStartedProcessAsync(process, waitTime, OutputFile, keepAfterInspect, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    PrintWarning("Run cancelled.");
                    return (int)ExitCode.RunFailed;
                }
            }

            return (int)await InvalidArgumentsShowHelpAsync(command);
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
        try
        {
            PrintInfo($"Process {process.ProcessName}({process.Id}) started...");

            if (waitTime > 0)
            {
                PrintInfo("Waiting an additional {0}ms before inspecting...", waitTime);
                await Task.Delay(waitTime, cancellationToken);
            }

            bool inspectResult = await InspectProcessAsync(process, outputFilename, cancellationToken);

            return inspectResult ? ExitCode.Success : ExitCode.InspectFailed;
        }
        finally
        {
            // No matter what happens, try to kill the process we started
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
        }
    }
}
