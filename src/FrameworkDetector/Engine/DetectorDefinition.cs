using FrameworkDetector.Checks;
using System;
using System.Collections.Generic;

namespace FrameworkDetector.Engine;

public class DetectorDefinition : IConfigSetupDetectorRequirements, IConfigAdditionalDetectorRequirements
{
    public IDetector Info { get; init; }

    public Dictionary<string, DetectorCheckGroup> RequiredChecks { get; init; } = new();

    public Dictionary<string, DetectorCheckGroup> OptionalChecks { get; init; } = new();

    public DetectorDefinition(IDetector detector)
    {
        Info = detector;
    }

    public IConfigAdditionalDetectorRequirements Required(string groupName, Func<DetectorCheckGroup, DetectorCheckGroup> checks)
    {
        RequiredChecks.Add(groupName, checks(new(groupName)));

        // Mark all required
        foreach (var check in RequiredChecks[groupName])
        {
            check.IsRequired = true;
            check.GroupName = groupName;
        }

        return this;
    }

    // TODO: We could define a record here of metadata about the optional check beyond just a simple string... (for now though not sure what we want here beyond a string... as I think languages and other libraries and features would just be their own dedicated detectors)
    public IConfigAdditionalDetectorRequirements Optional(string groupName, Func<DetectorCheckGroup, DetectorCheckGroup> checks)
    {
        OptionalChecks.Add(groupName, checks(new(groupName)));

        // Tag all metadata to the check
        foreach (var check in OptionalChecks[groupName])
        {
            check.GroupName = groupName;
        }
        
        return this;
    }

    public DetectorDefinition BuildDefinition()
    {
        return this;
    }
}