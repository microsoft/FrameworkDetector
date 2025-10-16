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
    /// The Regex used by <see cref="TryParseCleaned"/> to clean a s string before parsing.
    /// </summary>
    static readonly Regex CleanSemVersionRegex = new Regex(@"^v?((\d+)(\.|, |,)(\d+)?(\.|, |,)?(\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    extension (SemVersion)
    {
        /// <summary>
        /// Parses the given s string into a <see cref="SemVersion"/> after first cleaning the string of known problems.
        /// </summary>
        /// <param name="s">The string to clean and parse.</param>
        /// <param name="result">The parsed <see cref="SemVersion"/>, if successful.</param>
        /// <returns>Whether or not parsing succeeded.</returns>
        public static bool TryParseCleaned(string? s, out SemVersion? result)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var match = CleanSemVersionRegex.Match(s);
                if (match.Success)
                {
                    var major = int.TryParse(match.Groups[2].Value, out int p1) ? p1 : 0;
                    var minor = int.TryParse(match.Groups[4].Value, out int p2) ? p2 : 0;
                    var patch = int.TryParse(match.Groups[6].Value, out int p3) ? p3 : 0;
                    var cleanVersion = $"{major}.{minor}.{patch}";
                    return SemVersion.TryParse(cleanVersion, out result);
                }
            }

            result = default;
            return false;
        }
    }
}
