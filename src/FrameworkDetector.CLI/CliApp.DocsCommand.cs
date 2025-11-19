// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Parsing;
using YamlDotNet.Serialization;

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

using DocFile = (DocMetadata Metadata, string MarkdownContents);

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
                await PrintFrameworksByIdAsync();
                return (int)ExitCode.Success;
            }
            else
            {
                if (DetectorDocsById.TryGetValue(frameworkId, out var frameworkDoc))
                {
                    PrintInfo("Docs found for \"{0}\":", frameworkId);

                    // Print out metadata table first
                    if (Verbosity >= VerbosityLevel.Normal)
                    {
                        var metadata = frameworkDoc.Metadata;

                        var table = ConsoleTable.From(new KeyValuePair<string, object?>[]
                        {
                            new ("FrameworkId", metadata.FrameworkId),
                            new ("Title", metadata.Title),
                            new ("Description", metadata.Description),
                            new ("Category", metadata.Category),
                            new ("Keywords", metadata.Keywords),
                            new ("Source", metadata.Source),
                            new ("Website", metadata.Website),
                            new ("Author", metadata.Author),
                            new ("Date", string.Format("{0:MM/dd/yyyy}", metadata.Date)),
                            new ("Status", metadata.Status),
                        });

                        table.SetMaxWidthBasedOnColumn(2);
                        table.Write(Format.MarkDown);
                    }

                    // Print rest of the markdown document
                    PrintMarkdown(frameworkDoc.MarkdownContents);
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
                await PrintFrameworksByIdAsync();
                return (int)ExitCode.ArgumentParsingError;
            }
        });

        return command;
    }

    private async Task PrintFrameworksByIdAsync()
    {
        // TODO: Maybe tailor table display on verbosity?
        var table = new ConsoleTable("FrameworkId",
                                     "Title",
                                     "Status",
                                     "Updated",
                                     "Website");

        table.Options.EnableCount = false;

        foreach (var (frameworkId, doc) in DetectorDocsById
            .OrderBy(d => d.Key))
        {
            await Task.Yield();
            table.AddRow(doc.Metadata.FrameworkId,
                         doc.Metadata.Title,
                         doc.Metadata.Status switch
                         {
                             DocStatus.Detectable => "✅",
                             DocStatus.Experimental => "🧪",
                             DocStatus.Placeholder => "🟥",
                             _ => "?"
                         },
                         string.Format("{0:MM/dd/yy}", doc.Metadata?.Date),
                         (doc.Metadata?.Website ?? doc.Metadata?.Source)?.Replace("https://", ""));
        }

        Console.WriteLine();
        table.Write(Format.MarkDown);

        Console.WriteLine("✅ Detectable, 🧪 Experimental, 🟥 Placeholder");
    }

    // Key is lowercase FrameworkId with associated DocFile
    private Dictionary<string, DocFile> DetectorDocsById
    {
        get
        {
            if (_detectorDocsById is null)
            {
                // Docs: https://github.com/aaubry/YamlDotNet/wiki/Serialization.Deserializer
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                    .Build();

                _detectorDocsById = new Dictionary<string, DocFile>();

                foreach (var resourceName in AssemblyInfo.ToolAssembly.GetManifestResourceNames())
                {
                    var filename = Path.GetFileName(resourceName);
                    if (Path.GetExtension(filename) == ".md")
                    {
                        var docStream = AssemblyInfo.ToolAssembly.GetManifestResourceStream(resourceName);

                        if (docStream is not null)
                        {
                            using var reader = new StreamReader(docStream);

                            DocMetadata? metadata = null;

                            var contents = reader.ReadToEnd();
                            var parts = contents.Split("---", StringSplitOptions.RemoveEmptyEntries);

                            // Try to read YAML Frontmatter of doc
                            if (parts.Length > 1 
                                && parts.FirstOrDefault() is string yamlMetadata)
                            {
                                try
                                {
                                    metadata = deserializer.Deserialize<DocMetadata>(yamlMetadata);

                                    // Ensure we have a FrameworkId
                                    metadata.FrameworkId ??= filename.Split('.')[^2];
                                }
                                catch (Exception ex)
                                {
                                    PrintWarning("Failed to parse doc metadata for {0}: {1}", filename, ex.Message);
                                }
                            }

                            if (metadata is null)
                            {
                                // Assume docs with no YAML are placeholders
                                metadata = new DocMetadata()
                                {
                                    FrameworkId = filename.Split('.')[^2],
                                    Title = "Unknown Title",
                                    Description = "No Description",
                                    Status = DocStatus.Placeholder,
                                };
                            }

                            var detectorId = metadata.FrameworkId!.ToLowerInvariant();
                            _detectorDocsById[detectorId] = (metadata, parts.Last());
                        }
                    }
                }

                // Now loop through detectors to find any without docs and add placeholder entries
                var engine = Services.GetRequiredService<DetectionEngine>();

                foreach (var detector in engine.Detectors.OrderBy(d => d.Info.FrameworkId))
                {
                    var detectorId = detector.Info.FrameworkId.ToLowerInvariant();
                    if (!_detectorDocsById.ContainsKey(detectorId))
                    {
                        // If we have a Detector with no docs, then we're in the experimental phase as we have some sort of code running, it just may not be accurate across all scenarios yet.
                        var placeholderMetadata = new DocMetadata()
                        {
                            FrameworkId = detector.Info.FrameworkId,
                            Title = detector.Info.Name,
                            Description = detector.Info.Description,
                            Status = DocStatus.Experimental,
                        };

                        _detectorDocsById[detectorId] = (placeholderMetadata, $"# {detector.Info.Name}\n\n_Docs not yet written for this detector. Results for this detector should not be relied upon._");
                    }
                }
            }
            return _detectorDocsById;
        }
    }

    private Dictionary<string, DocFile>? _detectorDocsById = null;
}
