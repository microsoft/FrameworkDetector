// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Models;

/// <summary>
/// Represents metadata describing an active window, including its class name, window text, and visibility state.
/// </summary>
/// <param name="ClassName">The class name of the window. Can be null if the class name is not available.</param>
/// <param name="Text">The text displayed in the window's title bar. Can be null if the window does not have text.</param>
/// <param name="IsVisible">Indicates whether the window is currently visible. Can be null if the visibility state is unknown.</param>
public record ActiveWindowMetadata(string? ClassName = null, string? Text = null, bool? IsVisible = null) { }
