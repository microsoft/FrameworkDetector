// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ConsoleTables;
using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single loose executable file on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectJsonSubCommand()
    {
        Option<bool> redetectOption = new("--redetect", "-r")
        {
            Description = "Use the input data only from the JSON results file and re-run through the detection engine before displaying/outputting results.",
            Arity = ArgumentArity.Zero, // Note: Flag only, no value
        };

        Argument<string?> pathToJsonArgument = new("path")
        {
            Description = "The full path to the json file on disk to inspect.",
            Arity = ArgumentArity.ExactlyOne,
        };
        
        Command jsonCommand = new("json", "Display a prior results JSON file's results from disk (will not re-output unless redetecting).")
        {
            pathToJsonArgument,
            redetectOption,
            OutputFileOption,
            PluginFilesOption,
        };

        jsonCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
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

            var redetect = parseResult.GetValue(redetectOption);
            var pathToJson = parseResult.GetValue(pathToJsonArgument);

            // This is only relevant really if we redetect the inputs, as otherwise would expect to just re-output the same input data/results...
            if (!TryParseOutputFile(parseResult) && redetect)
            {
                PrintError("Invalid output file specified");
                return (int)ExitCode.ArgumentParsingError;
            }

            if ((OutputFile is not null || parseResult.GetValue(OutputFileOption) is not null) && !redetect)
            {
                PrintWarning("Output File is unused if not redetecting results.");
            }

            if (!TryInitializeFrameworkDetectorServices(parseResult))
            {
                PrintError("Unable to initialize FrameworkDetector services.");
                return (int)ExitCode.ArgumentParsingError;
            }

            if (pathToJson is not null)
            {
                FileInfo fileInfo = new FileInfo(pathToJson);
                if (!fileInfo.Exists)
                {
                    PrintError("Could not location file at path: {0}", pathToJson);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (!await InspectJsonAsync(fileInfo, redetect, OutputFile, cancellationToken))
                {
                    return (int)ExitCode.InspectFailed;
                }

                return (int)ExitCode.Success;
            }

            return (int)await InvalidArgumentsShowHelpAsync(jsonCommand);
        });

        return jsonCommand;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectJsonAsync(FileInfo fileInfo, bool redetect, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"json {fileInfo.FullName}";

        try
        {
            PrintInfo("Preparing to inspect {0}...", target);

            ToolRunResult? toolRunResult = null;
            using (var fileStream = fileInfo.OpenRead())
            {
                try
                {
                    toolRunResult = JsonSerializer.Deserialize<ToolRunResult>(fileStream, DetectorJsonSerializerOptions.Options);
                }
                catch (Exception e)
                {
                    PrintError("Problem parsing JSON result file: {0}", e.Message);
                    return false;
                }
            }

            if (toolRunResult is null)
            {
                PrintError("Unknown error: Could not read JSON result file {0}", fileInfo.FullName);
                return false;
            }

            // Output info about original run
            if (Verbosity >= VerbosityLevel.Normal)
            {
                var table = new ConsoleTable("Property", "Value");
                table.Options.EnableCount = false;

                table.AddRow("Tool", toolRunResult.ToolName);
                table.AddRow("Version", toolRunResult.ToolVersion);
                table.AddRow("Arguments", toolRunResult.ToolArguments);
                table.AddRow("Timestamp", toolRunResult.Timestamp);
                table.AddRow("# Inputs", toolRunResult.Inputs.Values.Count());
                table.AddRow("# Results", toolRunResult.DetectorResults.Count());

                table.SetMaxWidthBasedOnColumn(1);
                Console.WriteLine();
                table.Write(Format.MarkDown);
            }

            // Are we re-running the detection? (i.e. against original re-serialized input data)
            if (redetect)
            {
                PrintInfo("Reinspecting {0}:", target);

                var inputs = toolRunResult.Inputs.Values.SelectMany(v => v).OfType<IInputType>().ToList();

                return await RunInspectionAsync(target, inputs, outputFilename, cancellationToken);
            }
            // Or just re-outputting the stored results (easy)
            else
            {
                PrintResult(toolRunResult);

                return true;
            }
        }
        catch (OperationCanceledException)
        {
            PrintWarning("Inspection canceled.");
            return false;
        }
    }
}
