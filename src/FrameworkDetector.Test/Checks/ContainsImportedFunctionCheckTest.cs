// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsImportedFunctionCheck;
using FrameworkDetector.DataSources;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsImportedFunctionCheckTest() : CheckTestBase<ContainsImportedFunctionArgs, ContainsImportedFunctionData>(GetCheckRegistrationInfo)
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("TestModule.dll", "")]
    [DataRow("TestModule.dll", "TestFunctionName")]
    [DataRow("TestModule.dll", "TestFunctionName", "TestModule.dll", "")]
    [DataRow("TestModule.dll", "TestFunctionName", "TestModule.dll", "TestFunction")]
    [DataRow("TestModule.dll", "TestFunctionName", "TestModule.dll", "Function")]
    [DataRow("TestModule.dll", "TestFunctionName", "TestModule.dll", "FunctionName")]
    public async Task ContainsImportedFunctionCheck_FunctionNameFoundTest(string moduleName, string functionName, string? moduleNameToCheck = null, string? functionNameToCheck = null)
    {
        await RunFunctionNameCheck(moduleName, [functionName], moduleNameToCheck ?? moduleName, functionNameToCheck ?? functionName, DetectorCheckStatus.CompletedPassed, moduleName, functionName);
    }

    [TestMethod]
    [DataRow("TestModule.dll", "", "TestModule.dll", "TestFunctionName")]
    [DataRow("TestModule.dll", "TestFunctionName", "TestModule.dll", "WrongFunctionName")]
    public async Task ContainsImportedFunctionCheck_FunctionNameNotFoundTest(string actualModuleName, string actualFunctionName, string moduleNameToCheck, string functionNameToCheck)
    {
        await RunFunctionNameCheck(actualModuleName, [actualFunctionName], moduleNameToCheck, functionNameToCheck, DetectorCheckStatus.CompletedFailed, null, null);
    }

    private async Task RunFunctionNameCheck(string actualModuleName, string[] actualFunctionNames, string moduleNameToCheck, string functionNameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedModuleName, string? expectedFunctionName)
    {
        var actualImportedFunctions = new ImportedFunctionsMetadata(actualModuleName, actualFunctionNames.Select(afn => new FunctionMetadata(afn)).ToArray());
        var args = new ContainsImportedFunctionArgs(moduleNameToCheck, functionNameToCheck);

        ContainsImportedFunctionData? expectedOutput = expectedModuleName is not null && expectedFunctionName is not null ? new ContainsImportedFunctionData(new ImportedFunctionsMetadata(expectedModuleName, [new FunctionMetadata(expectedFunctionName)])) : null;

        var input = new ImportedFunctionsTestInput([actualImportedFunctions]);

        await RunCheck_ValidArgsAsync([input], args, expectedCheckStatus, expectedOutput);
    }

    private record ImportedFunctionsTestInput(ImportedFunctionsMetadata[] ImportedFunctions) : IInputType, IImportedFunctionsDataSource
    {
        public string InputGroup => nameof(ImportedFunctionsTestInput);

        public IEnumerable<ImportedFunctionsMetadata> GetImportedFunctions() => ImportedFunctions;
    }

    protected override void ValidateOutputData(ContainsImportedFunctionData? expected, ContainsImportedFunctionData? actual)
    {
        if (expected is not null && actual is not null)
        {
            var expectedFunction = expected.Value.ImportedFunctionFound;
            var actualFunction = actual.Value.ImportedFunctionFound;

            Assert.AreEqual(expectedFunction.ModuleName, actualFunction.ModuleName);
            Assert.AreEqual(expectedFunction.Functions is null, actualFunction.Functions is null);

            if (expectedFunction.Functions is not null)
            {
                Assert.AreEqual(expectedFunction.Functions.Length, actualFunction.Functions?.Length ?? 0);

                for (int i = 0; i < expectedFunction.Functions.Length; i++)
                {
                    Assert.AreEqual(expectedFunction.Functions[i], actualFunction.Functions?[i]);
                }
            }
        }
    }
}
