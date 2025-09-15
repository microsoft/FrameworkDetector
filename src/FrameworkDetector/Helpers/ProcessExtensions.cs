// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Windows.Win32.Foundation;

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

    public static IEnumerable<ProcessWindowMetadata> GetActiveWindowMetadata(this Process process)
    {
        var windows = new HashSet<ProcessWindowMetadata>();

        var applicationFrameHosts = Process.GetProcessesByName("ApplicationFrameHost");

        bool addWindow(HWND hwnd)
        {
            try
            {
                var className = hwnd.GetClassName();
                var windowText = hwnd.GetWindowText();

                if (className is not null || windowText is not null)
                {
                    windows.Add(new ProcessWindowMetadata(className,
                                                          windowText,
                                                          hwnd.IsWindowVisible()));
                }
            }
            catch { }

            return true;
        }

        HWND.EnumWindows((HWND hwnd) =>
        {
            try
            {
                var threadID = hwnd.GetWindowThreadProcessId(out var processID);
                if (threadID > 0)
                {
                    var processMatch = processID == process.Id;
                    if (processMatch)
                    {
                        // Add the top-level windows for the process
                        addWindow(hwnd);
                    }

                    if (processMatch || applicationFrameHosts.Where(p => p.Id == processID).Any())
                    {
                        // Add child windows plus any for the process that are currently parented with with ApplicationFrameHost
                        hwnd.EnumChildWindows(child =>
                        {
                            try
                            {
                                var threadID = child.GetWindowThreadProcessId(out var processID);
                                if (threadID > 0 && processID == process.Id)
                                {
                                    addWindow(child);
                                }
                            }
                            catch { }

                            return true;
                        });
                    }
                }
            }
            catch { }

            return true;
        });

        return windows;
    }
}

public record ProcessWindowMetadata(string? ClassName, string? Text, bool? IsVisible) { } 
