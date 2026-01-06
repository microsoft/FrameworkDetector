// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects all running processes on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectAllProcessesSubCommand()
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
            Description = "Filters processes by those that are more likely to be applications with a MainWindowHandle or a visible child window. Default is true.",
        };

        var command = new Command("processes", "Inspect all running processes")
        {
            filterWindowProcessesOption,
            outputFileTemplateOption,
            outputFolderOption,
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

            var outputFileTemplate = parseResult.GetValue(outputFileTemplateOption);
            var outputFolderName = parseResult.GetValue(outputFolderOption);
            var filterProcesses = parseResult.GetValue(filterWindowProcessesOption) ?? true;
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            // Create output folder (if specified) for output
            if (!string.IsNullOrEmpty(outputFolderName) && !Directory.Exists(outputFolderName))
            {
                Directory.CreateDirectory(outputFolderName);
            }

            var processes = Process.GetProcesses();
            List<Process> processesToInspect = new();

            // 1. Add all inspectable processes (filtering for processes with a GUI window if requested)
            foreach (var process in processes)
            {
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested)
                {
                    PrintWarning("Inspect processes canceled.");
                    return (int)ExitCode.InspectFailed;
                }

                var isAccessible = process.IsAccessible();
                var hasGUI = process.HasGUI();

                if (isAccessible && (!filterProcesses || (filterProcesses && hasGUI)))
                {
                    // Add processes that are accessible and meet filtering
                    PrintInfo("Planning to inspect process {0}({1})", process.ProcessName, process.Id);
                    processesToInspect.Add(process);
                }
                else if (!isAccessible && (!filterProcesses || (filterProcesses && hasGUI)))
                {
                    // Warn for processes that aren't accessible and meet filtering
                    PrintWarning("Cannot access process {0}({1}) to inspect" + (!WindowsIdentity.IsRunningAsAdmin ? ", try running as Administrator." : "."), process.ProcessName, process.Id);
                }

                // Ignore remaining processes
            }

            if (processesToInspect.Count == 0)
            {
                PrintError("No processes to inspect.");
                return (int)ExitCode.InspectFailed;
            }

            // 2. Run against all the processes (one-by-one for now)
            int current = 0;
            int successes = 0;
            foreach (var process in processesToInspect.OrderBy(p => p.ProcessName))
            {
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested)
                {
                    PrintWarning("Inspect processes canceled.");
                    break;
                }

                string? outputFilename = string.IsNullOrEmpty(outputFolderName) ? null : Path.Combine(outputFolderName, FormatFileName(process, outputFileTemplate));
                PrintInfo("Inspecting process {0} [{1}]({2}) {3:00.0}%", process.MainWindowTitle, process.ProcessName, process.Id, 100.0 * current++ / processesToInspect.Count);
                if (await InspectProcessAsync(process, outputFilename, cancellationToken))
                {
                    successes++;
                }
                else
                {
                    PrintError("Failed to inspect process {0}({1}).", process.ProcessName, process.Id);
                }
            }

            // 3. Summary
            PrintInfo("Successfully inspected {0}/{1} ({2:00.0}%) processes.", successes, processesToInspect.Count, 100.0 * successes / processesToInspect.Count);

            return (int)(successes == current ? ExitCode.Success : ExitCode.InspectFailed);
        });

        return command;
    }
}
