// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace FrameworkDetector;

internal static class HWNDExtensions
{
    /// <summary>
    /// Calls the given function on child HWND of the parent HWND by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumchildwindows">EnumChildWindows</see>.
    /// </summary>
    /// <param name="parentHwnd">The parent HWND.</param>
    /// <param name="func">The function to call on each child HWND.</param>
    /// <returns>Whether or not the function succeeded.</returns>
    public static bool EnumChildWindows(this HWND parentHwnd, Func<HWND, bool> func) => PInvoke.EnumChildWindows(parentHwnd, (hwnd, _) => func(hwnd), IntPtr.Zero);

    extension(HWND hwnd)
    {
        /// <summary>
        /// Calls the given function on every top-level HWND on the system by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumwindows">EnumWindows</see>.
        /// </summary>
        /// <param name="func">The function to call on each top-level HWND.</param>
        /// <returns>Whether or not the function succeeded.</returns>
        public static bool EnumWindows(Func<HWND, bool> func) => PInvoke.EnumWindows((hwnd, _) => func(hwnd), IntPtr.Zero);
    }

    /// <summary>
    /// Gets the class name for a given HWND by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getclassnamew">GetClassName</see>.
    /// </summary>
    /// <param name="hwnd">The target HWND.</param>
    /// <returns>The class name of the target HWND.</returns>
    public static string? GetClassName(this HWND hwnd)
    {
        const int capacity = 256;
        var buffer = new char[capacity];

        var result = PInvoke.GetClassName(hwnd, buffer);

        return result > 0 ? new string(buffer, 0, result) : null;
    }

    /// <summary>
    /// Gets the window text for a given HWND by calling by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtextw">GetWindowText</see>.
    /// </summary>
    /// <param name="hwnd">The target HWND.</param>
    /// <returns>The window text of the target HWND.</returns>
    public static string? GetWindowText(this HWND hwnd)
    {
        int capacity = 1 + PInvoke.GetWindowTextLength(hwnd);

        var buffer = new char[capacity];

        var result = PInvoke.GetWindowText(hwnd, buffer);

        return result > 0 ? new string(buffer, 0, result) : null;
    }

    /// <summary>
    /// Gets the thread ID and process ID for the given HWND by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid">GetWindowThreadProcessId</see>.
    /// </summary>
    /// <param name="hwnd">The target HWND.</param>
    /// <param name="processId">The target HWND's process ID.</param>
    /// <returns>The target HWND's thread ID.</returns>
    public static uint GetWindowThreadProcessId(this HWND hwnd, out uint processId)
    {
        uint result;

        unsafe
        {
            uint outProcessId;
            result = PInvoke.GetWindowThreadProcessId(hwnd, &outProcessId);
            processId = outProcessId;
        }

        return result;
    }

    /// <summary>
    /// Gets if the given HWND is curently visible by calling <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-iswindowvisible">IsWindowVisible</see>.
    /// </summary>
    /// <param name="hwnd">The target HWND.</param>
    /// <returns>Whether or not the given HWND is curently visible.</returns>
    public static bool IsWindowVisible(this HWND hwnd) => PInvoke.IsWindowVisible(hwnd);
}
