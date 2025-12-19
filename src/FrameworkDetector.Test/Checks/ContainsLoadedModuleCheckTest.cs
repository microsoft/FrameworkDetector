// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsModuleCheck;
using FrameworkDetector.DataSources;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;
using System.Collections.Generic;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsLoadedModuleCheckTest() : CheckTestBase<ContainsModuleArgs, ContainsModuleData>(GetCheckRegistrationInfo)
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("")]
    [DataRow("TestModuleName.dll")]
    public async Task ContainsModuleCheck_FilenameFoundTest(string filename)
    {
        // Note: We pass null as that sets the isLoaded to null to mean to indicate we don't care if it is loaded or not (default)
        await RunFilenameCheck([filename], false, filename, DetectorCheckStatus.CompletedPassed, filename, null);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("TestModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameFoundTest(string filename)
    {
        await RunFilenameCheck([filename], true, filename, DetectorCheckStatus.CompletedPassed, filename, true);
    }

    [TestMethod]
    [DataRow("", "TestModuleName.dll")]
    [DataRow("TestModuleName.dll", "WrongModuleName.dll")]
    public async Task ContainsModuleCheck_FilenameNotFoundTest(string actualFilename, string filenameToCheck)
    {
        await RunFilenameCheck([actualFilename], false, filenameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    [TestMethod]
    [DataRow("", true, "TestModuleName.dll")]
    [DataRow("TestModuleName.dll", true, "WrongModuleName.dll")]
    [DataRow("TestModuleName.dll", false, "TestModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameNotFoundTest(string actualFilename, bool isLoaded, string filenameToCheck)
    {
        await RunFilenameCheck([actualFilename], isLoaded, filenameToCheck, DetectorCheckStatus.CompletedFailed, null, true);
    }

    [TestMethod]
    [DataRow("TestModuleName.dll", true, "TestModuleName.dll")]
    [DataRow("TestModuleName.dll", false, "TestModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameNotLoadedFlagTest(string actualFilename, bool isLoaded, string filenameToCheck)
    {
        // IsLoaded = false: should only match if module.IsLoaded == false
        var expectedStatus = isLoaded ? DetectorCheckStatus.CompletedFailed : DetectorCheckStatus.CompletedPassed;
        var expectedOutput = isLoaded ? null : filenameToCheck;
        await RunFilenameCheck([actualFilename], isLoaded, filenameToCheck, expectedStatus, expectedOutput, false);
    }

    private async Task RunFilenameCheck(string[] actualFilenames, bool areLoaded, string filenameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedFilename, bool? checkIsLoaded = null)
    {
        var actualLoadedModules = actualFilenames.Select(filename => new WindowsModuleMetadata(filename, IsLoaded: areLoaded == true)).ToArray();
        var args = new ContainsModuleArgs(filenameToCheck, isLoaded: checkIsLoaded);

        ContainsModuleData? expectedOutput = expectedFilename is not null ? new ContainsModuleData(new WindowsModuleMetadata(expectedFilename, IsLoaded: areLoaded == true)) : null;

        var input = new ModulesTestInput(actualLoadedModules);

        await RunCheck_ValidArgsAsync([input], args, expectedCheckStatus, expectedOutput);
    }

    private record ModulesTestInput(WindowsModuleMetadata[] Modules) : IInputType, IModulesDataSource
    {
        public string InputGroup => nameof(ModulesTestInput);

        public IEnumerable<WindowsModuleMetadata> GetModules() => Modules;
    }
}
