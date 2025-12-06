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
/// CheckDefinition extension for looking for a specific window present within a process.
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
    /// <param name="text">An active window's text (title or caption) must contain this text, if specified.</param>
    public readonly struct ContainsActiveWindowArgs(string? className = null, string? text = null) : ICheckArgs
    {
        public string? ClassName { get; } = className;

        public string? Text { get; } = text;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find window ");

            bool namedAdded = false;
            if (ClassName is not null)
            {
                descriptionSB.AppendFormat("class has \"{0}\"", ClassName);
                namedAdded = true;
            }

            if (Text is not null)
            {
                if (namedAdded)
                {
                    descriptionSB.Append(", ");
                }
                descriptionSB.AppendFormat("title has \"{0}\"", Text);
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(ClassName ?? Text, nameof(ContainsActiveWindowArgs));
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
        /// <param name="text">An active window's text (title or caption) must contain this text, if specified.</param>
        /// <returns></returns>
        public IDetectorCheckGroup ContainsActiveWindow(string? className = null, string? text = null)
        {
            var dcg = @this.Get();

            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            var args = new ContainsActiveWindowArgs(className, text);
            args.Validate();

            dcg.AddCheck(new CheckDefinition<ContainsActiveWindowArgs, ContainsActiveWindowData>(GetCheckRegistrationInfo(args), args));

            return dcg;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsActiveWindowArgs, ContainsActiveWindowData> definition, IReadOnlyList<IInputType> inputs, DetectorCheckResult<ContainsActiveWindowArgs, ContainsActiveWindowData> result, CancellationToken cancellationToken)
    {
        result.CheckStatus = DetectorCheckStatus.InProgress;

        foreach (var input in inputs)
        {
            if (input is IActiveWindowsDataSource dataSource
                && dataSource.ActiveWindows is not null)
            {
                foreach (var window in dataSource.ActiveWindows)
                {
                    await Task.Yield();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.CheckStatus = DetectorCheckStatus.Canceled;
                        break;
                    }

                    var classNameMatch = definition.CheckArguments.ClassName is null || window.ClassName is null || window.ClassName.Contains(definition.CheckArguments.ClassName, StringComparison.InvariantCultureIgnoreCase);
                    var textMatch = definition.CheckArguments.Text is null || window.Text is null || window.Text.Contains(definition.CheckArguments.Text, StringComparison.InvariantCultureIgnoreCase);

                    if (classNameMatch && textMatch)
                    {
                        result.OutputData = new ContainsActiveWindowData(window);
                        result.CheckStatus = DetectorCheckStatus.CompletedPassed;
                        break;
                    }
                }
            }

            // Stop evaluating other process data sources if we've gotten a pass or cancel
            if (result.CheckStatus != DetectorCheckStatus.InProgress)
            {
                break;
            }
        }

        if (result.CheckStatus == DetectorCheckStatus.InProgress)
        {
            result.CheckStatus = DetectorCheckStatus.CompletedFailed;
        }
    }
}
