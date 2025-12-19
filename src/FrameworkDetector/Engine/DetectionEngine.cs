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
using FrameworkDetector.Inputs;
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

    /// <summary>
    /// Runs all configured detectors against the provided data sources asynchronously and returns the aggregated
    /// detection results.
    /// </summary>
    /// <remarks>Detection progress is reported via the DetectionProgressChanged event after each detector
    /// completes. If the operation is canceled, partial results are returned. This method is thread-safe and executes
    /// detector checks in parallel for improved performance.</remarks>
    /// <param name="sources">A collection of data sources to be analyzed by the detectors. Each source must be properly initialized and
    /// contain the relevant data for detection.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the detection operation before completion.</param>
    /// <param name="toolArguments">Optional arguments to record as being passed to the tool. null if no arguments metadata was provided.</param>
    /// <returns>A ToolRunResult containing the results of all detector runs, including metadata and individual detector
    /// outcomes.</returns>
    /// <exception cref="ArgumentException">Thrown if any required or optional check group for a detector does not contain at least one check.</exception>
    public async Task<ToolRunResult> DetectAgainstInputsAsync(IEnumerable<IInputType> inputs, CancellationToken cancellationToken, string? toolArguments = null)
    {
        int totalDetectors = _detectors.Count;
        int processedDetectors = 0;
        ConcurrentBag<DetectorResult> allDetectorResults = new();

        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion, toolArguments, inputs);

        try
        {
            // Step 1. Run all the detectors against the data sources.
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

                    var dcg = requiredCheckGroup.Value.Get();

                    // Sanity CheckDefinition
                    if (dcg.Count == 0)
                    {
                        throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Required {requiredCheckGroup.Key} group does not have any required checks!");
                    }

                    bool found = true;

                    foreach (var requiredCheck in dcg)
                    {
                        var innerResult = await requiredCheck.PerformCheckAsync(detector.Info, inputs, cancellationToken);

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

                    var dcg = optionalCheckGroup.Value.Get();

                    // Sanity CheckDefinition
                    if (dcg.Count == 0)
                    {
                        throw new ArgumentException($"Detector \"{detector.Info.Name}\"'s Optional {optionalCheckGroup.Key} group does not have any required checks!");
                    }

                    foreach (var optionalCheck in dcg)
                    {
                        var innerResult = await optionalCheck.PerformCheckAsync(detector.Info, inputs, cancellationToken);

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

        // Step 2. Aggregate/Finalize all the results?
        result.DetectorResults.AddRange(allDetectorResults.OrderBy(dr => dr.DetectorName));

        return result;
    }

    /// <summary>
    /// Dumps all available known info from Inputs and Data Sources without performing any detection logic.
    /// </summary>
    /// <param name="inputs">List of <see cref="IInputType"/> inputs to process and dump.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="toolArguments">Metadata to record of arguments used to run the tool.</param>
    /// <returns>Partial <see cref="ToolRunResult"/> object with data source information but without framework detection results.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<ToolRunResult> DumpAllDataFromInputsAsync(IEnumerable<IInputType> inputs, CancellationToken cancellationToken, string? toolArguments = null)
    {
        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion, toolArguments, inputs);

        try
        {
            // TODO: Is there more we have to do here atm?
        }
        catch (TaskCanceledException) { } // If it gets canceled, return what we found anyway

        return result;
    }
}
