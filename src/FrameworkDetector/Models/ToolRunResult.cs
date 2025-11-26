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

    public Dictionary<string, List<object?>?> Inputs { get; } 

    public List<DetectorResult> DetectorResults { get; set; } = [];

    public ToolRunResult(string toolName, string toolVersion, string? toolArguments)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        ToolArguments = toolArguments;
        Timestamp = DateTime.UtcNow.ToString("O");

        Inputs = new Dictionary<string, List<object?>?>();
    }

    public void AddInputs(IReadOnlyList<IInputType> inputs)
    {
        // Transform each input into a dictionary of lists based on the input type name.
        // i.e. all processes would be together in a "processes" bucket
        foreach (var inputKey in inputs.Select(i => i.Name).Distinct())
        {
            foreach (var input in inputs)
            {
                if (input.Name == inputKey)
                {
                    if (!Inputs.ContainsKey(inputKey))
                    {
                        Inputs[inputKey] = new List<object?>();
                    }
                    Inputs[inputKey]?.Add(input);
                }
            }
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, DetectorJsonSerializerOptions.Options);
    }
}
