// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsExportedFunctionCheck;
using FrameworkDetector.DataSources;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsExportedFunctionCheckTest() : CheckTestBase<ContainsExportedFunctionArgs, ContainsExportedFunctionData>(GetCheckRegistrationInfo)
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "?TestFunctionName@")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "@Type@")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "@TestModule@")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "@@YA_NXZ")]
    public async Task ContainsExportedFunctionCheck_FunctionNameFoundTest(string functionName, string? functionNameToCheck = null)
    {
        await RunFunctionNameCheck([functionName], functionNameToCheck ?? functionName, DetectorCheckStatus.CompletedPassed, functionName);
    }

    [TestMethod]
    [DataRow("", "?TestFunctionName@Type@TestModule@@YA_NXZ")]
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "?WrongFunctionName@Type@TestModule@@YA_NXZ")]
    public async Task ContainsExportedFunctionCheck_FunctionNameNotFoundTest(string actualFunctionName, string functionNameToCheck)
    {
        await RunFunctionNameCheck([actualFunctionName], functionNameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    private async Task RunFunctionNameCheck(string[] actualFunctionNames, string functionNameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedFunctionName)
    {
        var actualExportedFunctions = actualFunctionNames.Select(name => new ExportedFunctionsMetadata(name)).ToArray();
        var args = new ContainsExportedFunctionArgs(functionNameToCheck);

        ContainsExportedFunctionData? expectedOutput = expectedFunctionName is not null ? new ContainsExportedFunctionData(new ExportedFunctionsMetadata(expectedFunctionName)) : null;

        var input = new ExportedFunctionsTestInput(actualExportedFunctions ?? Array.Empty<ExportedFunctionsMetadata>());

        await RunCheck_ValidArgsAsync([input], args, expectedCheckStatus, expectedOutput);
    }

    private record ExportedFunctionsTestInput(ExportedFunctionsMetadata[] ExportedFunctions) : IInputType, IExportedFunctionsDataSource
    {
        public string InputGroup => nameof(ExportedFunctionsTestInput);

        public IEnumerable<ExportedFunctionsMetadata> GetExportedFunctions() => ExportedFunctions;
    }
}
