// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace FrameworkDetector.Models;

public static class AssemblyInfo
{
    public static Assembly LibraryAssembly => _libraryAssembly ??= Assembly.GetExecutingAssembly();
    private static Assembly? _libraryAssembly = null;

    public static Assembly ToolAssembly => _toolAssembly ??= Assembly.GetEntryAssembly() ?? LibraryAssembly;
    private static Assembly? _toolAssembly = null;

    public static string LibraryName => _libraryName ?? GetName(ToolAssembly);
    private static string? _libraryName = null;

    public static string ToolName => _toolName ?? GetName(ToolAssembly);
    private static string? _toolName = null;

    public static string LibraryVersion => _libraryVersion ?? GetVersionString(ToolAssembly);
    private static string? _libraryVersion = null;

    public static string ToolVersion => _toolVersion ?? GetVersionString(ToolAssembly);
    private static string? _toolVersion = null;

    internal static string GetName(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return name is null ? "Unknown" : name;
    }

    internal static string GetVersionString(Assembly assembly)
    {
        var vers = assembly.GetName().Version;
        return vers is null ? "0.0.0.0" : $"{vers.Major}.{vers.Minor}.{vers.Build}.{vers.Revision}";
    }
}
