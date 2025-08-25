// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.DetectorChecks;
using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;

public class Detector : IConfigDetectorRequirements
{
    // TODO: Maybe this is just passed through via putting the interface directly on the detector...
    public IDetector Info { get; init; }

    public DetectorResult Result { get; protected set; }

    public IReadOnlyList<ProcessDetectorCheck> ProcessChecks => _processChecks;
    protected readonly List<ProcessDetectorCheck> _processChecks = new List<ProcessDetectorCheck>();

    public IReadOnlyList<PathDetectorCheck> PathChecks => _pathChecks;
    protected readonly List<PathDetectorCheck> _pathChecks = new List<PathDetectorCheck>();

    internal Detector(IDetector detectorInfo)
    {
        Info = detectorInfo ?? throw new ArgumentNullException(nameof(detectorInfo));

        Result = new DetectorResult()
        {
            DetectorName = Info.Name,
            DetectorVersion = AssemblyInfo.LibraryVersion,
            FrameworkId = Info.FrameworkId,
        };
    }

    public virtual async Task<DetectorStatus> DetectByProcessAsync(Process process, CancellationToken cancellationToken)
    {
        Result.Status = DetectorStatus.InProgress;

        if (!cancellationToken.IsCancellationRequested)
        {
            await Parallel.ForEachAsync(ProcessChecks, cancellationToken, async (check, token) =>
            {
                await check.RunCheckAsync(process, token);
            });
        }

        UpdateResult(ProcessChecks, cancellationToken.IsCancellationRequested);

        return Result.Status;
    }

    public virtual async Task<DetectorStatus> DetectByPathAsync(string path, CancellationToken cancellationToken)
    {
        Result.Status = DetectorStatus.InProgress;

        if (!cancellationToken.IsCancellationRequested)
        {
            await Parallel.ForEachAsync(PathChecks, cancellationToken, async (check, token) =>
            {
                await check.RunCheckAsync(path, token);
            });
        }

        UpdateResult(PathChecks, cancellationToken.IsCancellationRequested);

        return Result.Status;
    }

    protected void UpdateResult(IReadOnlyList<IDetectorCheck> checks, bool wasCanceled)
    {
        var requiredCheckCount = 0;
        var requiredCheckSuccesses = 0;
        var completedCount = 0;

        foreach (var check in checks)
        {
            if (check.IsRequired)
            {
                requiredCheckCount++;
                if (check.Result!.Status == DetectorCheckStatus.CompletedPassed)
                {
                    requiredCheckSuccesses++;
                }

                if (check.Result is not null)
                {
                    Result.CheckResults.Add(check.Result);
                }
            }

            if (check.Result!.Status is DetectorCheckStatus.CompletedPassed or DetectorCheckStatus.CompletedFailed)
            {
                completedCount++;
            }
        }

        if (requiredCheckCount == 0)
        {
            throw new ArgumentException($"Detector \"{Info.Name}\" does not have any required checks!");
        }

        if (completedCount == checks.Count)
        {
            Result.Status = DetectorStatus.Completed;
        }

        Result.FrameworkFound = requiredCheckSuccesses == requiredCheckCount;

        if (wasCanceled && Result.Status != DetectorStatus.Completed)
        {
            Result.Status = DetectorStatus.Canceled;
        }
    }

    public IConfigDetectorRequirements Required(Func<DetectorCheckList, DetectorCheckList> checks)
    {
        throw new NotImplementedException();
    }

    public IConfigDetectorRequirements Optional(string subtitle, Func<DetectorCheckList, DetectorCheckList> checks)
    {
        throw new NotImplementedException();
    }

    public DetectorDefinition BuildDefinition()
    {
        throw new NotImplementedException();
    }
}
