// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using YamlDotNet.Serialization;

using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

public record DocMetadata
{
    [YamlMember(Alias = "id")]
    public string? FrameworkId { get; set; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    // TODO: Actual Uri type not supported in AOT by YamlDotNet without explicit converter
    // https://github.com/aaubry/YamlDotNet/issues/1030
    public string? Source { get; init; }

    public string? Website { get; init; }

    public DetectorCategory Category { get; init; }

    // TODO: Converter to list of strings as CSV
    public string? Keywords { get; init; }

    [YamlMember(Alias = "ms.date")]
    public DateTimeOffset? Date { get; init; }

    public string? Author { get; init; }

    public DocStatus Status { get; set; }
}

public enum DocStatus
{
    // Indicates a doc exists but no corresponding Detector does yet
    Placeholder,
    // Indicates either a Detector exists where no doc exists or could be loaded for the corresponding Detector
    // But also used for in-progress docs/detector that are not verified completely for accuracy
    Experimental,
    // Used for completed docs that have been verified for accuracy
    Detectable
}