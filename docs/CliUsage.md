---
id: CliUsage
title: FrameworkDetector.CLI Usage
description: How to use FrameworkDetector.CLI.
keywords: Framework Detector
ms.date: 02/03/2026
author: jonthysell
---

# FrameworkDetector.CLI Usage

You can find the complete usage help for FrameworkDetector.CLI  (the CLI) by calling it with `-h`, `-?`, or `--help`:

```
Description:
  Framework Detector

Usage:
  FrameworkDetector.CLI [command] [options]

Options:
  -v, --verbosity <detailed|diagnostic|minimal|normal|quiet>  Set the verbosity level of printed output. If no
                                                              additional value specified after '-v', defaults to
                                                              'diagnostic'. [default: normal]
  -?, -h, --help                                              Show help and usage information
  --version                                                   Show version information

Commands:
  docs <frameworkId>  Get documentation for how a particular framework is detected. If no frameworkId specified, lists
                      the available frameworks.
  dump                Dump the metadata of an application for later framework detection
  inspect             Inspect an application to determine what frameworks it depends on
  list <number>       List the recent packages installed for the current user (or system if admin). [default: 5]
  run                 Run a given a process/package and inspect it
```

## Commands

The CLI has several main commands. Detailed help for each command can be found by running `FrameworkDetector.CLI.exe <command> --help`.

### Inspect Command

The `inspect` command is the primary command for the CLI, and can be used to perform framework detection on a variety of app targets. To inspect a running process, you can do so by calling `inspect process` with either the Process ID (PID in the Task Manager):

```ps
FrameworkDetector.CLI.exe inspect process -id 123
```

*OR* with the Process Name (usually the name of the executable without `.exe`):

```ps
FrameworkDetector.CLI.exe inspect process -name myapp
```

If you want to inspect every running process on the system, you can call `inspect processes`:

```ps
FrameworkDetector.CLI.exe inspect processes
```

By default, `inspect processes` filters to only GUI processes (aka those which have a Main Window Handle or visible child window). You can use `--filterWindowProcesses false` to inspect *all* processes. In either case, some processes may require admin rights to inspect.

If you want to inspect an app without running it, you can call `inspect exe` with the path to the app's exe file:

```ps
FrameworkDetector.CLI.exe inspect exe path\to\myapp.exe
```

*OR* if you want to inspect an installed app package (typically from the Microsoft Store) without running it, you can call `inspect package` with the app's [Package Full Name](https://learn.microsoft.com/windows/apps/desktop/modernize/package-identity-overview) (or PFN):

```ps
FrameworkDetector.CLI.exe inspect package Microsoft.WindowsNotepad_11.2510.13.0_x64__8wekyb3d8bbwe
```

You can use the `list` command (see below) to find the PFNs recently installed packages.

**Note:** Results from inspecting an app without running it will be less accurate than inspecting it as a running process.

### Dump Command

The `dump` command essentially performs the "first half" of the `inspect` command - it does all the work to gather the useful information of a target app but does not run any of the detectors. If you're looking to write a new detector (or improve one) and need to understand what inputs get created, you can use the `dump` command to get a comprehensive view of all data available the target.

For example, to dump all of the inputs for a running Notepad process, you could call:

```ps
FrameworkDetector.CLI.exe dump process -n Notepad
```

The `dump` command takes all of the same options as the `inspect`, so you can for example, also output to a JSON file using the `-o` flag.

### Run Command

The `run` command allows the CLI to launch a target app and then immediately inspect it. For example, to ask the CLI to launch a particular exe in order to inspect it, call `run` with the `-exe` option:

```ps
FrameworkDetector.CLI.exe run -exe "C:\Path\To\MyApp.exe"
```

Similarly, if you wanted to run an installed app package (again typically from the Microsoft Store), you can also call `run` with either the app's [Package Full Name](https://learn.microsoft.com/windows/apps/desktop/modernize/package-identity-overview) (or PFN) by using the `-pkg` option:

```ps
FrameworkDetector.CLI.exe run -pkg Microsoft.WindowsNotepad_11.2510.13.0_x64__8wekyb3d8bbwe
```

*OR* the app's [Application User Model Id](https://learn.microsoft.com/windows/configuration/store/find-aumid) (or AUMID) by using the `-aumid` option:

```ps
FrameworkDetector.CLI.exe run -aumid Microsoft.WindowsNotepad_8wekyb3d8bbwe!App
```

Tou can use the `list` command (see below) to find the PFNs and AUMIDs of recently installed packages.

### List Command

The `list` command lists the recently installed application packages on the machine:

```ps
FrameworkDetector.CLI.exe list
```

This utility is provided to help users find the Package Full Names and Application User Model IDs needed by the other commands. You can pass the number of packages to return (sorted from most recent to least recent) or `-1` to return all installed packages.

### Docs Command

You can view the list of available detectors (and detailed documentation if available) by using the `docs` command:

```ps
FrameworkDetector.CLI.exe docs
```

This will provide a table of detector ids, names, and whether documentation is available. Add the id as an argument to retrieve the detailed specification of how that particular framework is detected (if available), for example:

```ps
FrameworkDetector.CLI.exe docs WinUI3
```

## Common Command Options

There are some options that are common to most commands.

### Output to JSON

Use the `-o` parameter to specify an output file when calling `inspect`, `dump`, or `run` for a single target, which will follow the [schema described here](./OutputJsonSchema.md). For example, creating the output file when inspecting a process:

```ps
FrameworkDetector.CLI.exe inspect process -n Notepad -o myresults.json
```

When calling `inspect`, `dump`, or `run` with multiple targets (aka `inspect processes`) then a file will be created for each target in the folder specified by the `-o` parameter. You can then use the `--outputFileTemplate` parameter to customize the resulting file names.

Use `inspect processes --help` to see the available replacement options (case-sensitive). For example, to output files for inspecting multiple processes, you might call:

```ps
FrameworkDetector.CLI.exe inspect processes -o results\ --outputFileTemplate "{processName}_{processId}.json"
```

### Verbosity

You can control the verbosity of the output with the `--verbosity` (or `-v`) parameter. Options are:

- `quiet`: Only show errors
- `minimal`: Show errors and essential information
- `normal`: Console shows found frameworks only (default when unspecified)
- `detailed`: Console shows found and unfound frameworks
- `diagnostic`: Show all information, including verbose diagnostic output of checks in table

If you specify `-v` without a value, it defaults to `diagnostic`.
