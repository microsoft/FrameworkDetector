// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.DetectorChecks;

public abstract class PathDetectorCheck : IDetectorCheck
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public bool IsRequired { get; private set; }

    public DetectorCheckResult Result { get; private set; }

    public string? Path { get; private set; } = null;

    protected PathDetectorCheck(bool isRequired)
    {
        IsRequired = isRequired;
        Result = new DetectorCheckResult(this);
    }

    public async Task<DetectorCheckStatus> RunCheckAsync(string path, CancellationToken cancellationToken)
    {
        Path = path;
        return await RunCheckAsync(cancellationToken);
    }

    public virtual async Task<DetectorCheckStatus> RunCheckAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            throw new ArgumentNullException(nameof(Path));
        }

        await Task.Yield();

        return DetectorCheckStatus.None;
    }
}
