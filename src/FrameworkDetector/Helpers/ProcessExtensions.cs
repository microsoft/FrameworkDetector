// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

using System.Management;

using Windows.Win32;
using Windows.Win32.Foundation;

using PeNet;

namespace FrameworkDetector;

public static class ProcessExtensions
{
    /// <summary>
    /// Gets all of the children processes for the given process.
    /// Adapted from https://stackoverflow.com/a/38614443
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="recursive">Whether or not to recursively include children of the children.</param>
    /// <returns>The children processes.</returns>
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

    /// <summary>
    /// Given an array of processes, tries to identify if one is the root (parent) of all of the others.
    /// </summary>
    /// <param name="processes">The array of processes.</param>
    /// <param name="rootProcess">The root process.</param>
    /// <returns>Whether or not a root process was identified.</returns>
    public static bool TryGetRootProcess(this Process[] processes, out Process? rootProcess)
    {
        var remainingProcesses = new List<Process>(processes);

        foreach (var process in processes)
        {
            var children = process.GetChildProcesses().Select(p => p.Id);
            remainingProcesses.RemoveAll(p => children.Contains(p.Id));
        }

        if (remainingProcesses.Count == 1)
        {
            rootProcess = remainingProcesses[0];
            return true;
        }

        rootProcess = default;
        return false;
    }

