// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.Inputs;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which dumps a single loose executable file on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetDumpExeSubCommand()
    {
        Argument<string?> pathToExeArgument = new("path")
        {
            Description = "The full path to the executable file on disk to dump.",
            Arity = ArgumentArity.ExactlyOne,
        };
        
        Command exeCommand = new("exe", "Dump an executable file on disk")
        {
            pathToExeArgument,
            OutputFileOption,
            IncludeChildrenOption,
            WaitForInputIdleOption,
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
            TryParseOutputFile(parseResult);
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            if (pathToExe is not null)
            {
                FileInfo fileInfo = new FileInfo(pathToExe);
                if (!fileInfo.Exists)
                {
                    PrintError("Could not location file at path: {0}", pathToExe);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (await DumpExeAsync(fileInfo, OutputFile, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }

            return (int)await InvalidArgumentsShowHelpAsync(exeCommand);
        });

        return exeCommand;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> DumpExeAsync(FileInfo fileInfo, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"exe {fileInfo.FullName}";

        PrintInfo("Preparing to dump {0}...", target);

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.Write($"Dumping {target}:");
        }

        var inputs = await InputHelper.GetInputsFromExecutableAsync(fileInfo, isLoaded: false, cancellationToken);

        return await RunDumpAsync(target, inputs, outputFilename, cancellationToken);
    }
}
