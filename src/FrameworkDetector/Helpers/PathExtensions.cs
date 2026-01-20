// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrameworkDetector;

public static class PathExtensions
{
    extension(Path)
    {
        /// <summary>
        /// Takes an absolute path and, if possible, replaces its root with an environment variable or <see cref="Environment.SpecialFolder"/> placeholder.
        /// </summary>
        /// <param name="path">An absolute path.</param>
        /// <returns>The path with the root replaced.</returns>
        public static string ReplaceRootWithVariable(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path))
            {
                //// First check for paths that can be resolved with environment variables
                foreach (var envPath in EnvironmentFolderPaths)
                {
                    if (path.StartsWith(envPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return path.Replace(envPath, $"%{EnvironmentFolderPathToId[envPath]}%", StringComparison.InvariantCultureIgnoreCase);
                    }
                }

                // Then check for paths that could be resolved with special folders
                foreach (var specialPath in SpecialFolderPaths)
                {
                    if (path.StartsWith(specialPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return path.Replace(specialPath, $"[{SpecialFolderPathToId[specialPath]}]", StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Tries to find if a given file supposedly in <paramref name="fromRoot"/> is actually redirecting to a file present in <paramref name="toRoot"/>.
        /// </summary>
        /// <param name="filename">Path to the target file.</param>
        /// <param name="fromRoot">The root where the target file is supposedly.</param>
        /// <param name="toRoot">The root where the file msy actually be.</param>
        /// <param name="newFilename">Path to the actual file in <paramref name="toRoot"/>.</param>
        /// <returns>Whether or not the file was found in <paramref name="toRoot"/>.</returns>
        public static bool TryFindRedirectedFile(string filename, Environment.SpecialFolder fromRoot, Environment.SpecialFolder toRoot, out string? newFilename)
        {
            return TryFindRedirectedFile(filename, Environment.GetFolderPath(fromRoot), Environment.GetFolderPath(toRoot), out newFilename);
        }

        /// <summary>
        /// Tries to find if a given file supposedly in <paramref name="fromRoot"/> is actually redirecting to a file present in <paramref name="toRoot"/>.
        /// </summary>
        /// <param name="filename">Path to the target file.</param>
        /// <param name="fromRoot">The root where the target file is supposedly.</param>
        /// <param name="toRoot">The root where the file msy actually be.</param>
        /// <param name="newFilename">Path to the actual file in <paramref name="toRoot"/>.</param>
        /// <returns>Whether or not the file was found in <paramref name="toRoot"/>.</returns>
        public static bool TryFindRedirectedFile(string filename, string fromRoot, string toRoot, out string? newFilename)
        {
            var oldPath = filename;
            if (oldPath.StartsWith(fromRoot, StringComparison.InvariantCultureIgnoreCase))
            {
                var newPath = Path.Join(toRoot, Path.GetRelativePath(fromRoot, oldPath));
                if (File.Exists(newPath))
                {
                    newFilename = newPath;
                    return true;
                }
            }

            newFilename = default;
            return false;
        }
    }

    /// <summary>
    /// List of paths that can be replaced with environment variable placeholders, sorted descending by length.
    /// </summary>
    public static readonly IReadOnlyList<string> EnvironmentFolderPaths = EnvironmentFolderPathToId.Keys.OrderByDescending(k => k.Length).ToList();

    /// <summary>
    /// Dictionary mapping paths to their environment variable placeholders.
    /// </summary>
    public static IReadOnlyDictionary<string, string> EnvironmentFolderPathToId
    {
        get
        {
            if (_environmentFolderPathToId is null)
            {
                var environmentVariables = Environment.GetEnvironmentVariables();

                var environmentFolders = new Dictionary<string, string>();
                foreach (string key in environmentVariables.Keys)
                {
                    var path = environmentVariables[key] as string;

                    if (!string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path))
                    {
                        environmentFolders[path] = key;
                    }
                }
                _environmentFolderPathToId = environmentFolders;
            }
            return _environmentFolderPathToId;
        }
    }

    private static Dictionary<string, string>? _environmentFolderPathToId = null;

    /// <summary>
    /// List of paths that can be replaced with <see cref="Environment.SpecialFolder"/> placeholders, sorted descending by length.
    /// </summary>
    public static readonly IReadOnlyList<string> SpecialFolderPaths = SpecialFolderPathToId.Keys.OrderByDescending(k => k.Length).ToList();

    /// <summary>
    /// Dictionary mapping paths to their <see cref="Environment.SpecialFolder"/> placeholders.
    /// </summary>
    public static IReadOnlyDictionary<string, string> SpecialFolderPathToId
    {
        get
        {
            if (_specialFolderPathToId is null)
            {
                var specialFolders = new Dictionary<string, string>();
                foreach (var specialFolder in Enum.GetValues<Environment.SpecialFolder>())
                {
                    var path = Environment.GetFolderPath(specialFolder);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        specialFolders[path] = specialFolder.ToString();
                    }
                }
                _specialFolderPathToId = specialFolders;
            }
            return _specialFolderPathToId;
        }
    }

    private static Dictionary<string, string>? _specialFolderPathToId = null;
}