    /// <summary>
    /// Tries to find the Package Full Name (PFN) of a process by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/appmodel/nf-appmodel-getpackagefullname">GetPackageFullName</see>.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="packageFullName">The Package Full Name (PFN), if found.</param>
    /// <returns>Whether or not the Package Full Name (PFN) was found.</returns>
    public static bool TryGetPackageFullName(this Process process, out string? packageFullName)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(8))
        {
            uint length = 0;
            if (WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER == PInvoke.GetPackageFullName(process.SafeHandle, ref length, null) && length > 0)
            {
                var buffer = new char[length];
                if (WIN32_ERROR.ERROR_SUCCESS == PInvoke.GetPackageFullName(process.SafeHandle, ref length, buffer))
                {
                    packageFullName = new string(buffer, 0, (int)length - 1);
                    return true;
                }
            }
        }

        packageFullName = default;
        return false;
    }

    /// <summary>
    /// Tries to find the Application User Model Id (AUMID) of a process by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/appmodel/nf-appmodel-getapplicationusermodelid">GetApplicationUserModelId</see>.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="applicationUserModelId">The Application User Model Id (AUMID), if found.</param>
    /// <returns>Whether or not the Application User Model Id (AUMID) was found.</returns>
    public static bool TryGetApplicationUserModelId(this Process process, out string? applicationUserModelId)
    {
        uint length = 0;

        if (WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER == PInvoke.GetApplicationUserModelId(process.SafeHandle, ref length, null) && length > 0)
        {
            var buffer = new char[length];
            if (WIN32_ERROR.ERROR_SUCCESS == PInvoke.GetApplicationUserModelId(process.SafeHandle, ref length, buffer))
            {
                applicationUserModelId = new string(buffer, 0, (int)length - 1);
                return true;
            }
        }

        applicationUserModelId = default;
        return false;
    }

    /// <summary>
    /// Gets the window metadata for every active window belonging to the given process.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>The metadata for each active window.</returns>
    public static IEnumerable<ProcessWindowMetadata> GetActiveWindowMetadata(this Process process)
    {
        var windows = new HashSet<ProcessWindowMetadata>();

        // The HWNDs for UWP apps are hidden as children under ApplicationFrameHost's top-level HWND,
        // so we'll need to make sure we check there if the target process is UWP (which we don't know)
        var applicationFrameHosts = Process.GetProcessesByName("ApplicationFrameHost");

        // The lambda we'll use to add HWNDs that belong to the target process to our result list
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

        // EnumWindows calls the given callback on every top-level HWND on the system
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
                        // This top-level HWND belongs to our target process, so add it to our result list
                        addWindow(hwnd);
                    }

                    if (processMatch || applicationFrameHosts.Where(p => p.Id == processID).Any())
                    {
                        // HWNDs can be parented to HWNDs of different processes, which is especially true
                        // for UWP apps, whose HWNDs get parented to ApplicationFrameHost's top-level HWND
                        // So:
                        // If this top-level HWND belongs to our target process OR to ApplicationFrameHost then
                        // we need to check that if its child HWNDs also belong to the target process
                        // TODO: Would it be too expensive to just check every top-level HWND's children?
                        hwnd.EnumChildWindows(child =>
                        {
                            try
                            {
                                var threadID = child.GetWindowThreadProcessId(out var processID);
                                if (threadID > 0 && processID == process.Id)
                                {
                                    // This child HWND belongs to our target process, so add it to our result list
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

    /// <summary>
    /// Gets the metadata for the functions imported by the main module of the given process.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>The metadata from each imported function.</returns>
    public static IEnumerable<ProcessImportedFunctionsMetadata> ProcessImportedFunctionsMetadata(this Process process)
    {
        var importedFunctions = new HashSet<ProcessImportedFunctionsMetadata>();

        if (process.MainModule is not null && process.MainModule.FileName is not null)
        {
            if (TryGetCachedPeFile(process.MainModule.FileName, out var peFile) && peFile is not null)
            {
                lock (peFile)
                {
                    var tempMap = new Dictionary<string, List<ProcessFunctionMetadata>>();

                    if (peFile.ImportedFunctions is not null)
                    {
                        foreach (var importedFunction in peFile.ImportedFunctions)
                        {
                            if (!tempMap.TryGetValue(importedFunction.DLL, out var functions))
                            {
                                functions = new List<ProcessFunctionMetadata>();
                                tempMap[importedFunction.DLL] = functions;
                            }

                            if (importedFunction.Name is not null)
                            {
                                tempMap[importedFunction.DLL].Add(new ProcessFunctionMetadata(importedFunction.Name, false));
                            }
                        }
                    }

                    if (peFile.DelayImportedFunctions is not null)
                    {
                        foreach (var delayImportedFunction in peFile.DelayImportedFunctions)
                        {
                            if (!tempMap.TryGetValue(delayImportedFunction.DLL, out var functions))
                            {
                                functions = new List<ProcessFunctionMetadata>();
                                tempMap[delayImportedFunction.DLL] = functions;
                            }

                            if (delayImportedFunction.Name is not null)
                            {
                                tempMap[delayImportedFunction.DLL].Add(new ProcessFunctionMetadata(delayImportedFunction.Name, true));
                            }
                        }
                    }

                    foreach (var kvp in tempMap)
                    {
                        importedFunctions.Add(new ProcessImportedFunctionsMetadata(kvp.Key, kvp.Value.ToArray()));
                    }
                }
            }
        }

        return importedFunctions;
    }

    /// <summary>
    /// Gets the metadata for the functions exported by the main module of the given process.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>The metadata from each exported function.</returns>
    public static IEnumerable<ProcessExportedFunctionsMetadata> ProcessExportedFunctionsMetadata(this Process process)
    {
        var exportedFunctions = new HashSet<ProcessExportedFunctionsMetadata>();

        if (process.MainModule is not null && process.MainModule.FileName is not null)
        {
            if (TryGetCachedPeFile(process.MainModule.FileName, out var peFile) && peFile is not null)
            {
                lock (peFile)
                {
                    if (peFile.ExportedFunctions is not null)
                    {
                        foreach (var exportedFunction in peFile.ExportedFunctions)
                        {
                            if (exportedFunction is not null && exportedFunction.Name is not null)
                            {
                                exportedFunctions.Add(new ProcessExportedFunctionsMetadata(exportedFunction.Name));
                            }
                        }
                    }
                }
            }
        }

        return exportedFunctions;
    }

    private static bool TryGetCachedPeFile(string filename, out PeFile? peFile)
    {
        PeFile? result = null;
        lock (_cachedPeFiles)
        {
            if (!_cachedPeFiles.TryGetValue(filename, out result))
            {
                // Cache whatever PeFile.TryParse gets, so we don't ever waste time reparsing a file
                PeFile.TryParse(filename, out var newPeFile);
                _cachedPeFiles.TryAdd(filename, newPeFile);
                result = newPeFile;
            }

            peFile = result;
            return result is not null;
        }
    }

    private static readonly ConcurrentDictionary<string, PeFile?> _cachedPeFiles = new ConcurrentDictionary<string, PeFile?>();
}

public record ProcessWindowMetadata(string? ClassName = null, string? Text = null, bool? IsVisible = null) { }

public record ProcessFunctionMetadata(string Name, bool? DelayLoaded = null);

public record ProcessImportedFunctionsMetadata(string ModuleName, ProcessFunctionMetadata[]? Functions = null) { }

public record ProcessExportedFunctionsMetadata(string Name) : ProcessFunctionMetadata(Name);
