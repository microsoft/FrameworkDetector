// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

using Semver;

namespace FrameworkDetector;

internal static class SemVersionExtensions
{
    extension (SemVersion semVersion)
    {
        public static bool TryLooseParse(string? version, out SemVersion? semver)
        {
            if (version is not null)
            {
                try
                {
                    semver = SemVersion.Parse(version, SemVersionStyles.Any);
                    return true;
                }
                catch (FormatException fex)
                {
                    if (fex.Message.StartsWith("Fourth version number"))
                    {
                        version = string.Join('.', version.Split('.').Take(3));
                        return SemVersion.TryParse(version, SemVersionStyles.Any, out semver);
                    }
                }
            }

            semver = default;
            return false;
        }
    }
}
