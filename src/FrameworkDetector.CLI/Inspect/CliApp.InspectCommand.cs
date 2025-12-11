// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using FrameworkDetector.Inputs;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects an application on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectCommand()
    {
        // Inspect will be a grouping of sub-commands for the different types of inputs we may expect (process, package, exe, etc...)
        Command command = new("inspect", "Inspect an application to determine what frameworks it depends on")
        {
        };
        command.TreatUnmatchedTokensAsErrors = true;

        // Add all subcommands for processing different input types
        // TODO: Not sure if better way to handle global options for now, at least we have one currently...
        command.Subcommands.Add(GetInspectExeSubCommand());
        command.Subcommands.Add(GetInspectInstalledPackageSubCommand());
        command.Subcommands.Add(GetInspectProcessSubCommand());
        command.Subcommands.Add(GetInspectAllProcessesSubCommand());

        return command;
    }

    private async Task<bool> RunInspectionAsync(string label, IReadOnlyList<IInputType> inputs, string? outputFilename, CancellationToken cancellationToken)
    {
        DetectionEngine engine = Services.GetRequiredService<DetectionEngine>();
        engine.DetectionProgressChanged += (s, e) =>
        {
            if (Verbosity > VerbosityLevel.Quiet)
            {
                Console.Write($"\rInspecting {label}: {e.Progress:000.0}%");
            }
        };

        ToolRunResult result = await engine.DetectAgainstInputsAsync(inputs, cancellationToken, ArgumentMetadata);

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.WriteLine();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            PrintWarning("Inspection was canceled prior to completion.");
            Console.WriteLine();
        }

        PrintResult(result);

        TrySaveOutput(result, outputFilename);

        // TODO: Return false on failure
        return true;
    }
}
