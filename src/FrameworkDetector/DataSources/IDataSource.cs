using System;
using System.Threading;
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
    //// In the SG case, we probably need a parent class which represents the data source type, then we could
    //// create a keyed index within the services bucket at compile/load time in an AOT way. Then the specific instance of the data source is registered to that bucket. Since we have the DataSourceCollection type now, that'd probably expand in some ways to represent that role or split apart, as the look-up would be done through DI, so the inner value list of datasource would be the bit encapsulated??
    /// <summary>
    /// Gets the static Id of the type of data source this is.
    /// </summary>
    /// <returns></returns>
    Guid GetId();

    /// <summary>
    /// Called during initialization for the data source to snapshot data about an application and cache for use by all detectors.
    /// </summary>
    /// <returns></returns>
    Task<bool> LoadAndCacheDataAsync(CancellationToken cancellationToken);
}