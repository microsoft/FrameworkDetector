// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace FrameworkDetector.Engine;

public class DetectorCheckList
{
    // TODO: Figure out how to make these extension methods for each type of check...
    // They basically should setup an item to add to the DetectorDefinition which specifies the data source required, the piece of information, and how to invoke the check...
    // Is this merged with the DetectorDefinition itself (with another special interface)?

    internal DetectorCheckList ContainsClass(string v)
    {
        throw new NotImplementedException();
    }
}