// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    // Value here represents the default verbosity when the -v option is specified without argument; otherwise it is defined below as normal in the DefaultValueFactory.
    protected VerbosityLevel Verbosity { get; private set; } = VerbosityLevel.Normal;

    // TODO: Figure out how to accept the various shortnames, not specified in help:
    // https://learn.microsoft.com/dotnet/standard/commandline/syntax#parse-errors
    // https://learn.microsoft.com/dotnet/standard/commandline/design-guidance#the---verbosity-option
    protected Option<string?> VerbosityOption
    {
        get
        {
            if (_verbosityOption is null)
            {
                _verbosityOption = new("--verbosity", "-v")
                {
                    Description = "Set the verbosity level of printed output. If no additional value specified after '-v', defaults to 'diagnostic'.",
                    DefaultValueFactory = parseResult => "normal", // Note: Default value for standard running mode
                    Recursive = true, // Note: Makes this a global command when added to the Root Command
                    Arity = ArgumentArity.ZeroOrOne, // Note: Let's us specify -v without a value, uses C# default in that case (default above used when not specified at all)
                };
                _verbosityOption.AcceptOnlyFromAmong("quiet", "minimal", "normal", "detailed", "diagnostic");
            }
            return _verbosityOption;
        }
    }
    private Option<string?>? _verbosityOption = null;

    protected bool TryParseVerbosity(ParseResult parseResult)
    {
        try
        {
            var verbosityString = parseResult.GetValue(VerbosityOption);

            if (Enum.TryParse(verbosityString, true, out VerbosityLevel verbosity))
            {
                Verbosity = verbosity;
                PrintInfo("Verbosity set to {0} - Running as Admin: {1}", Verbosity, WindowsIdentity.IsRunningAsAdmin);

                return true;
            }

            return true;
        }
        catch { }

        return false;
    }

    protected bool IncludeChildren
    {
        get
        {
            if (_includeChildren is null)
            {
                throw new ArgumentNullException(nameof(IncludeChildren), $"{nameof(IncludeChildren)} was not set, did you forget to call {nameof(TryParseIncludeChildren)}?");
            }
            return _includeChildren.Value;
        }
    }
    private bool? _includeChildren = null;

    protected Option<bool> IncludeChildrenOption = new("--includeChildren", "-c")
    {
        Description = "Include the child processes of the target process when inspecting/dumping. (May require running with elevation.)",
        Arity = ArgumentArity.Zero, // Note: Flag only, no value
    };

    protected bool TryParseIncludeChildren(ParseResult parseResult)
    {
        try
        {
            _includeChildren = parseResult.GetValue(IncludeChildrenOption);
            return true;
        }
        catch { }

        return false;
    }

    protected bool WaitForInputIdle
    {
        get
        {
            if (_waitForInputIdle is null)
            {
                throw new ArgumentNullException(nameof(WaitForInputIdle), $"{nameof(WaitForInputIdle)} was not set, did you forget to call {nameof(TryParseWaitForInputIdle)}?");
            }
            return _waitForInputIdle.Value;
        }
    }
    private bool? _waitForInputIdle = null;

    protected Option<bool> WaitForInputIdleOption = new("--waitForInputIdle", "-w")
    {
        Description = "Wait for input idle of process before inpspecting/dumping.",
        Arity = ArgumentArity.Zero, // Note: Flag only, no value
    };

    protected bool TryParseWaitForInputIdle(ParseResult parseResult)
    {
        try
        {
            _waitForInputIdle = parseResult.GetValue(WaitForInputIdleOption);
            return true;
        }
        catch { }

        return false;
    }

    protected string? OutputFile { get; private set; } = null;

    protected Option<string?> OutputFileOption = new("--outputFile", "-o")
    {
        Description = "Output the report as JSON to the given filename.",
    };

    protected bool TryParseOutputFile(ParseResult parseResult)
    {
        try
        {
            OutputFile = parseResult.GetValue(OutputFileOption);
            return true;
        }
        catch { }

        return false;
    }

}
