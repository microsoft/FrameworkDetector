// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Base interface for all input types based into the <see cref="DetectionEngine"/>. Used to group inputs within a <see cref="ToolRunResult"/>.
/// Inputs wrap and provide Data Sources for the target type they wrap only. It may be possible for an input source to reach other types of inputs,
/// but that is handled by the parent application to create multiple base inputs as needed. Data Sources from the input type should be what is
/// only accessible solely from that input type.
/// </summary>
/// <remarks>
/// Input types should also implement <see cref="IInputTypeFactory{T}"/>.
/// </remarks>
public interface IInputType
{
    /// <summary>
    /// An identifier to group similar inputs within a <see cref="ToolRunResult"/>.
    /// </summary>
    public string InputGroup { get; }
}
