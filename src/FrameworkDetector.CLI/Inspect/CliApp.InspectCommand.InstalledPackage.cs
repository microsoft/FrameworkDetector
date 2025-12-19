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
    private Command GetInspectInstalledPackageSubCommand()
    {
        Argument<string?> packageFullNameArgument = new("packageFullName")
        {
            Description = "The Package Full Name of the installed package to inspect.",
            Arity = ArgumentArity.ExactlyOne,
        };
        
        Command appCommand = new("package", "Inspect an installed package")
        {
            packageFullNameArgument,
            OutputFileOption,
            IncludeChildrenOption,
            WaitForInputIdleOption,
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
            TryParseIncludeChildren(parseResult);
            TryParseWaitForInputIdle(parseResult);

            if (!TryParseOutputFile(parseResult))
            {
                PrintError("Invalid output file specified");
                return (int)ExitCode.ArgumentParsingError;
            }

            if (packageFullName is not null)
            {
                PackageManager packageManager = new();

                var package = WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackage(packageFullName) : packageManager.FindPackageForUser(string.Empty, packageFullName);

                if (package is null)
                {
                    PrintError("Could not find installed package with full name: {0}", packageFullName);
                    return (int)ExitCode.ArgumentParsingError;
                }

                if (!await InspectPackageAsync(package, OutputFile, cancellationToken))
                {
                    return (int)ExitCode.InspectFailed;
                }

                return (int)ExitCode.Success;
            }

            return (int)await InvalidArgumentsShowHelpAsync(appCommand);
        });

        return appCommand;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectPackageAsync(Package package, string? outputFilename, CancellationToken cancellationToken)
    {
        var target = $"package {package.DisplayName}";

        try
        {
            PrintInfo("Preparing to inspect {0}...", target);

            var inputs = await InputHelper.GetInputsFromPackageAsync(package, isLoaded: false, cancellationToken);

            PrintInfo("Inspecting {0}:", target);

            return await RunInspectionAsync(target, inputs, outputFilename, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            PrintWarning("Inspection canceled.");
            return false;
        }
    }
}
