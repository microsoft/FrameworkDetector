// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.Engine;

// TODO: Think replaced by I/CheckDefinition now...
public interface IDetectorCheck
{
    string Name { get; }

    string Description { get; }

    bool IsRequired { get; }

    DetectorCheckResult Result { get; }
}
