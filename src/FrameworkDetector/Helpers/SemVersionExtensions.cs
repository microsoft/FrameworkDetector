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
            if (!string.IsNullOrWhiteSpace(version))
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
                        // Remove the fourth version number part and try again
                        version = string.Join('.', version.Split('.').Take(3));
                        return SemVersion.TryLooseParse(version, out semver);
                    }
                    else if (fex.Message.Contains("contains invalid character ' '"))
                    {
                        // Some version strings have extra content after a space, take just the version and try again
                        version = version.Split(' ')[0];
                        return SemVersion.TryLooseParse(version, out semver);
                    }
                    else if (fex.Message.Contains("contains invalid character ','"))
                    {
                        // Some version strings use ',' or ', ' instead of '.' as a delimiter, reformat and try again
                        version = version.Replace(", ", ".").Replace(',', '.');
                        return SemVersion.TryLooseParse(version, out semver);
                    }
#if DEBUG
                    else
                    {
                        // Log other unexpected formatting errors
                        Console.Error.WriteLine(fex.Message);
                    }
#endif
                }
                catch { }
            }

            semver = default;
            return false;
        }
    }
}
