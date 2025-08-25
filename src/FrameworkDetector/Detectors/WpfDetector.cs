// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DetectorChecks;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Detectors;

public class WpfDetector : IDetector 
{
    public string Name => nameof(WpfDetector);

    public string Description => "Windows Presentation Framework";

    public string FrameworkId => "WPF";

    public WpfDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        // WPF
        return this.Create()
            .Required(checks => checks
                .ContainsModule("PresentationFramework.dll")
                .ContainsModule("PresentationCore.dll"))
            .BuildDefinition();
    }
}
