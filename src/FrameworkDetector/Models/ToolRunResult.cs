// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Text.Json;
using System.Text.Json.Serialization;

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

    [JsonConverter(typeof(InputTypeJsonConverter))]
    public IReadOnlyDictionary<string, IReadOnlyList<object?>> Inputs => _inputs;
    private readonly Dictionary<string, IReadOnlyList<object?>> _inputs = new Dictionary<string, IReadOnlyList<object?>>();

    public List<DetectorResult> DetectorResults { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolRunResult"/> class.
    /// </summary>
    /// <param name="toolName">Name of the tool.</param>
    /// <param name="toolVersion">Version of the tool.</param>
    /// <param name="toolArguments">Arguments used with the tool.</param>
    /// <param name="timestamp">Timestamp of when the tool was run.</param>
    /// <param name="inputs">Input data for the tool.</param>
    /// <param name="detectorResults">Results from the detectors.</param>
    [JsonConstructor]
    internal ToolRunResult(string toolName, string toolVersion, string? toolArguments, string timestamp, IReadOnlyDictionary<string, IReadOnlyList<object?>> inputs, List<DetectorResult> detectorResults)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        ToolArguments = toolArguments;
        Timestamp = timestamp;
        foreach (var input in inputs)
        {
            _inputs.Add(input.Key, input.Value);
        }
        DetectorResults = detectorResults;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolRunResult"/> class.
    /// </summary>
    /// <param name="toolName">Name of the tool.</param>
    /// <param name="toolVersion">Version of the tool.</param>
    /// <param name="toolArguments">Arguments used with the tool.</param>
    /// <param name="inputs">Input data for the tool.</param>
    public ToolRunResult(string toolName, string toolVersion, string? toolArguments, IEnumerable<IInputType> inputs)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        ToolArguments = toolArguments;
        Timestamp = DateTime.UtcNow.ToString("O");

        // Transform each input into a dictionary of lists based on the input type name.
        // i.e. all processes would be together in a "processes" bucket
        foreach (var group in inputs.GroupBy(i => i.InputGroup))
        {
            _inputs.Add(group.Key, group.Cast<object?>().ToList());
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, DetectorJsonSerializerOptions.Options);
    }
}
