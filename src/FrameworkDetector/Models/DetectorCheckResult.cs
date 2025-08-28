// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
/// Represents the status of an individual check of a specific detector.
/// </summary>
public interface IDetectorCheckResult
{
    public IDetector Detector { get; }

    public ICheckDefinition Check { get; }

    public DetectorCheckStatus Status { get; set; }

    /// <summary>
    /// Gets additional metadata as defined by <see cref="CheckDefinition{T}.Metadata"/> type. Basically the extra information passed from the detector to the check to specify what to look for.
    /// </summary>
    public object? Metadata { get; }
}

public record DetectorCheckResult<T>(
    IDetector Detector,
    ICheckDefinition Check
) : IDetectorCheckResult where T : struct
{
    public DetectorCheckStatus Status { get; set; } = DetectorCheckStatus.None;

    /// <summary>
    /// Gets the Metadata type registered by the check to store information from Detector defintion for processing. Automatically populated by <see cref="CheckDefinition{T}"/>.
    /// </summary>
    public T? ExtraMetadata { get; set; }

    //// Needed to unwrap the generic for dumping to JSON.
    /// <inheritdoc/>
    public object? Metadata => ExtraMetadata;
}
