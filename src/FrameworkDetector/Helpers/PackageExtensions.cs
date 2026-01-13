// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;

using Windows.ApplicationModel;
using Windows.Storage;

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
        // Get Dependency Info only for the top-level package, not for dependencies of dependencies
        // Note: I don't think we should need to worry about dependencies of dependencies, as we should know what a top-level framework dependency is comprised of.
        PackageMetadata[] dependencies = [];
        if (includeTopLevelDependencyInfo)
        {
            dependencies = package.Dependencies
                              .Select(p => p.GetMetadata(false)) // false to not include the information of dependencies of dependencies (can get recursive, so this is just a first-level flag)
                              .ToArray();
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
                        package.TryGetPublisherDisplayName(out var publisherDisplayName) && publisherDisplayName is not null ? publisherDisplayName : "",
                        package.TryGetDisplayName(out var displayName) && displayName is not null ? displayName : "",
                        package.TryGetDescription(out var description) && description is not null ? description : "",
                        // Note: There's a lot of paths, these seem most relevant?
                        // Most Path locations only available 19041+, see Version History: https://learn.microsoft.com/uwp/api/windows.applicationmodel.package
                        package.TryGetInstalledPath(out var installedPath) && installedPath is not null ? Path.ReplaceRootWithVariable(installedPath) : "",
                        package.TryGetEffectivePath(out var effectivePath) && effectivePath is not null ? Path.ReplaceRootWithVariable(effectivePath) : "",
                        package.TryGetEffectiveExternalPath(out var effectiveExternalPath) && effectiveExternalPath is not null ? Path.ReplaceRootWithVariable(effectiveExternalPath) : "",
                        package.TryGetInstalledDate(out var installedDate) && installedDate.HasValue ? installedDate.Value : DateTimeOffset.MinValue,
                        new PackageFlags(
                            package.IsBundle,
                            package.IsDevelopmentMode,
                            package.IsFramework,
                            package.IsOptional,
                            package.IsResourcePackage,
                            package.TryGetIsStub(out var isStub) ? isStub : null
                        ),
                        dependencies);
    }

    public static bool TryGetPublisherDisplayName(this Package package, out string? result)
    {
        try
        {
            result = package.PublisherDisplayName;
            return true;
        }
        catch
        {
            // Sometimes this throws for some reason (0x80070490 not found?), so just ignore it
            // Doesn't seem documented that exception can be thrown, only maybe empty pre-19041.
            // https://learn.microsoft.com/uwp/api/windows.applicationmodel.package.publisherdisplayname#remarks
        }

        result = default;
        return false;
    }

    public static bool TryGetDisplayName(this Package package, out string? result)
    {
        try
        {
            result = package.DisplayName;
            return true;
        }
        catch
        {
            // Sometimes this throws for some reason (0x80070490 not found?), so just ignore it
            // Doesn't seem documented that exception can be thrown, only maybe empty pre-19041.
            // https://learn.microsoft.com/uwp/api/windows.applicationmodel.package.displayname#remarks
        }

        result = default;
        return false;
    }

    public static bool TryGetDescription(this Package package, out string? result)
    {
        try
        {
            result = package.Description;
            return true;
        }
        catch
        {
            // Sometimes this throws for some reason (0x80070490 not found?), so just ignore it
            // Doesn't seem documented that exception can be thrown, only maybe empty pre-19041.
            // https://learn.microsoft.com/uwp/api/windows.applicationmodel.package.description#remarks
        }

        result = default;
        return false;
    }

    public static bool TryGetIsStub(this Package package, out bool? result)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                result = package.IsStub;
                return true;
            }
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetInstalledDate(this Package package, out DateTimeOffset? result)
    {
        try
        {
            result = package.InstalledDate;
            return true;
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetInstalledLocation(this Package package, out StorageFolder? result)
    {
        try
        {
            result = package.InstalledLocation;
            return true;
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetInstalledPath(this Package package, out string? result)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                result = package.InstalledPath;
                return true;
            }
            else if (package.TryGetInstalledLocation(out var installedLocation) && installedLocation is not null)
            {
                result = installedLocation.Path;
                return true;
            }
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetEffectiveLocation(this Package package, out StorageFolder? result)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
            {
                result = package.EffectiveLocation;
                return true;
            }
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetEffectivePath(this Package package, out string? result)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                result = package.EffectivePath;
                return true;
            }
            else if (package.TryGetEffectiveLocation(out var effectiveLocation) && effectiveLocation is not null)
            {
                result = effectiveLocation.Path;
                return true;
            }
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetEffectiveExternalLocation(this Package package, out StorageFolder? result)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                result = package.EffectiveExternalLocation;
                return true;
            }
        }
        catch { }

        result = default;
        return false;
    }

    public static bool TryGetEffectiveExternalPath(this Package package, out string? result)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                result = package.EffectiveExternalPath;
                return true;
            }
            else if (package.TryGetEffectiveExternalLocation(out var effectiveExternalLocation) && effectiveExternalLocation is not null)
            {
                result = effectiveExternalLocation.Path;
                return true;
            }
        }
        catch { }

        result = default;
        return false;
    }
}
