// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Linq;

using System.CommandLine;
using System.CommandLine.Parsing;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

using DocFile = (DocMetadata Metadata, string MarkdownContents);

public partial class CliApp
{
    /// <summary>
    /// A command which outputs a list of recently installed application packages.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetListCommand()
    {
        // https://learn.microsoft.com/dotnet/standard/commandline/syntax#arguments
        Argument<int?> numberArgument = new("number")
        {
            // TODO: We could probably create an 'all' flag instead later...
            Description = "Optionally, specify number of packages to return, default '5'. '0' or negative for all.",
            // Make it optional so we'll list out top 5 if unspecified
            Arity = ArgumentArity.ZeroOrOne,
            DefaultValueFactory = parseResult => 5,
        };

        var command = new Command("list", "List the recent packages installed for the current user (or system if admin).")
        {
            numberArgument,
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

            var number = parseResult.GetValue(numberArgument);

            if (number is not null)
            {
                PackageManager packageManager = new();

                IEnumerable<Package> packages = [];

                if (number.Value > 0)
                {
                    Console.WriteLine($"Listing {number.Value} most recent packages installed:");
                    packages = (WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackages() : packageManager.FindPackagesForUser(string.Empty))
                        .OrderByDescending(p => p.InstalledDate)
                        .Take(number.Value);
                }
                else
                {
                    Console.WriteLine("Listing all packages installed:");
                    // Weird cast issue with trying to do this later, so just separate out for now.
                    packages = (WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackages() : packageManager.FindPackagesForUser(string.Empty))
                        .OrderByDescending(p => p.InstalledDate);
                }

                Console.WriteLine();

                foreach (var package in packages)
                {
                    Console.WriteLine($"FullName:  {package.Id.FullName}");
                    Console.WriteLine($"Installed: {package.InstalledDate}");

                    var installPath = "Unknown";
                    try
                    {
                        installPath = package.InstalledLocation.Path;
                    }
                    catch (System.IO.FileNotFoundException) { }
                    Console.WriteLine($"Location:  {installPath}");

                    var entries = await package.GetAppListEntriesAsync();
                    if (entries is not null && entries.Count > 0)
                    {
                        Console.WriteLine("Applications [AUMID] DisplayName:");
                        foreach (var entry in entries)
                        {
                            Console.WriteLine($"\t[{entry.AppUserModelId}] {entry.DisplayInfo.DisplayName}");
                        }
                    }

                    Console.WriteLine();
                }

                return (int)ExitCode.Success;
            }

            return (int)await InvalidArgumentsShowHelpAsync(command);
        });

        return command;
    }
}
