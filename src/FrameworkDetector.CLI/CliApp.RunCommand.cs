// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using System.CommandLine;
using System.CommandLine.Parsing;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

using FrameworkDetector.Engine;

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

        Option<int?> waitTimeOption = new("--waitTime", "-wait")
        {
            Description = "The time in milliseconds to wait after starting the program before inspecting it. Default is 2000.",
        };

        Option<string?> outputFileOption = new("--outputFile", "-o")
        {
            Description = "Save the inspection report as JSON to the given filename.",
        };

        var command = new Command("run", "Inspect a process/package provided to run first")
        {
            pathOption,
            packageOption,
            waitTimeOption,
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

            var exepath = parseResult.GetValue(pathOption);
            var packageFullName = parseResult.GetValue(packageOption);
            var waitTime = parseResult.GetValue(waitTimeOption) ?? 2000;
            var outputFilename = parseResult.GetValue(outputFileOption);

            if (exepath is not null)
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting program at \"{0}\"", exepath);
                    process = Process.Start(exepath);
                }
                catch (Exception)
                {
                    PrintError("Unable to find/start process at \"{0}\".", exepath);
                    return (int)ExitCode.ArgumentParsingError;
                }

                PrintInfo("Process Running (PID={0})", process.Id);

                // TODO: This is copied below, we should refactor into a common method for "waiting for UI process".
                PrintInfo("Waiting for UI Idle of app");
                if (process?.WaitForInputIdle() == true && process?.Responding == true)                
                {
                    // Question: Should we wait first and then check for idle?
                    if (waitTime > 0)
                    {
                        PrintInfo("Waiting an additional {0}ms before inspecting...", waitTime);
                        Thread.Sleep(waitTime);
                    }

                    PrintInfo("Inspecting app...");
                    if (await InspectProcessAsync(process, outputFilename, cancellationToken))
                    {
                        return (int)ExitCode.Success;
                    }
                }

                return (int)ExitCode.InspectFailed;
            }
            else if (!string.IsNullOrWhiteSpace(packageFullName))
            {
                // See: https://github.com/microsoft/WindowsAppSDK/discussions/2747
                // Note: This code required us to specify a specific Windows Version TFM
                PackageManager packageManager = new();
                Package? pkg = null;

                // 1. Find package by full name
                if (IsRunningAsAdmin)
                {
                    PrintInfo("Running as Admin, Searching Across System for App Package");

                    // https://learn.microsoft.com/uwp/api/windows.management.deployment.packagemanager.findpackage
                    pkg = packageManager.FindPackage(packageFullName);
                }
                else
                {
                    // Empty string == current user
                    // https://learn.microsoft.com/uwp/api/windows.management.deployment.packagemanager.findpackageforuser
                    pkg = packageManager.FindPackageForUser(string.Empty, packageFullName);
                }

                if (pkg is null)
                {
                    PrintError("Unable to find package with full name \"{0}\".", packageFullName);
                    return (int)ExitCode.ArgumentParsingError;
                }

                PrintInfo("Found app: \"{0}\"", pkg.Id.Name);

                // 2. Get the AppList entries for the package (hopefully only one in most cases)
                var entries = await pkg.GetAppListEntriesAsync();

                // TODO: Support selecting an entry to run...
                if (entries.Count > 1)
                {
                    PrintWarning("Package has multiple AppList entries, using the first one found.");
                }
                else if (entries.Count == 0)
                {
                    PrintError("Package \"{0}\" has no AppList entries to run.", packageFullName);
                    return (int)ExitCode.ArgumentParsingError;
                }

                var entry = entries.First();

                PrintInfo("Starting app entry: \"{0}\"", entry.DisplayInfo.DisplayName);
                if (!await entry.LaunchAsync())
                {
                    PrintError("Failed to launch package \"{0}\".", packageFullName);
                    return (int)ExitCode.InspectFailed;
                }

                // 3. Find the process that was just started...
                var processes = Process.GetProcesses();
                Process? targetProcess = null;

                foreach (var process in processes)
                {
                    try
                    {
                        // Arbitrary 30-sec window to find recently started processes
                        if (process.StartTime > DateTime.Now.AddSeconds(-30)
                            && process.TryGetPackageFullName(out var pkgName)
                            && pkgName == packageFullName)
                        {
                            PrintInfo("Found Packaged App Process Running (PID={0})", process.Id);
                            targetProcess = process;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore processes we can't access the start time for.
                    }
                }

                if (targetProcess is null)
                {
                    PrintError("Could not find process for started package \"{0}\".", packageFullName);
                    return (int)ExitCode.InspectFailed;
                }

                // 4. Inspect the process (and children)
                // TODO: This is copied from above, we should refactor into a common method for "waiting for UI process".
                PrintInfo("Waiting for UI Idle of app");
                if (targetProcess?.WaitForInputIdle() == true && targetProcess?.Responding == true)
                {
                    // Question: Should we wait first and then check for idle?
                    if (waitTime > 0)
                    {
                        PrintInfo("Waiting an additional {0}ms before inspecting...", waitTime);
                        Thread.Sleep(waitTime);
                    }

                    PrintInfo("Inspecting app...");
                    if (await InspectProcessAsync(targetProcess, outputFilename, cancellationToken))
                    {
                        return (int)ExitCode.Success;
                    }
                }

                return (int)ExitCode.InspectFailed;
            }

            PrintError("Missing command arguments.");
            await command.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }
}
