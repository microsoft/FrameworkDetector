// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.Checks;
using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Engine;

public delegate void DetectionProgressChangedEventHandler(object sender, DetectionProgressChangedEventArgs e);

public class DetectionProgressChangedEventArgs(double progress) : EventArgs
{
    public readonly double Progress = progress;
}

/// <summary>
/// Core handler of logic for running various <see cref="IDetector"/> defined detectors (as registered in service collection of parent runner detector application) against <see cref="IDataSource"/> sources provided by said app's configuration (e.g. processId to get Process info).
/// Will use defined <see cref="ICheckDefinition"/> provided by <see cref="IDetector"/> implementation to calculate <see cref="IDetectorCheckResult"/> value and provide results in aggregate in <see cref="ToolRunResult"/>.
/// </summary>
public class DetectionEngine
{
    public event DetectionProgressChangedEventHandler? DetectionProgressChanged;

    public IReadOnlyList<DetectorDefinition> Detectors => _detectors;

    private List<DetectorDefinition> _detectors { get; init; } = new();

    public DetectionEngine(IEnumerable<IDetector> detectors)
    {
        foreach (IDetector detector in detectors)
        {
            _detectors.Add(detector.CreateDefinition());
        }
    }

    public async Task<ToolRunResult> DetectAgainstSourcesAsync(DataSourceCollection sources, CancellationToken cancellationToken)
    {
        int totalDetectors = _detectors.Count;
        int processedDetectors = 0;
        ConcurrentBag<DetectorResult> allDetectorResults = new();

        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion);

        try
        {
            // TODO: Do we want to have this be 1-step of progress?
            // Step 1. Initialize all the data sources.
            await Parallel.ForEachAsync(sources.Values.SelectMany(inner => inner), cancellationToken, async static (source, ct) =>
            {
                await source.LoadAndCacheDataAsync(ct);
            });

            result.AddDataSources(sources);

            // Step 2. Run all the detectors against the data sources.
            await Parallel.ForEachAsync(_detectors, cancellationToken, async (detector, cancellationToken) =>
            {
                // TODO: Probably parallelizing on the detectors is enough vs. each check

                DetectorResult detectorResult = new()
                {
                    DetectorName = detector.Info.Name,
                    DetectorDescription = detector.Info.Description,
                    DetectorVersion = AssemblyInfo.LibraryVersion,
                    FrameworkId = detector.Info.FrameworkId,
                    DetectorStatus = DetectorStatus.InProgress
                };

                // Required checks ALL need to pass for the framework to be detected and reported as such, from any single required group.
                // A detector must have 1 required check group (as otherwise it would always 'fail' to detect its framework)
                foreach (var requiredCheckGroup in detector.RequiredChecks ?? [])
                {
                    await Task.Yield();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Sanity CheckDefinition
                    if (requiredCheckGroup.Value is not DetectorCheckGroup dcg)
                    {
                        throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Required {requiredCheckGroup.Key} group is not a DetectorCheckGroup!");
                    }
                    else if (dcg.Count == 0)
                    {
                        throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Required {requiredCheckGroup.Key} group does not have any required checks!");
                    }

                    bool found = true;

                    foreach (var requiredCheck in dcg)
                    {
                        var innerResult = await requiredCheck.PerformCheckAsync(detector.Info, sources, cancellationToken);

                        if (requiredCheck == dcg.CheckWhichProvidesVersion && string.IsNullOrEmpty(detectorResult.FrameworkVersion) && dcg.VersionGetter is not null)
                        {
                            detectorResult.FrameworkVersion = dcg.VersionGetter.Invoke(innerResult);
                        }

                        // If any check fails then we fail to find the framework.
                        if (innerResult.CheckStatus != DetectorCheckStatus.CompletedPassed)
                        {
                            found = false;
                        }

                        detectorResult.CheckResults.Add(innerResult);
                    }

                    // If any of these required groups pass, then we've found the framework
                    if (found)
                    {
                        detectorResult.FrameworkFound = true;
                    }
                }

                // Optional checks won't fail the detection of the framework and are used to provide stronger confidence or additional metadata about the framework.
                // Each is its own set of additional checks under an extra tagged string metadata piece.
                foreach (var optionalCheckGroup in detector.OptionalChecks)
                {
                    await Task.Yield();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Sanity CheckDefinition
                    if (optionalCheckGroup.Value is not DetectorCheckGroup dcg)
                    {
                        throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Optional {optionalCheckGroup.Key} group is not a DetectorCheckGroup!");
                    }
                    else if (dcg.Count == 0)
                    {
                        throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Optional {optionalCheckGroup.Key} group does not have any required checks!");
                    }

                    foreach (var optionalCheck in dcg)
                    {
                        var innerResult = await optionalCheck.PerformCheckAsync(detector.Info, sources, cancellationToken);

                        detectorResult.CheckResults.Add(innerResult);
                    }

                    // TODO: We probably want to summarize/tally each optional check under its metadata that
                    // then says if that particular bucket was fully satisfied? This needs a bit more definition about what we want to use these for... (though I think even if we change this up this new API setup is pretty flexible to reconfigure to whatever our needs are).
                }

                detectorResult.DetectorStatus = cancellationToken.IsCancellationRequested ? DetectorStatus.Canceled: DetectorStatus.Completed;

                // Add to main collection of results.
                allDetectorResults.Add(detectorResult);

                // Update progress after each detector finishes
                lock (this)
                {
                    DetectionProgressChanged?.Invoke(this, new DetectionProgressChangedEventArgs(100.0 * Interlocked.Increment(ref processedDetectors) / totalDetectors));
                }
            });

        }
        catch (TaskCanceledException) { } // If it gets canceled, return what we found anyway

        // Step 3. Aggregate/Finalize all the results?
        result.DetectorResults = allDetectorResults.ToList();

        return result;
    }

    /// <summary>
    /// Dumps known info from DataSources without performing any detection logic.
    /// </summary>
    /// <param name="sources"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<ToolRunResult> DumpAgainstSourcesAsync(DataSourceCollection sources, CancellationToken cancellationToken)
    {
        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion);

        try
        {
            // Step 1. Initialize all the data sources.
            await Parallel.ForEachAsync(sources.Values.SelectMany(inner => inner), cancellationToken, async static (source, ct) =>
            {
                await source.LoadAndCacheDataAsync(ct);
            });

            result.AddDataSources(sources);

            // TODO: Is there more we have to do here atm?
        }
        catch (TaskCanceledException) { } // If it gets canceled, return what we found anyway

        return result;
    }
}
