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

    public string Timestamp { get; }

    public Dictionary<Guid, List<object?>?> DataSources { get; } 

    public List<DetectorResult> DetectorResults { get; set; } = [];

    public ToolRunResult(string toolName, string toolVersion)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        Timestamp = DateTime.UtcNow.ToString("O");

        DataSources = new Dictionary<Guid, List<object?>?>();
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
