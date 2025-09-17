// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;

using FrameworkDetector.Checks;

namespace FrameworkDetector.Engine;

//// TODO: Added Name as a property here, but we don't use it as we have them defined in CheckDefinition.GroupName... Probably depends on if we aggregate or use it within the results in another way.

/// <summary>
/// General collection of <see cref="ICheckDefinition"/> checks. Extension point for any checks to hook into Fluent API surface. e.g. <see cref="ContainsLoadedModuleCheck"/>.
/// </summary>
public class DetectorCheckGroup(string Name) : IReadOnlyCollection<ICheckDefinition>
{
    internal List<ICheckDefinition> Checks { get; init; } = new();

    public int Count => Checks.Count;
    
    // TODO: nit: How to make this accessible to extension author but not detector author?
    public void AddCheck(ICheckDefinition definition)
    {
        Checks.Add(definition);
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