using FrameworkDetector.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FrameworkDetector.Models;

/// <summary>
/// Represents the overall result of all detectors run against an pp.
/// </summary>
public record ToolRunResult
{
    public string ToolName { get; }

    public string ToolVersion { get; }

    public string Timestamp { get; }

    public WindowsBinaryMetadata[] ProcessMetadata { get; private set; } = [];

    public List<DetectorResult> Detectors { get; set; } = [];

    public ToolRunResult(string toolName, string toolVersion, DataSourceCollection sources)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        Timestamp = DateTime.UtcNow.ToString("O");

        // TODO: We may want to think about this as an extension point where each DataSource can add info to the Run Result data...?
        // For now just pipe metadata from our process datasource.
        if (sources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            ProcessMetadata = [.. processes.Where(static p => p.Metadata != null)
                                           .Select(static p => p.Metadata!)];
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, DetectorJsonSerializerOptions.Options);
    }
}
