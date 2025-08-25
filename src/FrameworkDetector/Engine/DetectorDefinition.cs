using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;

public class DetectorDefinition : IConfigDetectorRequirements
{
    public IDetector Info { get; init; }

    public List<IDetectorCheck> RequiredChecks { get; init; } = new();

    public List<IDetectorCheck> OptionalChecks { get; init; } = new();

    public DetectorDefinition(IDetector detector)
    {
        Info = detector;
    }

    public IConfigDetectorRequirements Required(Func<DetectorCheckList, DetectorCheckList> checks)
    {
        RequiredChecks.Add();

        return this;
    }

    public IConfigDetectorRequirements Optional(string subtitle, Func<DetectorCheckList, DetectorCheckList> checks)
    {
        OptionalChecks.Add();

        return this;
    }

    public DetectorDefinition BuildDefinition()
    {
        return this;
    }
}