// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Text.Json;

using FrameworkDetector.Inputs;

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

    public IReadOnlyDictionary<string, List<object?>> Inputs { get; private set; } 

    public List<DetectorResult> DetectorResults { get; set; } = [];

    public ToolRunResult(string toolName, string toolVersion, string? toolArguments, IReadOnlyList<IInputType> inputs)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        ToolArguments = toolArguments;
        Timestamp = DateTime.UtcNow.ToString("O");

        // Transform each input into a dictionary of lists based on the input type name.
        // i.e. all processes would be together in a "processes" bucket
        Inputs = inputs.GroupBy(i => i.Name)
                       .ToDictionary(g => g.Key, g => g.Cast<object?>().ToList()).AsReadOnly();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, DetectorJsonSerializerOptions.Options);
    }
}
