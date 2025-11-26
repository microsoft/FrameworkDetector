// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Base interface for all input types based into the <see cref="DetectionEngine"/>. Used to group inputs within a <see cref="ToolRunResult"/>.
/// Input types should implement <see cref="I"/>
/// </summary>
public interface IInputType
{
    /// <summary>
    /// Gets the name of the input type. This will be used as the bucket for inputs when grouping within a <see cref="ToolRunResult"/>.
    /// </summary>
    public string Name { get; }
}
