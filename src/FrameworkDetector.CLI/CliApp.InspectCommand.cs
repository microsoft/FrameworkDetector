// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectCommand()
    {
        Option<string?> outputFileOption = new("--outputFile", "-o")
        {
            Description = "Save the inspection report as JSON to the given filename.",
            Recursive = true, // Works for all sub-commands
        };

        // Inspect will be a grouping of sub-commands for the different types of inputs we may expect (process, package, exe, etc...)
        Command command = new("inspect", "Inspect an application to determine what frameworks it depends on.")
        {
            outputFileOption
        };
        command.TreatUnmatchedTokensAsErrors = true;

        // Add all subcommands for processing different input types
        // TODO: Not sure if better way to handle global options for now, at least we have one currently...
        command.Subcommands.Add(GetInspectProcessSubCommand(outputFileOption));

        return command;
    }
}
