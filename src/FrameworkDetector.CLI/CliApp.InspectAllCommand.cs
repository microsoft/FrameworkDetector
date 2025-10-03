// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectAllCommand()
    {
        Option<string?> outputFolderOption = new("--outputFolder", "-o")
        {
            Description = "Save the inspection reports as JSON to the given folder name. Each file will be named by the process id.",
        };

        Option<string?> outputFileTemplateOption = new("--outputFileTemplate")
        {
            Description = 
                """
                The output file template, default is '{appName}.json'.

                Supported tokens:
                    {appName}            - The package full name (if available) otherwise the process name
                    {packageFullName}    - The package full name (if available)
                    {processId}          - The process ID
                    {processName}        - The process name
                    {version}            - The version of the tool
                """,
            
        };

        // See: https://learn.microsoft.com/dotnet/api/system.diagnostics.process.mainwindowhandle
        Option<bool?> filterWindowProcessesOption = new("--filterWindowProcesses")
        {
            Description = "Filters processes by those that are more likely to be applications with a MainWindowHandle. Default is true.",
        };

        var command = new Command("all", "Inspect all running processes")
        {
            filterWindowProcessesOption,
            outputFileTemplateOption,
            outputFolderOption
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

            var outputFileTemplate = parseResult.GetValue(outputFileTemplateOption);
            var outputFolderName = parseResult.GetValue(outputFolderOption);
            var filterProcesses = parseResult.GetValue(filterWindowProcessesOption) ?? true;

            // Create output folder (if specified) for output
            if (!string.IsNullOrEmpty(outputFolderName) && !System.IO.Directory.Exists(outputFolderName))
            {
                System.IO.Directory.CreateDirectory(outputFolderName);
            }

            var processes = Process.GetProcesses();
            List<Process> processesToInspect = new();

            // 1. Add all processes if we're not filtering to MainWindows (not default)
            if (!filterProcesses)
            {
                processesToInspect.AddRange(processes);
            }
            else
            {
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            // Wait for the process to be ready (in case it just started).
                            // TODO: If this is too slow we can probably ignore, as we assume that all apps we want to inspect will already be running...
                            process.WaitForInputIdle();
                            processesToInspect.Add(process);
                        }
                    }
                    catch
                    {
                        // Ignore processes we can't access
                    }
                }
            }

            // 2. Run against all the processes (one-by-one for now)
            ExitCode result = ExitCode.Success;
            int count = 0;
            int fails = 0;
            foreach (var process in processesToInspect)
            {
                string? outputFilename = string.IsNullOrEmpty(outputFolderName) ? null : Path.Combine(outputFolderName, FormatFileName(process, outputFileTemplate));
                PrintInfo("Inspecting process {0}({1}) {2:00.0}%", process.ProcessName, process.Id, 100.0 * count++ / processesToInspect.Count);
                if (!await InspectProcessAsync(process, outputFilename, cancellationToken))
                {
                    PrintError("Failed to inspect process {0}({1}).", process.ProcessName, process.Id);
                    // Set error, but continue
                    result = ExitCode.InspectFailed;
                    fails++;
                }
            }

            // 3. Summary
            if (fails == 0)
            {
                PrintInfo("Successfully inspected all {0} processes.", processesToInspect.Count);
            }
            else
            {
                PrintError("Failed to inspect {0}/{1} processes.", fails, processesToInspect.Count);
            }

            return (int)result;
        });

        return command;
    }

    private string FormatFileName(Process process, string? outputFileTemplate)
    {
        outputFileTemplate ??= "{appName}.json";

        bool hasPackageName = process.TryGetPackageFullName(out string? packageFullName);

        return outputFileTemplate
            .Replace("{processId}", process.Id.ToString())
            .Replace("{processName}", process.ProcessName)
            .Replace("{packageFullName}", packageFullName)
            .Replace("{appName}", hasPackageName ? packageFullName : process.ProcessName)
            .Replace("{version}", AssemblyInfo.ToolVersion);
    }
}
