// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which outputs the documentation for how a particular framework is detected.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetDocsCommand()
    {
        // https://learn.microsoft.com/dotnet/standard/commandline/syntax#arguments
        Argument<string?> frameworkIdArgument = new("frameworkId")
        {
            Description = "Get the doc for the given id",
            // Make it optional so we'll list out frameworks if not specified
            Arity = ArgumentArity.ZeroOrOne,
            DefaultValueFactory = parseResult => null,
        };

        var command = new Command("docs", "Get documentation for how a particular framework is detected. If no frameworkId specified, lists the available frameworks.")
        {
            frameworkIdArgument,
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

            var frameworkId = parseResult.GetValue(frameworkIdArgument)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(frameworkId))
            {
                PrintFrameworksById();
                return (int)ExitCode.Success;
            }
            else
            {
                if (FrameworkDocsById.TryGetValue(frameworkId, out var frameworkDoc))
                {
                    PrintInfo("Docs found for \"{0}\":", frameworkId);

                    PrintMarkdown(frameworkDoc);
                    return (int)ExitCode.Success;
                }
                else if (Services.GetRequiredService<DetectionEngine>()
                                 .Detectors
                                 .Any(d => d.Info.FrameworkId.ToLowerInvariant() == frameworkId))
                {
                    PrintWarning("No docs currently written for {0} Detector.", frameworkId);
                    return (int)ExitCode.InspectFailed;
                }
                
                PrintError("Unable to find docs for \"{0}\"", frameworkId);
                PrintError("Available frameworks are:");
                PrintFrameworksById();
                return (int)ExitCode.ArgumentParsingError;
            }
        });

        return command;
    }

    private void PrintFrameworksById()
    {
        var table = new ConsoleTable("FrameworkId",
                                     "Framework Description",
                                     "Docs");

        table.Options.EnableCount = false;
        table.MaxWidth = Console.BufferWidth - 10;

        var engine = Services.GetRequiredService<DetectionEngine>();

        foreach (var detector in engine.Detectors.OrderBy(d => d.Info.FrameworkId))
        {
            var frameworkId = detector.Info.FrameworkId;
            var frameworkDescription = detector.Info.Description;
            var hasDocs = FrameworkDocsById.ContainsKey(frameworkId.ToLowerInvariant());

            table.AddRow(frameworkId,
                         frameworkDescription,
                         hasDocs ? " ✅" : " 🟥");
        }

        Console.WriteLine();
        table.Write(Format.MarkDown);
    }

    private Dictionary<string, string> FrameworkDocsById
    {
        get
        {
            if (_frameworkDocsById is null)
            {
                _frameworkDocsById = new Dictionary<string, string>();

                foreach (var resourceName in AssemblyInfo.ToolAssembly.GetManifestResourceNames())
                {
                    var filename = Path.GetFileName(resourceName);
                    if (Path.GetExtension(filename) == ".md")
                    {
                        var docStream = AssemblyInfo.ToolAssembly.GetManifestResourceStream(resourceName);

                        if (docStream is not null)
                        {
                            using var reader = new StreamReader(docStream);

                            var frameworkId = filename.Split('.')[^2].ToLowerInvariant();
                            _frameworkDocsById[frameworkId] = reader.ReadToEnd();
                        }
                    }
                }
            }
            return _frameworkDocsById;
        }
    }

    private Dictionary<string, string>? _frameworkDocsById = null;
}
