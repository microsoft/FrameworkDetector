// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Management;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace FrameworkDetector;

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

    public static bool TryGetPackageFullName(this Process process, out string? packageFullName)
    {
        NativeMethods.PACKAGE_ID PackageId;
        unsafe
        {
            int len = 0;
            if (NativeMethods.ERROR_INSUFFICIENT_BUFFER != NativeMethods.GetPackageId(process.Handle, ref len, IntPtr.Zero))
            {
                packageFullName = default;
                return false;
            }

            IntPtr buffer = Marshal.AllocHGlobal(len);

            try
            {
                if (NativeMethods.ERROR_SUCCESS != NativeMethods.GetPackageId(process.Handle, ref len, buffer))
                {
                    packageFullName = default;
                    return false;
                }

                PackageId = Marshal.PtrToStructure<NativeMethods.PACKAGE_ID>(buffer);

                packageFullName = string.Join("_",
                                              Marshal.PtrToStringUni(PackageId.Name),
                                              string.Join(".", PackageId.Version.Major, PackageId.Version.Minor, PackageId.Version.Build, PackageId.Version.Revision),
                                              PackageId.ProcessorArchitecture.ToString(),
                                              Marshal.PtrToStringUni(PackageId.ResourceId),
                                              Marshal.PtrToStringUni(PackageId.PublisherId)
                                             );
                return true;

            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
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
                        // Add the top-level windows for the rootProcess
                        addWindow(hwnd);
                    }

                    if (processMatch || applicationFrameHosts.Where(p => p.Id == processID).Any())
                    {
                        // Add child windows plus any for the rootProcess that are currently parented with ApplicationFrameHost
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

internal partial class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PACKAGE_VERSION
    {
        public ushort Revision;
        public ushort Build;
        public ushort Minor;
        public ushort Major;
    }

    public enum ProcessorArchitecture : uint
    {
        x86 = 0,
        arm = 5,
        x64 = 9,
        neutral = 11,
        arm64 = 12
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PACKAGE_ID
    {
        public uint Reserved;
        public ProcessorArchitecture ProcessorArchitecture;
        public PACKAGE_VERSION Version;
        public IntPtr Name;
        public IntPtr Publisher;
        public IntPtr ResourceId;
        public IntPtr PublisherId;
    }

    public const int ERROR_SUCCESS = 0;
    public const int ERROR_INSUFFICIENT_BUFFER = 122;

    [DllImport("kernel32.dll")]
    public static extern int GetPackageId(IntPtr hProcess, ref int bufferLength, IntPtr pBuffer);
}