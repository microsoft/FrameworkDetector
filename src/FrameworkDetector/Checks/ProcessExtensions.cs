// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Management;

namespace FrameworkDetector.DetectorChecks;

public static class ProcessExtensions
{
    // Adapted from https://stackoverflow.com/a/38614443
    public static IEnumerable<Process> GetChildProcesses(this Process process, bool recursive = false)
    {
        var children = new ManagementObjectSearcher(
                $"Select * From Win32_Process Where ParentProcessID={process.Id}")
            .Get()
            .Cast<ManagementObject>()
            .Select(mo =>
                Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));

        return recursive ? children.Union(children.Select(c => c.GetChildProcesses(recursive)).SelectMany(x => x)) : children;
    }
}
