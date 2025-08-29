using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace FrameworkDetector.DataSources;

/// <summary>
/// A grouped collection of <see cref="IDataSource"/> by <see cref="IDataSource.Id"/>, used for live lookup of datasources added after initialization.
/// </summary>
public class DataSourceCollection : ReadOnlyDictionary<Guid, IDataSource[]>, IReadOnlyCollection<IDataSource[]>
{
    // Take in a collection of data sources and group by common id (e.g. if an app has multiple processes)
    public DataSourceCollection(IDataSource[] list) 
        : base(
            list.GroupBy(item => item.GetId())
              .ToImmutableDictionary(g => g.Key, g => g.ToArray()))
    {
    }

    /// <summary>
    /// Tries to retrieve all data sources of the requested type. Provides them as the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="sources">array of sources of the specified type, or empty array.</param>
    /// <returns>True if successful.</returns>
    public bool TryGetSources<T>(Guid sourceIdType, out T[] sources)
        where T : IDataSource
    {
        if (TryGetValue(sourceIdType, out IDataSource[]? innerSources))
        {
            if (innerSources == null)
            {
                sources = [];

                return false;
            }

            sources = innerSources.OfType<T>().ToArray();

            return true;
        }

        sources = [];

        return false;
    }

    IEnumerator<IDataSource[]> IEnumerable<IDataSource[]>.GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    // TODO: Implicit conversion with IDataSource[]
}
