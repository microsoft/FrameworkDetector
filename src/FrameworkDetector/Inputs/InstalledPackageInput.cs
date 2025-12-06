// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    : IPackageMetadataDataSource,
      IInputTypeFactory<Package>,
      IInputType
{
    public string Name => "installedPackages";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(Package package, bool? isLoaded, CancellationToken cancellationToken)
    {
        // No async initialization needed here yet, so just construct
        return new InstalledPackageInput(package.DisplayName,
                                         package.Description,
                                         package.Id.FamilyName,
                                         package.GetMetadata());
    }
}
