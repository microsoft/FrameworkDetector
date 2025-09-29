// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsExportedFunctionCheck;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsExportedFunctionCheckTest() : CheckTestBase<ContainsExportedFunctionArgs, ContainsExportedFunctionData>(GetCheckRegistrationInfo)
{
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
    [DataRow("?TestFunctionName@Type@TestModule@@YA_NXZ", "?TestFunctionName2@Type@TestModule@@YA_NXZ")]
    public async Task ContainsExportedFunctionCheck_FunctionNameNotFoundTest(string actualFunctionName, string functionNameToCheck)
    {
        await RunFunctionNameCheck([actualFunctionName], functionNameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    private async Task RunFunctionNameCheck(string[] actualFunctionNames, string functionNameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedFunctionName)
    {
        var actualExportedFunctions = actualFunctionNames.Select(name => new ProcessExportedFunctionsMetadata(name)).ToArray();
        var args = new ContainsExportedFunctionArgs(functionNameToCheck);

        ContainsExportedFunctionData? expectedOutput = expectedFunctionName is not null ? new ContainsExportedFunctionData(new ProcessExportedFunctionsMetadata(expectedFunctionName)) : null;

        var cts = new CancellationTokenSource();

        await RunTest(actualExportedFunctions, args, expectedCheckStatus, expectedOutput, cts.Token);
    }

    private async Task RunTest(ProcessExportedFunctionsMetadata[]? actualExportedFunctions, ContainsExportedFunctionArgs args, DetectorCheckStatus expectedCheckStatus, ContainsExportedFunctionData? expectedOutput, CancellationToken cancellationToken)
    {
        var dataSources = GetTestProcessDataSource(new ProcessMetadata(nameof(ContainsExportedFunctionCheckTest), ExportedFunctions: actualExportedFunctions));
        await RunCheck_ValidArgsAsync(dataSources, args, expectedCheckStatus, expectedOutput, cancellationToken);
    }
}
