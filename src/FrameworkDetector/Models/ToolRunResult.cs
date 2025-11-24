// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FrameworkDetector.DataSources;

namespace FrameworkDetector.Models;

/// <summary>
/// Represents the overall result of all detectors run against an pp.
/// </summary>
public record ToolRunResult
{
    public string ToolName { get; }

    public string ToolVersion { get; }

    /// <summary>
    /// Optionalally provided information from the tool about the arguments passed into it to record the results.
    /// </summary>
    public string? ToolArguments { get; }

    public string Timestamp { get; }

    public Dictionary<string, List<object?>?> DataSources { get; } 

    public List<DetectorResult> DetectorResults { get; set; } = [];

    public ToolRunResult(string toolName, string toolVersion, string toolArguments)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        ToolArguments = toolArguments;
        Timestamp = DateTime.UtcNow.ToString("O");

        DataSources = new Dictionary<string, List<object?>?>();
    }

    public void AddDataSources(DataSourceCollection sources)
    {
        foreach (var kvp in sources)
        {
            if (kvp.Value is not null && kvp.Value.Length > 0)
            {
                var list = new List<object?>();
                foreach (var dataSource in kvp.Value)
                {
                    list.Add(dataSource.Data);
                }
                if (list.Count > 0)
                {
                    DataSources[kvp.Key] = list;
                }
            }
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, DetectorJsonSerializerOptions.Options);
    }
}
