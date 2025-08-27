using System;
using System.Threading.Tasks;

namespace FrameworkDetector.DataSources;

public interface IDataSource
{
    /// <summary>
    /// Gets the Id of the category/type of this data source (e.g. all Process data sources will have the same Id, compared to those that comes from File/disk, etc...)
    /// This can be used by checks that care about that type of data source to gather from the <see cref="DataSourceCollection"/>.
    /// </summary>
    static virtual Guid Id { get; } = Guid.Empty;

    //// TODO: If we use source generators for this setup/registry then this probably gets simpler.
    /// <summary>
    /// Gets the static Id of the type of data source this is.
    /// </summary>
    /// <returns></returns>
    Guid GetId();

    /// <summary>
    /// Called during initialization for the data source to snapshot data about an application and cache for use by all detectors.
    /// </summary>
    /// <returns></returns>
    Task<bool> LoadAndCacheDataAsync();
}