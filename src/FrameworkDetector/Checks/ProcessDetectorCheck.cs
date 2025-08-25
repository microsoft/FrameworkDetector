// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.DetectorChecks;

public abstract class ProcessDetectorCheck : IDetectorCheck
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public bool IsRequired { get; private set; }

    public DetectorCheckResult Result { get; private set; }

    public Process? Process { get; private set; } = null;

    protected ProcessDetectorCheck(bool isRequired)
    {
        IsRequired = isRequired;
        Result = new DetectorCheckResult(this);
    }

    public async Task<DetectorCheckStatus> RunCheckAsync(Process process, CancellationToken cancellationToken)
    {
        Process = process;
        return await RunCheckAsync(cancellationToken);
    }

    protected virtual async Task<DetectorCheckStatus> RunCheckAsync(CancellationToken cancellationToken)
    {
        if (Process is null)
        {
            throw new ArgumentNullException(nameof(Process));
        }

        await Task.Yield();

        return DetectorCheckStatus.None;
    }
}
