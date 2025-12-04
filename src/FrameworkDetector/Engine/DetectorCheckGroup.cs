// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

using FrameworkDetector.Checks;
using FrameworkDetector.Models;

namespace FrameworkDetector.Engine;

//// TODO: Added Name as a property here, but we don't use it as we have them defined in CheckDefinition.GroupName... Probably depends on if we aggregate or use it within the results in another way.

/// <summary>
/// General collection of <see cref="ICheckDefinition"/> checks. Extension point for any checks to hook into Fluent API surface. e.g. <see cref="ContainsModuleCheck"/>.
/// </summary>
public class DetectorCheckGroup(string Name) : IDetectorCheckGroup,
                                               IReadOnlyCollection<ICheckDefinition>
{
    internal List<ICheckDefinition> Checks { get; init; } = new();

    internal ICheckDefinition? CheckWhichProvidesVersion = null;

    internal Func<IDetectorCheckResult, string>? VersionGetter = null;

    public int Count => Checks.Count;

    public DetectorCheckGroup Get() => this;

    public void AddCheck(ICheckDefinition definition)
    {
        Checks.Add(definition);
    }

    public void SetVersionGetter(Func<IDetectorCheckResult, string> versionGetter)
    {
        if (Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Count));
        }

        CheckWhichProvidesVersion = Checks[^1];

        VersionGetter = versionGetter;
    }
        
    public IEnumerator<ICheckDefinition> GetEnumerator()
    {
        return Checks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString() => Name;
}

public interface IDetectorCheckGroup
{
    DetectorCheckGroup Get();
}

/// <summary>
/// Abstract class wrapping <see cref="IDetectorCheckGroup"/> to allow check extensions to define extra methods for configuration of checks. E.g. providing the specification for returning the version of the detected framework.
/// </summary>
/// <param name="idcg"><see cref="IDetectorCheckGroup"/></param>
public abstract class DetectorCheckGroupWrapper(IDetectorCheckGroup idcg) : IDetectorCheckGroup
{
    internal IDetectorCheckGroup IDetectorCheckGroup = idcg;

    public DetectorCheckGroup Get() => IDetectorCheckGroup.Get();
}