// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace FrameworkDetector;

public class DetectorResult
{
    public required string DetectorName;

    public required string DetectorVersion;

    public required string FrameworkId;

    public bool FrameworkFound = false;

    public DetectorResultStatus Status = DetectorResultStatus.None;

    public JsonObject? Data;

    public JsonObject AsJson()
    {
        var result = new JsonObject();
        result["detectorName"] = DetectorName;
        result["detectorVersion"] = DetectorVersion;
        result["frameworkId"] = FrameworkId;
        result["frameworkFound"] = FrameworkFound;
        result["status"] = Status.ToString().ToLowerInvariant();
        if (Data is not null)
        {
            result["data"] = Data;
        }
        return result;
    }

    public override string ToString()
    {
        return AsJson().ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
    }
}

public enum DetectorResultStatus
{
    None,
    Canceled,
    Completed,
}