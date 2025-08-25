using System;
using System.Threading.Tasks;

namespace FrameworkDetector.DataSources;

public interface IDataSource
{
    static virtual Guid Id { get; } = Guid.Empty;

    Task<bool> LoadAndCacheDataAsync();
}