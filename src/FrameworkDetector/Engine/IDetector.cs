// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Engine;

public interface IDetector
{
    string Name { get; }

    string Description { get; }

    string FrameworkId { get; }

    /// <summary>
    /// 
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
        /// <returns><see cref="IConfigDetectorRequirements"/></returns>
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
        public IConfigDetectorRequirements Create()
        {
            return new DetectorDefinition(@this);
        }
    }
}