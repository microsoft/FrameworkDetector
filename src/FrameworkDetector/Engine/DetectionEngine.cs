using FrameworkDetector.Checks;
using FrameworkDetector.DataSources;
using FrameworkDetector.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;

/// <summary>
/// Core handler of logic for running various <see cref="IDetector"/> defined detectors (as registered in service collection of parent runner detector application) against <see cref="IDataSource"/> sources provided by said app's configuration (e.g. processId to get Process info).
/// Will use defined <see cref="ICheckDefinition"/> provided by <see cref="IDetector"/> implementation to calculate <see cref="DetectorCheckResult"/> value and provide results in aggregate in <see cref="ToolRunResult"/>.
/// </summary>
public class DetectionEngine
{
    private List<DetectorDefinition> _detectors { get; init; } = new();

    public DetectionEngine(IEnumerable<IDetector> detectors)
    {
        foreach (IDetector detector in detectors)
        {
            _detectors.Add(detector.CreateDefinition());
        }
    }

    public async Task<ToolRunResult> DetectAgainstSourcesWithProgressAsync(DataSourceCollection sources, IProgress<int> progress, CancellationToken cancellationToken)
    {
        int totalDetectors = _detectors.Count;
        int processedDetectors = 0;
        ConcurrentBag<DetectorResult> allDetectorResults = new();

        // TODO: Do we want to have this be 1-step of progress?
        // Step 1. Initialize all the data sources.
        await Parallel.ForEachAsync(sources.Values.SelectMany(inner => inner), cancellationToken, async static (source, ct) =>
        {
            await source.LoadAndCacheDataAsync(ct);
        });

        // NOTE: We need to create the run result AFTER the data sources are loaded in case we output data source info in the results...
        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion, sources);

        // Step 2. Run all the detectors against the data sources.
        await Parallel.ForEachAsync(_detectors, cancellationToken, async (detector, cancellationToken) =>
        {
            // TODO: Probably parallelizing on the detectors is enough vs. each check

            DetectorResult detectorResult = new()
            {
                DetectorName = detector.Info.Name,
                // TODO: Do we want a version field on each detector, as otherwise if this is just the tool version then why also include it here?
                DetectorVersion = AssemblyInfo.LibraryVersion,
                FrameworkId = detector.Info.FrameworkId,
                Status = DetectorStatus.InProgress
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

                // Sanity Check
                if (requiredCheckGroup.Value.Count == 0)
                {
                    throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Required {requiredCheckGroup.Key} group does not have any required checks!");
                }

                bool found = true;

                foreach (var requiredCheck in requiredCheckGroup.Value)
                {
                    var innerResult = await requiredCheck.PerformCheckAsync(detector.Info, sources, cancellationToken);

                    // If any check fails then we fail to find the framework.
                    if (innerResult.Status != DetectorCheckStatus.CompletedPassed)
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

                foreach (var optionalCheck in optionalCheckGroup.Value)
                {
                    var innerResult = await optionalCheck.PerformCheckAsync(detector.Info, sources, cancellationToken);

                    detectorResult.CheckResults.Add(innerResult);
                }

                // TODO: We probably want to summarize/tally each optional check under its metadata that
                // then says if that particular bucket was fully satisfied? This needs a bit more definition about what we want to use these for... (though I think even if we change this up this new API setup is pretty flexible to reconfigure to whatever our needs are).
            }

            // Update progress after each detector finishes
            Interlocked.Increment(ref processedDetectors);
            progress.Report((processedDetectors * 100) / totalDetectors);

            // TODO: We need to mark cancelled status somewhere?
            detectorResult.Status = DetectorStatus.Completed;

            // Add to main collection of results.
            allDetectorResults.Add(detectorResult);
        });

        // Step 3. Aggregate/Finalize all the results?
        result.Detectors = allDetectorResults.ToList();

        return result;
    }
}
