// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Engine;

public enum DetectorCategory
{
    Framework,
    ProgrammingLanguage,
    Library,
    Component
}

public interface IDetector
{
    string Name { get; }

    string Description { get; }

    // TODO: Make this just a general Id?
    string FrameworkId { get; }

    DetectorCategory Category { get; }

    /// <summary>
    /// Main method to implement a detector's definition of requirements.
    /// </summary>
    /// <returns></returns>
    /// <seealso cref="IDetectorExtensions.Create(IDetector)"/>
    DetectorDefinition CreateDefinition();
}

public static class IDetectorExtensions
{
    extension(IDetector @this)
    {
        /// <summary>
        /// Static extension method for <see cref="IDetector"/>.
        /// </summary>
        /// <returns><see cref="IConfigSetupDetectorRequirements"/></returns>
        /// <example>
        /// <code>
        ///     public DetectorDefinition CreateDefinition()
        ///     {
        ///         // WPF
        ///         return this.Create()
        ///             .Required(checks => checks
        ///                 .ContainsModule("PresentationFramework.dll")
        ///                 .ContainsModule("PresentationCore.dll"))
        ///             .BuildDefinition();
        ///     }
        /// </code>
        /// </example>
        public IConfigSetupDetectorRequirements Create()
        {
            return new DetectorDefinition(@this);
        }
    }
}