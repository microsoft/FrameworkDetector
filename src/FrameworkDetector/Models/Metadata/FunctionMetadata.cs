// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Models;

public record FunctionMetadata(string Name, bool? DelayLoaded = null);

public record ExecutableImportedFunctionsMetadata(string ModuleName, FunctionMetadata[]? Functions = null) { }

public record ExecutableExportedFunctionsMetadata(string Name) : FunctionMetadata(Name);
