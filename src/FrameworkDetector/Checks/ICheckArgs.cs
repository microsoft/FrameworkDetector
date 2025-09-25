// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Checks;


public interface ICheckArgs
{
    string GetDescription();

    void Validate();
}
