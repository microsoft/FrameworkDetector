// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;

namespace FrameworkDetector;

public class WindowsBinaryMetadata
{
    public string? Filename;

    public string? OriginalFilename;

    public string? FileVersion;

    public string? ProductName;

    public string? ProductVersion;

    public static WindowsBinaryMetadata GetMetadata(Process process) => GetMetadata(process.MainModule?.FileName);

    public static WindowsBinaryMetadata GetMetadata(string? filename)
    {
        if (filename == null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return new WindowsBinaryMetadata()
        {
            Filename = Path.GetFileName(fileVersionInfo.FileName),
            OriginalFilename = fileVersionInfo.OriginalFilename,
            FileVersion = fileVersionInfo.FileVersion,
            ProductName = fileVersionInfo.ProductName,
            ProductVersion = fileVersionInfo.ProductVersion,
        };
    }

    public JsonObject AsJson()
    {
        var result = new JsonObject();

        if (Filename is not null)
        {
            result["filename"] = Filename;
        }

        if (OriginalFilename is not null)
        {
            result["originalFilename"] = OriginalFilename;
        }

        if (FileVersion is not null)
        {
            result["fileVersion"] = FileVersion;
        }

        if (ProductName is not null)
        {
            result["productName"] = ProductName;
        }

        if (ProductVersion is not null)
        {
            result["productVersion"] = ProductVersion;
        }

        return result;
    }

    public override string ToString()
    {
        return AsJson().ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
    }
}
