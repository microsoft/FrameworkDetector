// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.RegularExpressions;

namespace FrameworkDetector;

internal static class VersionExtensions
{
    /// <summary>
    /// The Regex used by <see cref="TryParseCleaned"/> to clean a s string before parsing.
    /// </summary>
    static readonly Regex CleanVersionRegex = new Regex(@"^v?((\d+)(\.|, |,)(\d+)?(\.|, |,)?(\d+)?(\.|, |,)?(\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    extension (Version)
    {
        /// <summary>
        /// Parses the given s string into a <see cref="Version"/> after first cleaning the string of known problems.
        /// </summary>
        /// <param name="s">The string to clean and parse.</param>
        /// <param name="result">The parsed <see cref="Version"/>, if successful.</param>
        /// <returns>Whether or not parsing succeeded.</returns>
        public static bool TryParseCleaned(string? s, out Version? result)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var match = CleanVersionRegex.Match(s);
                if (match.Success)
                {
                    var major = int.TryParse(match.Groups[2].Value, out int p1) ? p1 : 0;
                    var minor = int.TryParse(match.Groups[4].Value, out int p2) ? p2 : 0;
                    var build = int.TryParse(match.Groups[6].Value, out int p3) ? p3 : 0;
                    var revision = int.TryParse(match.Groups[8].Value, out int p4) ? p4 : 0;

                    var cleanVersion = $"{major}.{minor}.{build}.{revision}";
                    return Version.TryParse(cleanVersion, out result);
                }
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    /// Converts the value of the current <see cref="Version"/> object to its equivalent <see cref="string"/> representation, removing trailing 0 parts.
    /// </summary>
    /// <param name="version">The version to convert.</param>
    /// <returns>The <see cref="string"/> representation.</returns>
    public static string ToShortString(this Version version)
    {
        if (version.Revision == 0)
        {
            if (version.Build == 0)
            {
                return version.ToString(2);
            }
            return version.ToString(3);
        }
        return version.ToString();
    }
}
