// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkDetector.Test;

[TestClass]
public class ProcessExtensionsTest
{
    [TestMethod]
    public void ProcessExtensions_ThisProcessChildrenTest()
    {
        var children = Process.GetCurrentProcess().GetChildProcesses();
        Assert.IsNotNull(children);
    }
}
