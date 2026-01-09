// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

using Windows.ApplicationModel;

using FrameworkDetector.Models;

namespace FrameworkDetector;

public static class PackageExtensions
{
    /// <summary>
    /// Helper to get all package metadata recursively for a given installed Package and its dependencies.
    /// </summary>
    /// <param name="package">The target package.</param>
    /// <param name="includeTopLevelDependencyInfo">When true, includes information about the metadata of direct dependencies. Defaults to true to include the details of the top-most dependencies of the root/parent package when called without arguments.</param>
    /// <returns><see cref="PackageMetadata"/> block with available information about a <see cref="Package"/>.</returns>
    public static PackageMetadata GetMetadata(this Package package, bool includeTopLevelDependencyInfo = true)
    {
        // Note: There's a lot of paths, these seem most relevant?
        // Most Path locations only available 19041+, see Version History: https://learn.microsoft.com/uwp/api/windows.applicationmodel.package
        string installedPath = package.InstalledLocation.Path ?? "Unknown";
        string externalPath = "Unknown, Run on Windows 19041 or later.";
        string path = externalPath;
        bool? isStub = null;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
        {
            installedPath = package.InstalledPath ?? "Unknown";
            externalPath = package.EffectiveExternalPath ?? "Unknown";
            path = package.EffectivePath ?? "Unknown";
            isStub = package.IsStub;
        }
        else if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
        {
            externalPath = package.EffectiveLocation?.Path ?? "Unknown";
            path = package.EffectiveLocation is null ? installedPath : externalPath;
        }

        // Get Dependency Info only for the top-level package, not for dependencies of dependencies
        // Note: I don't think we should need to worry about dependencies of dependencies, as we should know what a top-level framework dependency is comprised of.
        PackageMetadata[] dependencies = [];
        if (includeTopLevelDependencyInfo)
        {
            dependencies = package.Dependencies
                              .Select(p => p.GetMetadata(false)) // false to not include the information of dependencies of dependencies (can get recursive, so this is just a first-level flag)
                              .ToArray();
        }

        // TODO: Check if this is maybe a run as admin thing?
        string publisherDisplayName = "Unavailable";
        string displayName = "Unavailable";
        string description = "Unavailable";
        try
        {
            publisherDisplayName = package.PublisherDisplayName;
            displayName = package.DisplayName;
            description = package.Description;
        }
        catch
        {
            // Sometimes this throws for some reason (0x80070490 not found?), so just ignore it
            // Doesn't seem documented that exception can be thrown, only maybe empty pre-19041.
            // https://learn.microsoft.com/uwp/api/windows.applicationmodel.package.publisherdisplayname#remarks
        }

        // Return all data
        return new PackageMetadata(
                        // Note: Author and ProductId are excluded, Windows Phone Only, as per docs
                        // https://learn.microsoft.com/uwp/api/windows.applicationmodel.packageid.author
                        new PackageIdentity(
                            package.Id.Architecture.ToString(),
                            package.Id.Name,
                            package.Id.FamilyName,
                            package.Id.FullName,
                            package.Id.Publisher,
                            package.Id.PublisherId,
                            package.Id.ResourceId,
                            $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}"
                        ),
                        publisherDisplayName,
                        displayName,
                        description,
                        installedPath,
                        externalPath,
                        path,
                        package.InstalledDate,
                        new PackageFlags(
                            package.IsBundle,
                            package.IsDevelopmentMode,
                            package.IsFramework,
                            package.IsOptional,
                            package.IsResourcePackage,
                            isStub
                        ),
                        dependencies);
    }
}
