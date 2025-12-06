// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsExportedFunctionCheck;
using FrameworkDetector.Inputs;
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

        var cts = new CancellationTokenSource();

        await RunTest(actualExportedFunctions, args, expectedCheckStatus, expectedOutput, cts.Token);
    }

    private async Task RunTest(ExportedFunctionsMetadata[]? actualExportedFunctions, ContainsExportedFunctionArgs args, DetectorCheckStatus expectedCheckStatus, ContainsExportedFunctionData? expectedOutput, CancellationToken cancellationToken)
    {
        ExecutableInput input = new(new(nameof(ContainsExportedFunctionCheckTest)),
                                    ImportedFunctions: [],
                                    ExportedFunctions: actualExportedFunctions ?? Array.Empty<ExportedFunctionsMetadata>(),
                                    Modules: []);

        await RunCheck_ValidArgsAsync([input], args, expectedCheckStatus, expectedOutput, cancellationToken);
    }
}
