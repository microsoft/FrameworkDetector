// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace FrameworkDetector.Engine;

/// <summary>
/// Fluent API Helper interface to provide meaningful context to defining a detector after calling <see cref="IDetectorExtensions.Create(FrameworkDetector.Engine.IDetector)"/>.
/// </summary>
public interface IConfigSetupDetectorRequirements
{
    /// <summary>
    /// Used to define required checks that ALL must pass to provide a positive result of the detector.
    /// Multiple groups of required checks may be defined for a detector (there must be at least one), and it must be defined first.
    /// If multiple required groups are defined, only one of the groups must pass to mark as detected.
    /// </summary>
    /// <param name="groupName">Label for this check group.</param>
    /// <param name="checks">List of checks to perform.</param>
    /// <returns></returns>
    IConfigAdditionalDetectorRequirements Required(string groupName, Func<DetectorCheckGroup, DetectorCheckGroup> checks);
}
