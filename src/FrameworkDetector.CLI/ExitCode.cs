// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.CLI;

public enum ExitCode
{
    Success = 0,
    ArgumentParsingError = 1,
    InspectFailed = 2,
    DumpFailed = 3,
    RunFailed = 4,
}
