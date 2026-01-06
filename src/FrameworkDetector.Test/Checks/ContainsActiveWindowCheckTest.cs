// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsActiveWindowCheck;
using FrameworkDetector.DataSources;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsActiveWindowCheckTest() : CheckTestBase<ContainsActiveWindowArgs, ContainsActiveWindowData>(GetCheckRegistrationInfo)
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("")]
    [DataRow("TestWindowClassName")]
    [DataRow("TestWindowClassFullName", "")]
    [DataRow("TestWindowClassFullName", "TestWindowClass")]
    [DataRow("TestWindowClassFullName", "WindowClassFull")]
    [DataRow("TestWindowClassFullName", "ClassFullName")]
    public async Task ContainsActiveWindowCheck_WindowClassFoundTest(string className, string? classNameToCheck = null)
    {
        await RunWindowClassTest([className], classNameToCheck ?? className, DetectorCheckStatus.CompletedPassed, className);
    }

    [TestMethod]
    [DataRow("", "TestWindowClassName")]
    [DataRow("TestWindowClassName", "WrongWindowClassName")]
    public async Task ContainsActiveWindowCheck_WindowClassNotFoundTest(string actualClassName, string classNameToCheck)
    {
        await RunWindowClassTest([actualClassName], classNameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    private async Task RunWindowClassTest(string[] actualWindowClassNames, string classNameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedWindowClassName)
    {
        var actualWindows = actualWindowClassNames.Select(className => new ActiveWindowMetadata(className)).ToArray();
        var args = new ContainsActiveWindowArgs(classNameToCheck);

        ContainsActiveWindowData? expectedOutput = expectedWindowClassName is not null ? new ContainsActiveWindowData(new ActiveWindowMetadata(expectedWindowClassName)) : null;

        var input = new ActiveWindowsTestInput(actualWindows);

        await RunCheck_ValidArgsAsync([input], args, expectedCheckStatus, expectedOutput);
    }

    private record ActiveWindowsTestInput(ActiveWindowMetadata[] ActiveWindows) : IInputType, IActiveWindowsDataSource
    {
        public string InputGroup => nameof(ActiveWindowsTestInput);

        public IEnumerable<ActiveWindowMetadata> GetActiveWindows() => ActiveWindows;
    }
}
