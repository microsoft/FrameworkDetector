// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

public class ProcessDataSource : IDataSource
{
    public static Guid Id => new Guid("9C719E0C-2E53-4379-B2F5-C90F47E6C730");
    public Guid GetId() => Id; //// Passthru

    public int ProcessId => Process.Id;

    public IReadOnlyList<ProcessModule> Modules { get; private set; } = Array.Empty<ProcessModule>();

    public WindowsBinaryMetadata? Metadata { get; private set; }

    internal Process Process { get; private set; }

    public ProcessDataSource(Process process)
    {
        Process = process;
    }

    public Task<bool> LoadAndCacheDataAsync(CancellationToken cancellationToken)
    {
        Modules = Process.Modules.Cast<ProcessModule>().ToList();

        Metadata = WindowsBinaryMetadata.GetMetadata(Process);

        return Task.FromResult(true);
    }
}
