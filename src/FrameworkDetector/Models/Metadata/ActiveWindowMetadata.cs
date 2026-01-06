// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Models;

/// <summary>
/// Represents metadata describing an active window, including its class name, window text, and visibility state.
/// </summary>
/// <param name="ClassName">The class name of the window. Can be blank if the class name is not available.</param>
/// <param name="IsVisible">Indicates whether the window is currently visible. Can be null if the visibility state is unknown.</param>
public record ActiveWindowMetadata(string ClassName, bool? IsVisible = null) { }
