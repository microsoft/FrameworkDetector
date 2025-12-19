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
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            if (!TryParseOutputFile(parseResult))
            {
                PrintError("Invalid output file specified");
                return (int)ExitCode.ArgumentParsingError;
            }

            if (pathToExe is not null)
            {
                FileInfo fileInfo = new FileInfo(pathToExe);
                if (!fileInfo.Exists)
                {
                    PrintError("Could not location file at path: {0}", pathToExe);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (!await DumpExeAsync(fileInfo, OutputFile, cancellationToken))
                {
                    return (int)ExitCode.DumpFailed;
                }

                return (int)ExitCode.Success;
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

        try
        {
            PrintInfo("Preparing to dump {0}...", target);

            var inputs = await InputHelper.GetInputsFromExecutableAsync(fileInfo, isLoaded: false, cancellationToken);

            PrintInfo("Dumping {0}:", target);

            return await RunDumpAsync(target, inputs, outputFilename, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            PrintWarning("Dump canceled.");
            return false;
        }
    }
}
