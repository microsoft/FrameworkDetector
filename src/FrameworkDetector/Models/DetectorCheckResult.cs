// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Models;

public enum DetectorCheckStatus
{
    None,
    InProgress,
    Canceled,
    CompletedPassed,
    CompletedFailed,
    Error,
}

/// <summary>
/// Represents the result of an individual check of a specific detector.
/// </summary>
public interface IDetectorCheckResult
{
    [JsonIgnore]
    public IDetector Detector { get; }

    public ICheckDefinition CheckDefinition { get; }

    public DetectorCheckStatus CheckStatus { get; set; }

    /// <summary>
    /// Gets any input args to the check as defined by <see cref="CheckDefinition{TInput,TOutput}"/> type.
    /// </summary>
    public object? CheckInput { get; }

    /// <summary>
    /// Gets any output data from the check as defined by <see cref="CheckDefinition{TInput,TOutput}"/> type.
    /// </summary>
    public object? CheckOutput { get; }
}

public record DetectorCheckResult<TInput, TOutput>(
    IDetector Detector,
    ICheckDefinition CheckDefinition
) : IDetectorCheckResult where TInput : ICheckArgs
                         where TOutput : struct
{
    public DetectorCheckStatus CheckStatus { get; set; } = DetectorCheckStatus.None;

    /// <summary>
    /// Gets the type registered by the check to store input arguments from Detector definition for processing. Automatically populated by <see cref="CheckDefinition{TInput,TOutput}"/>.
    /// </summary>
    public TInput? InputArgs { get; set; }

    /// <summary>
    /// Gets the type registered by the check to store output data from Detector definition for processing. Automatically populated by <see cref="CheckDefinition{TInput,TOutput}"/>.
    /// </summary>
    public TOutput? OutputData { get; set; }

    //// Needed to unwrap the generic for dumping to JSON.
    /// <inheritdoc/>
    public object? CheckInput => InputArgs;

    //// Needed to unwrap the generic for dumping to JSON.
    /// <inheritdoc/>
    public object? CheckOutput => OutputData;
}
