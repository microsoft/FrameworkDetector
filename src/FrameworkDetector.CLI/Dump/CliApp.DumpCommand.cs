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
    private Command GetDumpCommand()
    {
        // Inspect will be a grouping of sub-commands for the different types of inputs we may expect (process, package, exe, etc...)
        Command command = new("dump", "Dump the metadata of an application for later framework detection")
        {
        };
        command.TreatUnmatchedTokensAsErrors = true;

        // Add all subcommands for processing different input types
        // TODO: Not sure if better way to handle global options for now, at least we have one currently...
        command.Subcommands.Add(GetDumpExeSubCommand());
        command.Subcommands.Add(GetDumpInstalledPackageSubCommand());
        command.Subcommands.Add(GetDumpProcessSubCommand());
        command.Subcommands.Add(GetDumpAllProcessesSubCommand());

        return command;
    }

    private async Task<bool> RunDumpAsync(string label, IReadOnlyList<IInputType> inputs, string? outputFilename, CancellationToken cancellationToken)
    {
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
