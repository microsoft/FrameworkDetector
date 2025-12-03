// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;

using PeNet;

using System.IO;

namespace FrameworkDetector;

public static class FileInfoExtensions
{
    extension(FileInfo @this)
    {
        /// <summary>
        /// Gets the metadata for the functions imported by the main module of the given process.
        /// </summary>
        /// <param name="process">The target process.</param>
        /// <returns>The metadata from each imported function.</returns>
        public IEnumerable<ExecutableImportedFunctionsMetadata> GetImportedFunctionsMetadata()
        {
            var importedFunctions = new HashSet<ExecutableImportedFunctionsMetadata>();

            if (TryGetCachedPeFile(@this.FullName, out var peFile) && peFile is not null)
            {
                lock (peFile)
                {
                    var tempMap = new Dictionary<string, List<FunctionMetadata>>();

                    if (peFile.ImportedFunctions is not null)
                    {
                        foreach (var importedFunction in peFile.ImportedFunctions)
                        {
                            if (!tempMap.TryGetValue(importedFunction.DLL, out var functions))
                            {
                                functions = new List<FunctionMetadata>();
                                tempMap[importedFunction.DLL] = functions;
                            }

                            if (importedFunction.Name is not null)
                            {
                                tempMap[importedFunction.DLL].Add(new FunctionMetadata(importedFunction.Name, false));
                            }
                        }
                    }

                    if (peFile.DelayImportedFunctions is not null)
                    {
                        foreach (var delayImportedFunction in peFile.DelayImportedFunctions)
                        {
                            if (!tempMap.TryGetValue(delayImportedFunction.DLL, out var functions))
                            {
                                functions = new List<FunctionMetadata>();
                                tempMap[delayImportedFunction.DLL] = functions;
                            }

                            if (delayImportedFunction.Name is not null)
                            {
                                tempMap[delayImportedFunction.DLL].Add(new FunctionMetadata(delayImportedFunction.Name, true));
                            }
                        }
                    }

                    foreach (var kvp in tempMap)
                    {
                        importedFunctions.Add(new ExecutableImportedFunctionsMetadata(kvp.Key, kvp.Value.ToArray()));
                    }
                }
            }

            return importedFunctions;
        }

        /// <summary>
        /// Gets the metadata for the functions exported by the main module of the given process.
        /// </summary>
        /// <param name="process">The target process.</param>
        /// <returns>The metadata from each exported function.</returns>
        public IEnumerable<ExecutableExportedFunctionsMetadata> GetExportedFunctionsMetadata()
        {
            var exportedFunctions = new HashSet<ExecutableExportedFunctionsMetadata>();

            if (TryGetCachedPeFile(@this.FullName, out var peFile) && peFile is not null)
            {
                lock (peFile)
                {
                    if (peFile.ExportedFunctions is not null)
                    {
                        foreach (var exportedFunction in peFile.ExportedFunctions)
                        {
                            if (exportedFunction is not null && exportedFunction.Name is not null)
                            {
                                exportedFunctions.Add(new ExecutableExportedFunctionsMetadata(exportedFunction.Name));
                            }
                        }
                    }
                }
            }

            return exportedFunctions;
        }
    }

    private static bool TryGetCachedPeFile(string filename, out PeFile? peFile)
    {
        PeFile? result = null;
        lock (_cachedPeFiles)
        {
            if (!_cachedPeFiles.TryGetValue(filename, out result))
            {
                // Cache whatever PeFile.TryParse gets, so we don't ever waste time reparsing a file
                PeFile.TryParse(filename, out var newPeFile);
                _cachedPeFiles.TryAdd(filename, newPeFile);
                result = newPeFile;
            }

            peFile = result;
            return result is not null;
        }
    }

    private static readonly ConcurrentDictionary<string, PeFile?> _cachedPeFiles = new ConcurrentDictionary<string, PeFile?>();
}

public record FunctionMetadata(string Name, bool? DelayLoaded = null);

public record ExecutableImportedFunctionsMetadata(string ModuleName, FunctionMetadata[]? Functions = null) { }

public record ExecutableExportedFunctionsMetadata(string Name) : FunctionMetadata(Name);
