// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FrameworkDetector.Checks;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;
using FrameworkDetector.Test.Detectors;

namespace FrameworkDetector.Test.Checks;

public abstract class CheckTestBase<TInput, TOutput>(
    Func<TInput, CheckRegistrationInfo<TInput, TOutput>> GetCheckRegistration
) where TInput : ICheckArgs
  where TOutput : struct
{
    public async Task RunCheck_ValidArgsAsync(IReadOnlyList<IInputType> inputs, TInput args, DetectorCheckStatus expectedCheckStatus, TOutput? expectedOutput, CancellationToken cancellationToken)
    {
        args.Validate();

        var checkRegistration = GetCheckRegistration(args);

        var checkDefinition = new CheckDefinition<TInput, TOutput>(checkRegistration, args);
        var actualResult = new DetectorCheckResult<TInput, TOutput>(new TestDetector(), checkDefinition);

        await checkRegistration.PerformCheckAsync(checkDefinition, inputs, actualResult, cancellationToken);

        Assert.AreEqual(expectedCheckStatus, actualResult.CheckStatus);
        Assert.AreEqual(expectedOutput is null, actualResult.CheckOutput is null, "Expected and actual outputs are both defined or not.");

        ValidateOutputData(expectedOutput, actualResult?.OutputData);
    }

    protected virtual void ValidateOutputData(TOutput? expected, TOutput? actual)
    {
        Assert.AreEqual(expected, actual);
    }
}
