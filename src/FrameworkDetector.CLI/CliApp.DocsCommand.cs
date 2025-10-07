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
        Option<bool?> listOption = new("--list", "-l")
        {
            Description = "List the available framework ids",
        };

        Option<string?> frameworkIdOption = new("--frameworkId", "-i")
        {
            Description = "Get the doc for the given id",
        };

        var command = new Command("docs", "Get documentation for how a particular framework is detected.")
        {
            listOption,
            frameworkIdOption,
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

            var list = parseResult.GetValue(listOption);
            var frameworkId = parseResult.GetValue(frameworkIdOption);

            if (list is not null && list.Value)
            {
                PrintFrameworksById();
                return (int)ExitCode.Success;
            }
            else if (frameworkId is not null)
            {
                if (FrameworkDocsById.TryGetValue(frameworkId, out var frameworkDoc))
                {
                    PrintInfo("Docs found for \"{0}\":", frameworkId);

                    PrintMarkdown(frameworkDoc);
                    return (int)ExitCode.Success;
                }

                PrintError("Unable to find docs for \"{0}\"", frameworkId);
                PrintError("Try running with {0} to get a list of supported frameworks", listOption.Aliases.FirstOrDefault() ?? "");
                return (int)ExitCode.InspectFailed;
            }

            PrintError("Missing command arguments.");
            await command.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
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
            var hasDocs = FrameworkDocsById.ContainsKey(frameworkId);

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

                            var frameworkId = filename.Split('.')[^2];
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
