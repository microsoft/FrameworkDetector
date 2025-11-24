// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System.IO;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectExeSubCommand(Option<string?> outputFileOption)
    {
        Argument<string?> pathToExeArgument = new("path")
        {
            Description = "The full path to the executable file on disk to inspect.",
            Arity = ArgumentArity.ExactlyOne,
        };
        
        Command exeCommand = new("exe", "Inspect an executable file on disk")
        {
            pathToExeArgument
        };

        exeCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
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

            var pathToExe = parseResult.GetValue(pathToExeArgument);
            var outputFilename = parseResult.GetValue(outputFileOption);

            if (pathToExe is not null)
            {
                if (!File.Exists(pathToExe))
                {
                    PrintError("Could not location file at path: {0}", pathToExe);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (await InspectExeAsync(pathToExe, outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }

            PrintError("Missing command arguments.");
            await exeCommand.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return exeCommand;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectExeAsync(string pathToExe, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"exe {pathToExe}";

        PrintInfo("Preparing to inspect {0}...", target);

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.Write($"Inspecting {target}:");
        }

        var exeDataSources = new List<ProcessDataSource>() { }; // new ProcessDataSource(process)

        DataSourceCollection sources = new(exeDataSources.ToArray());

        return await RunInspectionAsync(target, sources, outputFilename, cancellationToken);
    }
}
