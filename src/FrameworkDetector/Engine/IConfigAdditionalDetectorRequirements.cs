// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace FrameworkDetector.Engine;

/// <summary>
/// Fluent API Helper interface to provide meaningful context to defining a detector after calling <see cref="IConfigSetupDetectorRequirements.Required(Func{FrameworkDetector.Engine.DetectorCheckList, FrameworkDetector.Engine.DetectorCheckList})"/>
/// </summary>
public interface IConfigAdditionalDetectorRequirements
{
    /// <summary>
    /// Used to define sets of optional checks which can further detect various configurations of an application.
    /// Any number of optional check sets can be provided. A lack of detection of an optional set will not fail the required set for the detector. Each check block will just individually pass/fail under the extra tag of the provided subtitle.
    /// </summary>
    /// <param name="subtitle"></param>
    /// <param name="checks"></param>
    /// <returns></returns>
    IConfigAdditionalDetectorRequirements Optional(string subtitle, Func<DetectorCheckList, DetectorCheckList> checks);

    DetectorDefinition BuildDefinition();
}