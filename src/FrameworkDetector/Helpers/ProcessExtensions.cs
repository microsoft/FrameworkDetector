// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using System.Management;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;

using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

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
    /// Checks if the given process information is accessible in our current context.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>Whether or not the process information is accessible in our current context.</returns>
    public static bool IsAccessible(this Process process)
    {
        try
        {
            // TODO: do this for real, this is just a quick heurestic
            return process.MainModule?.FileVersionInfo is not null;
        }
        catch { }

        return false;
    }

    /// <summary>
    /// Checks if the given process has a GUI.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>Whether or not the process has a GUI.</returns>
    public static bool HasGUI(this Process process)
    {
        try
        {
            // Note: MainWindowHandle works for Win32 processes easily, but UWP processes are hosted under the application frame host, so we also need to look there for apps.
            // Put it after the OR, so we only look up for UWP apps, if we don't already have a MainWindowHandle and only grab Visible windows (otherwise we get a lot of services and other background processes).
            // TODO: I think this is a bit similar to how Task Manager does it (filters their 'Apps' list) [i.e. has child window]; but we could probably still improve/test/encapsulate this further.
            return process.MainWindowHandle != IntPtr.Zero || process.GetActiveWindowMetadata().Any(w => w.IsVisible == true && w.ClassName != "PseudoConsoleWindow");
        }
        catch { }

        return false;
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
    /// Tries to find the Package Family Name of a process by calling <see cref="https://learn.microsoft.com/windows/win32/api/appmodel/nf-appmodel-getpackagefamilyname">GetPackageFamilyName</see>.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="packageFamilyName">The Package Family Name, if found.</param>
    /// <returns>Whether or not the Package Family Name was found.</returns>
    public static bool TryGetPackageFamilyName(this Process process, out string? packageFamilyName)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(8))
        {
            uint length = 0;
            if (WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER == PInvoke.GetPackageFamilyName(process.SafeHandle, ref length, null) && length > 0)
            {
                var buffer = new char[length];
                if (WIN32_ERROR.ERROR_SUCCESS == PInvoke.GetPackageFamilyName(process.SafeHandle, ref length, buffer))
                {
                    packageFamilyName = new string(buffer, 0, (int)length - 1);
                    return true;
                }
            }
        }

        packageFamilyName = default;
        return false;
    }

    /// <summary>
    /// Tries to find the Package Full Name (PFN) of a process by calling <see cref="https://learn.microsoft.com/windows/win32/api/appmodel/nf-appmodel-getpackagefullname">GetPackageFullName</see>.
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
    /// Try waiting for a process to be idle and responsive to input.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Whether or not an idle state was detected.</returns>
    public static async Task<bool> TryWaitForIdleAsync(this Process process, CancellationToken cancellationToken) => await process.TryWaitForIdleAsync(MaxWaitForIdleTimeoutMs, cancellationToken);

    private const int MaxWaitForIdleTimeoutMs = 30000;

    /// <summary>
    /// Try waiting for a process to be idle and responsive to input.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="timeoutMs">The maximum time to wait in milliseconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Whether or not an idle state was detected.</returns>
    public static async Task<bool> TryWaitForIdleAsync(this Process process, int timeoutMs, CancellationToken cancellationToken)
    {
        if (process.HasGUI())
        {
            var sw = Stopwatch.StartNew();
            timeoutMs = Math.Min(timeoutMs, MaxWaitForIdleTimeoutMs); // Limit to MaxWaitForIdleTimeoutMs

            var incrementMs = Math.Min(timeoutMs, 50); // Divide into max 50ms chunks

            while (!cancellationToken.IsCancellationRequested && (sw.ElapsedMilliseconds < timeoutMs))
            {
                try
                {
                    if (process.WaitForInputIdle(incrementMs) == true && process.Responding == true)
                    {
                        return true;
                    }
                }
                catch { }
                await Task.Yield();
            }
        }

        return false;
    }

    extension(Process)
    {
        /// <summary>
        /// Try to launch the app specified by the given Application User Model Id (AUMID) and return the started process.
        /// </summary>
        /// <param name="applicationUserModelId">The target Application User Model Id (AUMID).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The started process.</returns>
        public static async Task<Process?> StartByApplicationModelUserIdAsync(string applicationUserModelId, CancellationToken cancellationToken)
        {
            // See: https://github.com/microsoft/WindowsAppSDK/discussions/2747
            // Note: This code required us to specify a specific Windows Version TFM
            PackageManager packageManager = new();

            AppListEntry? appListEntry = null;

            foreach (var package in WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackages() : packageManager.FindPackagesForUser(string.Empty))
            {
                if (package is not null)
                {
                    var entries = await package.GetAppListEntriesAsync();
                    if (entries is not null)
                    {
                        foreach (var entry in entries)
                        {
                            if (entry.AppUserModelId == applicationUserModelId)
                            {
                                appListEntry = entry;
                                break;
                            }
                        }
                    }
                }
                if (appListEntry is not null)
                {
                    break;
                }
            }

            if (appListEntry is not null)
            {
                return await appListEntry.LaunchAndGetProcessAsync(cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// Try to launch the app specified by the given Package Full Name (PFN) and return the started process.
        /// </summary>
        /// <param name="packageFullName">The target Package Full Name (PFN).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The started process.</returns>
        public static async Task<Process?> StartByPackageFullNameAsync(string packageFullName, CancellationToken cancellationToken) => await StartByPackageFullNameAsync(packageFullName, null, cancellationToken);

        /// <summary>
        /// Try to launch the app specified by the given Application User Model Id (AUMID) of the given Package Full Name (PFN) and return the started process.
        /// </summary>
        /// <param name="packageFullName">The target Package Full Name (PFN).</param>
        /// <param name="applicationUserModelId">The target Application User Model Id (AUMID).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The started process.</returns>
        public static async Task<Process?> StartByPackageFullNameAsync(string packageFullName, string? applicationUserModelId, CancellationToken cancellationToken)
        {
            // See: https://github.com/microsoft/WindowsAppSDK/discussions/2747
            // Note: This code required us to specify a specific Windows Version TFM
            PackageManager packageManager = new();

            var package = WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackage(packageFullName) : packageManager.FindPackageForUser(string.Empty, packageFullName);

            if (package is not null)
            {
                var entries = await package.GetAppListEntriesAsync();
                if (entries is not null && entries.Count > 0)
                {
                    var appListEntry = applicationUserModelId is null ? entries[0] : entries.Where(entry => entry.AppUserModelId == applicationUserModelId).FirstOrDefault();
                    if (appListEntry is not null)
                    {
                        return await appListEntry.LaunchAndGetProcessAsync(cancellationToken);
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the window metadata for every active window belonging to the given process.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>The metadata for each active window.</returns>
    public static IReadOnlyCollection<ActiveWindowMetadata> GetActiveWindowMetadata(this Process process)
    {
        var windows = new HashSet<ActiveWindowMetadata>();

        // The HWNDs for UWP apps are hidden as children under ApplicationFrameHost's top-level HWND,
        // so we'll need to make sure we check there if the target process is UWP (which we don't know)
        var applicationFrameHosts = Process.GetProcessesByName("ApplicationFrameHost");

        // The lambda we'll use to add HWNDs that belong to the target process to our result list
        bool addWindow(HWND hwnd)
        {
            try
            {
                var className = hwnd.GetClassName();

                if (className is not null)
                {
                    // We're using a set to reduce "dupes" (since we're just capturing class name and is/isn't visible)
                    // but is there a world where the **count** of windows (of a certain class) matters?
                    windows.Add(new ActiveWindowMetadata(className,
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

                    if (processMatch || applicationFrameHosts.Any(p => p.Id == processID))
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
    /// Gets the module metadata for every module loaded by the given process.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>The metadata for each loaded module.</returns>
    public static IReadOnlySet<WindowsModuleMetadata> GetLoadedModuleMetadata(this Process process)
    {
        // Get modules loaded in memory by the process.
        var loadedModules = new HashSet<WindowsModuleMetadata>();
        foreach (var module in process.Modules.Cast<ProcessModule>())
        {
            var moduleMetadata = WindowsModuleMetadata.GetMetadata(module.FileName, isLoaded: true);
            if (moduleMetadata is not null)
            {
                loadedModules.Add(moduleMetadata);
            }
        }
        return loadedModules;
    }

    /// <summary>
    /// Gets a <see cref="FileInfo"/> object representing the <see cref="Process.MainModule"/> location of the process.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <returns>FileInfo location of Main Module or null.</returns>
    public static FileInfo? GetMainModuleFileInfo(this Process process)
    {
        if (process.MainModule is not null 
            && process.MainModule.FileName is not null)
        {
            return new FileInfo(process.MainModule.FileName);
        }

        return null;
    }

    /// <summary>
    /// Tries to get a <see cref="Package"/> from a <see cref="Process"/>, used to help create a <see cref="InstalledPackageInput"/> from a <see cref="Process"/>.
    /// </summary>
    /// <param name="process">The target process.</param>
    /// <param name="package">The <see cref="Package"/> of the process, if available.</param>
    /// <returns>Whether or not a <see cref="Package"/> was found.</returns>
    public static bool TryGetPackageFromProcess(this Process process, out Package? package)
    {
        try
        {
            var processInfo = ProcessDiagnosticInfo.TryGetForProcessId((uint)process.Id);
            if (processInfo is not null && processInfo.IsPackaged && process.TryGetPackageFullName(out var packageFullName))
            {
                PackageManager packageManager = new();

                package = WindowsIdentity.IsRunningAsAdmin ? packageManager.FindPackage(packageFullName) : packageManager.FindPackageForUser(string.Empty, packageFullName);
                return package is not null;
            }
        }
        catch { }

        package = default;
        return false;
    }
}
