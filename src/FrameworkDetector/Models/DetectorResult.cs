// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

using FrameworkDetector.Engine;

namespace FrameworkDetector.Models;

public enum DetectorStatus
{
    None,
    InProgress,
    Canceled,
    Completed,
}

/// <summary>
/// Represents the overall result output of a given <see cref="IDetector"/>.
/// Did the required check pass? What other metadata was used for that check, etc...
/// </summary>
public class DetectorResult
{
    public required string DetectorName { get; set; }

    public required string DetectorVersion { get; set; }

    public required string FrameworkId { get; set; }

    public bool FrameworkFound { get; set; } = false;

    public DetectorStatus Status { get; set; } = DetectorStatus.None;

    // TODO: Q: Do we want these effectively grouped by groups, i.e. the DetectorCheckGroup Name?
    public List<IDetectorCheckResult> CheckResults { get; } = [];
}
