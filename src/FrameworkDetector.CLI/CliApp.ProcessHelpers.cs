// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;

using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    private string FormatFileName(Process process, string? outputFileTemplate)
    {
        outputFileTemplate ??= "{appName}.json";

        bool hasPackageName = process.TryGetPackageFullName(out string? packageFullName);

        return outputFileTemplate
            .Replace("{processId}", process.Id.ToString())
            .Replace("{processName}", process.ProcessName)
            .Replace("{packageFullName}", packageFullName)
            .Replace("{appName}", hasPackageName ? packageFullName : process.ProcessName)
            .Replace("{version}", AssemblyInfo.ToolVersion);
    }

    private bool TryGetSingleProcessByName(string processName, out Process? result)
    {
        PrintInfo("Searching for process named \"{0}\"...", processName);

        var processes = Process.GetProcessesByName(processName);

        if (processes.Length == 0)
        {
            PrintError("Unable to find process with name \"{0}\".", processName);
        }
        else if (processes.Length == 1)
        {
            PrintInfo("Found process {0}({1}).\n", processes[0].ProcessName, processes[0].Id);
            result = processes[0];
            return true;
        }
        else
        {
            PrintWarning("More than one process with name \"{0}\":", processName);
            foreach (var process in processes)
            {
                PrintWarning("  {0}({1})", process.ProcessName, process.Id);
            }

            if (processes.TryGetRootProcess(out var rootProcess) && rootProcess is not null)
            {
                PrintInfo("Determined root process {0}({1}).\n", rootProcess.ProcessName, rootProcess.Id);
                result = rootProcess;
                return true;
            }
            else
            {
                PrintWarning("Unable to determine a root process, defaulting to the lowest PID.");
                result = processes.OrderBy(p => p.Id).First();
                return true;
            }
        }

        result = default;
        return false;
    }

    private bool TryKillProcess(Process? process)
    {
        try
        {
            if (process is not null)
            {
                PrintInfo("Trying to kill process {0}({1})...", process.ProcessName, process.Id);
                process.Kill();
                PrintInfo("Process killed.");
                return true;
            }
        }
        catch
        {
            PrintError("Unable to kill process.");
        }

        return false;
    }
}
