// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Models;

public record FunctionMetadata(string Name, bool? DelayLoaded = null);

public record ImportedFunctionsMetadata(string ModuleName, FunctionMetadata[] Functions) { }

public record ExportedFunctionsMetadata(string Name) : FunctionMetadata(Name);
