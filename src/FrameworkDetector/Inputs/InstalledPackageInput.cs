// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> which represents a package retrieved from the installed package store on the system.
/// </summary>
public record InstalledPackageInput(string DisplayName,
                                    string Description,
                                    string FamilyName,
                                    PackageMetadata PackageMetadata) 
    : IEquatable<InstalledPackageInput>,
      IPackageMetadataDataSource,
      IInputTypeFactory<Package>,
      IInputType
{
    [JsonIgnore]
    public string InputGroup => "installedPackages";

    public static async Task<IInputType?> CreateAndInitializeDataSourcesAsync(Package package, bool? isLoaded, CancellationToken cancellationToken)
    {
        // No async initialization needed here yet, so just construct
        return new InstalledPackageInput(package.DisplayName,
                                         package.Description,
                                         package.Id.FamilyName,
                                         package.GetMetadata());
    }

    public override int GetHashCode() => PackageMetadata.GetHashCode();

    public virtual bool Equals(InstalledPackageInput? input)
    {
        if (input is null)
        {
            return false;
        }

        return PackageMetadata.Id == input.PackageMetadata.Id;
    }
}
