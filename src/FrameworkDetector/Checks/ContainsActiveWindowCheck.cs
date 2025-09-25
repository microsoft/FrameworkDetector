// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

/// <summary>
/// CheckDefinition extension for looking for a specific window present within a process.
/// </summary>
public static class ContainsActiveWindowCheck
{
    /// <summary>
    /// Get registration information defining <see cref="ContainsActiveWindowCheck"/>.
    /// </summary>
    private static CheckRegistrationInfo<ContainsActiveWindowArgs, ContainsActiveWindowData> GetCheckRegistrationInfo(ContainsActiveWindowArgs args)
    {
        return new(
            Name: nameof(ContainsActiveWindowCheck),
            Description: args.GetDescription(),
            DataSourceIds: [ProcessDataSource.Id],
            PerformCheckAsync
            );
    }

    /// <summary>
    /// Input arguments for <see cref="ContainsActiveWindowCheck"/>.
    /// </summary>
    /// <param name="className">An active window's class name must contain this text, if specified.</param>
    /// <param name="text">An active window's text (title or caption) must contain this text, if specified.</param>
    public readonly struct ContainsActiveWindowArgs(string? className, string? text) : ICheckArgs
    {
        public string? ClassName { get; } = className;

        public string? Text { get; } = text;

        public string GetDescription()
        {
            var descriptionSB = new StringBuilder("Find window ");

            bool namedAdded = false;
            if (ClassName is not null)
            {
                descriptionSB.AppendFormat("{0}", ClassName);
                namedAdded = true;
            }

            if (Text is not null)
            {
                if (namedAdded)
                {
                    descriptionSB.Append(", ");
                }
                descriptionSB.AppendFormat("titled {0}", Text);
            }

            return descriptionSB.ToString();
        }

        public void Validate() => ArgumentNullException.ThrowIfNull(ClassName ?? Text, nameof(ContainsActiveWindowArgs));
    }

    /// <summary>
    /// Output data for <see cref="ContainsActiveWindowCheck"/>.
    /// </summary>
    /// <param name="windowFound">The window found.</param>
    public readonly struct ContainsActiveWindowData(ProcessWindowMetadata windowFound)
    {
        public ProcessWindowMetadata WindowFound { get; } = windowFound;
    }

    extension(DetectorCheckGroup @this)
    {
        /// <summary>
        /// Checks for an active window in the Process by class name or text.
        /// </summary>
        /// <param name="className">An active window's class name must contain this text, if specified.</param>
        /// <param name="text">An active window's text (title or caption) must contain this text, if specified.</param>
        /// <returns></returns>
        public DetectorCheckGroup ContainsActiveWindow(string? className = null, string? text = null)
        {
            // This copies over an entry pointing to this specific check's registration with the metadata requested by the detector.
            // The metadata along with the live data sources (as indicated by the registration)
            // will be passed into the PerformCheckAsync method below to do the actual check.
            var args = new ContainsActiveWindowArgs(className, text);
            args.Validate();
            @this.AddCheck(new CheckDefinition<ContainsActiveWindowArgs, ContainsActiveWindowData>(GetCheckRegistrationInfo(args), args));

            return @this;
        }
    }

    //// Actual check code run by engine

    public static async Task PerformCheckAsync(CheckDefinition<ContainsActiveWindowArgs, ContainsActiveWindowData> definition, DataSourceCollection dataSources, DetectorCheckResult<ContainsActiveWindowArgs, ContainsActiveWindowData> result, CancellationToken cancellationToken)
    {
        if (dataSources.TryGetSources(ProcessDataSource.Id, out ProcessDataSource[] processes))
        {
            result.CheckStatus = DetectorCheckStatus.InProgress;

            foreach (var process in processes)
            {
                var activeWindows = process.ProcessMetadata?.ActiveWindows;
                if (activeWindows is not null)
                {
                    foreach (var window in activeWindows)
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
        else
        {
            // No CheckInput = Error
            result.CheckStatus = DetectorCheckStatus.Error;
        }
    }
}
