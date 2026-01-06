// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific window present within an input.
/// </summary>
public static class ContainsActiveWindowCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsActiveWindowCheck"/>.
    /// </summary>
    internal static CheckRegistrationInfo<ContainsActiveWindowArgs, ContainsActiveWindowData> GetCheckRegistrationInfo(ContainsActiveWindowArgs args)
    {
        return new(
            Name: nameof(ContainsActiveWindowCheck),
            Description: args.GetDescription(),
            DataSourceInterfaces: [typeof(IActiveWindowsDataSource)],
            PerformCheckAsync
            );
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsActiveWindowCheck"/>.
    /// </summary>
    /// <param name="className">An active window's class name must contain this text, if specified.</param>
    /// <param name="isVisible">An active window must have this visibility, if specified.</param>
    public readonly struct ContainsActiveWindowArgs(string? className = null, bool? isVisible = null) : ICheckArgs
    {
        public string? ClassName { get; } = className;

        public bool? IsVisible { get; } = isVisible;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find window ");

            bool namedAdded = false;
            if (ClassName is not null)
            {
                descriptionSB.AppendFormat("class has \"{0}\"", ClassName);
                namedAdded = true;
            }

            if (IsVisible is not null)
            {
                if (namedAdded)
                {
                    descriptionSB.Append(' ');
                }
                descriptionSB.AppendFormat("where it's \"{0}\"", IsVisible.Value ? "visible" : "not visible");
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(ClassName, nameof(ContainsActiveWindowArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsActiveWindowCheck"/>.
    /// </summary>
    /// <param name="windowFound">The window found.</param>
    public readonly struct ContainsActiveWindowData(ActiveWindowMetadata windowFound)
    {
        public ActiveWindowMetadata WindowFound { get; } = windowFound;
    }

    extension(IDetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an active window in the Process by class name or text.
        /// </summary>
        /// <param name="className">An active window's class name must contain this text, if specified.</param>
        /// <param name="isVisible">An active window must have this visibility, if specified.</param>
        /// <returns></returns>
        public IDetectorCheckGroup ContainsActiveWindow(string? className = null, bool? isVisible = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            var args = new ContainsActiveWindowArgs(className, isVisible);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsActiveWindowArgs, ContainsActiveWindowData>(GetCheckRegistrationInfo(args), args));

            return dcg;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsActiveWindowArgs, ContainsActiveWindowData> definition, IEnumerable<IInputType> inputs, DetectorCheckResult<ContainsActiveWindowArgs, ContainsActiveWindowData> result, CancellationToken cancellationToken)
    {
        result.CheckStatus = DetectorCheckStatus.InProgress;

        foreach (var input in inputs)
        {
            // Stop evaluating inputs if we've gotten a cancellation or a result
            if (cancellationToken.IsCancellationRequested || result.CheckStatus != DetectorCheckStatus.InProgress) break;

            if (input is IActiveWindowsDataSource dataSource)
            {
                await foreach (var window in dataSource.GetActiveWindowsAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var classNameMatch = definition.CheckArguments.ClassName is null || window.ClassName is null || window.ClassName.Contains(definition.CheckArguments.ClassName, StringComparison.InvariantCultureIgnoreCase);

                    var isVisibleMatch = definition.CheckArguments.IsVisible is null || definition.CheckArguments.IsVisible == window.IsVisible;

                    if (classNameMatch)
                    {
                        result.OutputData = new ContainsActiveWindowData(window);
                        result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                        break;
                    }
                }
            }
        }

        if (result.CheckStatus == DetectorCheckStatus.InProgress)
        {
            result.CheckStatus = cancellationToken.IsCancellationRequested ? DetectorCheckStatus.Canceled : DetectorCheckStatus.CompletedFailed;
        }
    }
}
