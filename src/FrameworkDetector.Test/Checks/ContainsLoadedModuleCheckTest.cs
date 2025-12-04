// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsLoadedModuleCheck;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsLoadedModuleCheckTest() : CheckTestBase<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(GetCheckRegistrationInfo)
{
    [TestMethod]
    [DataRow("")]
    [DataRow("TestModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameFoundTest(string filename)
    {
        await RunFilenameCheck([filename], filename, DetectorCheckStatus.CompletedPassed, filename);
    }

    [TestMethod]
    [DataRow("", "TestModuleName.dll")]
    [DataRow("TestModuleName.dll", "WrongModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameNotFoundTest(string actualFilename, string filenameToCheck)
    {
        await RunFilenameCheck([actualFilename], filenameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    private async Task RunFilenameCheck(string[] actualFilenames, string filenameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedFilename)
    {
        var actualLoadedModules = actualFilenames.Select(filename => new WindowsModuleMetadata(filename)).ToArray();
        var args = new ContainsLoadedModuleArgs(filenameToCheck);

        ContainsLoadedModuleData? expectedOutput = expectedFilename is not null ? new ContainsLoadedModuleData(new WindowsModuleMetadata(expectedFilename)) : null;

        var cts = new CancellationTokenSource();

        await RunTest(actualLoadedModules, args, expectedCheckStatus, expectedOutput, cts.Token);
    }

    private async Task RunTest(WindowsModuleMetadata[]? actualLoadedModules, ContainsLoadedModuleArgs args, DetectorCheckStatus expectedCheckStatus, ContainsLoadedModuleData? expectedOutput, CancellationToken cancellationToken)
    {
        ProcessInput input = new(new(nameof(ContainsLoadedModuleCheckTest)), 
                                 ActiveWindows: [],
                                 Modules: actualLoadedModules ?? Array.Empty<WindowsModuleMetadata>());

        await RunCheck_ValidArgsAsync([input], args, expectedCheckStatus, expectedOutput, cancellationToken);
    }
}
