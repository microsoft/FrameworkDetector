using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkDetector.DataSources;

public class ProcessDataSource : IDataSource
{
    public static Guid Id => new Guid("9C719E0C-2E53-4379-B2F5-C90F47E6C730");

    public int ProcessId { get; }

    public IReadOnlyList<ProcessModule> Modules { get; private set; } = Array.Empty<ProcessModule>();

    private Process? Process { get; set; } = null;

    // TODO: Provide other helpers for finding processes, such as by executable name or window title.
    public ProcessDataSource(int processId)
    {
        ProcessId = processId;
    }

    public Task<bool> LoadAndCacheDataAsync()
    {
        Process = Process.GetProcessById(ProcessId);

        Modules = Process.Modules.Cast<ProcessModule>().ToList();

        return Task.FromResult(true);
    }
}
