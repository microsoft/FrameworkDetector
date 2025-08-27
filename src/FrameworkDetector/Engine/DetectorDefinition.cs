using FrameworkDetector.Checks;
using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;

public class DetectorDefinition : IConfigDetectorRequirements
{
    public IDetector Info { get; init; }

    public List<ICheckDefinition> RequiredChecks { get; init; } = new();

    public List<ICheckDefinition> OptionalChecks { get; init; } = new();

    public DetectorDefinition(IDetector detector)
    {
        Info = detector;
    }

    public IConfigDetectorRequirements Required(Func<DetectorCheckList, DetectorCheckList> checks)
    {
        DetectorCheckList checkList = checks(new());

        foreach (var check in checkList.Checks)
        {
            RequiredChecks.Add(check);
        }

        return this;
    }

    public IConfigDetectorRequirements Optional(string subtitle, Func<DetectorCheckList, DetectorCheckList> checks)
    {
        // TODO: Need to weave in subtitle to ICheckDefinition data here...
        DetectorCheckList checkList = checks(new());

        foreach (var check in checkList.Checks)
        {
            OptionalChecks.Add(check);
        }

        return this;
    }

    public DetectorDefinition BuildDefinition()
    {
        return this;
    }
}