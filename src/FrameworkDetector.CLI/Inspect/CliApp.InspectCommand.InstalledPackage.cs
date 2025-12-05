// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.Management.Deployment;

using FrameworkDetector.Inputs;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single installed application package on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectInstalledPackageSubCommand(Option<string?> outputFileOption)
    {
        Argument<string?> packageFullNameArgument = new("packageFullName")
        {
            Description = "The Package Full Name of the application to inspect.",
            Arity = ArgumentArity.ExactlyOne,
        };
        
        Command appCommand = new("app", "Inspect an installed application package")
        {
            packageFullNameArgument
        };

        appCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
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

            var packageFullName = parseResult.GetValue(packageFullNameArgument);
            var outputFilename = parseResult.GetValue(outputFileOption);

            if (packageFullName is not null)
            {
                PackageManager packageManager = new();

                var package = WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackage(packageFullName) : packageManager.FindPackageForUser(string.Empty, packageFullName);

                if (package is null)
                {
                    PrintError("Could not find installed package with full name: {0}", packageFullName);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (await InspectPackageAsync(package, outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }

            PrintError("Missing command arguments.");
            await appCommand.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return appCommand;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectPackageAsync(Package package, string? outputFilename, CancellationToken cancellationToken)
    {
        var target = $"app {package.DisplayName}";

        PrintInfo("Preparing to inspect {0}...", target);

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.Write($"Inspecting {target}:");
        }

        var inputs = await InputHelper.GetInputsFromPackageAsync(package, isLoaded: false, cancellationToken);

        return await RunInspectionAsync(target, inputs, outputFilename, cancellationToken);
    }
}
