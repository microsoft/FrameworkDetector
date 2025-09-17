// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace FrameworkDetector;

internal static class HWNDExtensions
{
    public static bool EnumChildWindows(this HWND parentHwnd, Func<HWND, bool> func) => PInvoke.EnumChildWindows(parentHwnd, (hwnd, _) => func(hwnd), IntPtr.Zero);

    extension(HWND hwnd)
    {
        public static bool EnumWindows(Func<HWND, bool> func) => PInvoke.EnumWindows((hwnd, _) => func(hwnd), IntPtr.Zero);
    }

    public static string? GetClassName(this HWND hwnd)
    {
        const int capacity = 256;
        var buffer = new char[capacity];

        var result = PInvoke.GetClassName(hwnd, buffer);

        return result > 0 ? new string(buffer, 0, result) : null;
    }

    public static string? GetWindowText(this HWND hwnd)
    {
        int capacity = 1 + PInvoke.GetWindowTextLength(hwnd);

        var buffer = new char[capacity];

        var result = PInvoke.GetWindowText(hwnd, buffer);

        return result > 0 ? new string(buffer, 0, result) : null;
    }

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

    public static bool IsWindowVisible(this HWND hwnd) => PInvoke.IsWindowVisible(hwnd);
}
