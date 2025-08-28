using FrameworkDetector.Checks;
using System;
using System.Collections.Generic;

namespace FrameworkDetector.Engine;

public class DetectorDefinition : IConfigSetupDetectorRequirements, IConfigAdditionalDetectorRequirements
{
    public IDetector Info { get; init; }

    public DetectorCheckList? RequiredChecks { get; private set; }

    public Dictionary<string, DetectorCheckList> OptionalChecks { get; init; } = new();

    public DetectorDefinition(IDetector detector)
    {
        Info = detector;
    }

    public IConfigAdditionalDetectorRequirements Required(Func<DetectorCheckList, DetectorCheckList> checks)
    {
        RequiredChecks = checks(new());

        return this;
    }

    // TODO: We could define a record here of metadata about the optional check beyond just a simple string... (for now though not sure what we want here beyond a string... as I think languages and other libraries and features would just be their own dedicated detectors)
    public IConfigAdditionalDetectorRequirements Optional(string subtitle, Func<DetectorCheckList, DetectorCheckList> checks)
    {
        OptionalChecks.Add(subtitle, checks(new()));
        
        return this;
    }

    public DetectorDefinition BuildDefinition()
    {
        return this;
    }
}