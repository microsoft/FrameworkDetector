using FrameworkDetector.DataSources;
using FrameworkDetector.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;
public class DetectionEngine
{
    private List<DetectorDefinition> _detectors { get; init; } = new();

    public DetectionEngine(IDetector[] detectors)
    {
        foreach (IDetector detector in detectors)
        {
            _detectors.Add(detector.CreateDefinition());
        }
    }

    // TODO: Have a progress type with more detailed information.
    public async Task<ToolRunResult> DetectAgainstSourcesWithProgressAsync(DataSourceCollection sources, IProgress<int> progress, CancellationToken ct)
    {
        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion, sources);

        foreach (var source in sources)
        {
            foreach (var detector in _detectors)
            {
                // TODO: Next, need to finish connecting the top here to the bottom.
                ////var detectionResult = await detector.DetectAsync(source);
                //// result.Merge(detectionResult);
            }
        }

        return result;
    }
}
