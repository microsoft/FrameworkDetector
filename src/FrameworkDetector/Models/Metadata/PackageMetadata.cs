// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Windows.Management.Deployment;
using Windows.System;

using Windows.ApplicationModel;

namespace FrameworkDetector.Models;

/// <summary>
/// Represents metadata information for a software package (app or dependency), including identity, display details, installation paths,
/// flags, and dependencies (usually top-level only).
/// </summary>
/// <param name="Id">The unique identity of the package, including its name and version.</param>
/// <param name="PackagePublisherDisplayName">The display name of the publisher who provided the package.</param>
/// <param name="PackageDisplayName">The display name of the package as presented to users.</param>
/// <param name="PackageDescription">A description of the package, typically used for display in user interfaces.</param>
/// <param name="InstalledPath">The file system path where the package is installed.</param>
/// <param name="PackageEffectiveExternalPath">The effective external path for the package, which may be used for accessing package resources outside the
/// installation directory.</param>
/// <param name="PackageEffectivePath">The effective path for the package within the installation context.</param>
/// <param name="InstalledDate">The date and time when the package was installed.</param>
/// <param name="Flags">Flags that provide additional information about the package, such as its state or characteristics.</param>
/// <param name="Dependencies">An array of metadata for packages that this package depends on. The array is empty if there are no dependencies.</param>
public record PackageMetadata(PackageIdentity Id, string PackagePublisherDisplayName, string PackageDisplayName, string PackageDescription, string InstalledPath, string PackageEffectiveExternalPath, string PackageEffectivePath, DateTimeOffset InstalledDate, PackageFlags Flags, PackageMetadata[] Dependencies) { }

/// <summary>
/// Wrapper around <see cref="PackageId"/>.
/// </summary>
public record PackageIdentity(string Architecture, string Name, string FamilyName, string FullName, string Publisher, string PublisherId, string ResourceId, string Version) { }

/// <summary>
/// Encapsulates a set of flags that describe characteristics of a package, such as whether it is a bundle, a framework,
/// or intended for development or resource purposes, properties from <see cref="Package"/>.
/// </summary>
/// <param name="IsBundle">Indicates whether the package is a bundle containing multiple components.</param>
/// <param name="IsDevelopmentMode">Indicates whether the package is intended for use in development mode.</param>
/// <param name="IsFramework">Indicates whether the package is a framework package that provides shared functionality.</param>
/// <param name="IsOptional">Indicates whether the package is optional and not required for the application to function.</param>
/// <param name="IsResourcePackage">Indicates whether the package contains resources, such as localized content or assets.</param>
/// <param name="IsStub">Indicates whether the package is a stub. If <see langword="null"/>, the stub status is unspecified.</param>
public record PackageFlags(bool IsBundle, bool IsDevelopmentMode, bool IsFramework, bool IsOptional, bool IsResourcePackage, bool? IsStub) { }



