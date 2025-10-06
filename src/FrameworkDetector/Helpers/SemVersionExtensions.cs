// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.RegularExpressions;

using Semver;

namespace FrameworkDetector;

internal static class SemVersionExtensions
{
    /// <summary>
    /// The Regex used by <see cref="TryParseCleaned"/> to clean a version string before parsing.
    /// </summary>
    static readonly Regex CleanVersionRegex = new Regex(@"^v?((\d+)(\.|, |,)(\d+)?(\.|, |,)?(\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    extension (SemVersion semVersion)
    {
        /// <summary>
        /// Parses the given version string into a <see cref="SemVersion"/> after first cleaning the string of known problems.
        /// </summary>
        /// <param name="version">The string to clean and parse.</param>
        /// <param name="semver">The parsed <see cref="SemVersion"/> version, if successful.</param>
        /// <returns>Whether or not parsing succeeded.</returns>
        public static bool TryParseCleaned(string? version, out SemVersion? semver)
        {
            if (!string.IsNullOrEmpty(version))
            {
                var match = CleanVersionRegex.Match(version);
                if (match.Success)
                {
                    var major = string.IsNullOrEmpty(match.Groups[2].Value) ? "0" : match.Groups[2].Value;
                    var minor = string.IsNullOrEmpty(match.Groups[4].Value) ? "0" : match.Groups[4].Value;
                    var patch = string.IsNullOrEmpty(match.Groups[6].Value) ? "0" : match.Groups[6].Value;
                    var cleanVersion = $"{major}.{minor}.{patch}";
                    return SemVersion.TryParse(cleanVersion, out semver);
                }
            }

            semver = default;
            return false;
        }
    }
}
