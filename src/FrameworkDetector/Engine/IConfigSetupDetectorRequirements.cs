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
    /// Only a single set of required checks MUST be defined for a detector, and it must be defined first.
    /// </summary>
    /// <param name="checks">List of checks to perform.</param>
    /// <returns></returns>
    IConfigAdditionalDetectorRequirements Required(Func<DetectorCheckList, DetectorCheckList> checks);

    DetectorDefinition BuildDefinition();
}
