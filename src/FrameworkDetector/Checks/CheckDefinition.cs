// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.Engine;
using FrameworkDetector.Inputs;
using FrameworkDetector.Models;


namespace FrameworkDetector.Checks;

//// This is runtime/definition information about checks to be performed for a detector.

/// <summary>
/// Base interface for <see cref="CheckDefinition{TInput,TOutput}"/> for common information about a performed check.
/// </summary>
public interface ICheckDefinition
{
    // TODO: Wondering if this is actually required to define by the check as currently we're just passing through all the data sources we have anyway... The check author has to grab the specific one they need anyway which they do through the IDataSource.Id right now anyway, so this is just more of a goodfaith declaration. It could be an extra check we do where if the data source is missing we throw an error or warning and don't run the check?
    /// <summary>
    /// Gets a list of required datasource interface type names this check expects to access to be able to run.
    /// </summary>
    public string[] DataSources { get; }

    /// <summary>
    /// Gets the description to be used as a ToString format with the ProcessMetadata.ToString() as a parameter.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the short name of the check.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a flag indicating if this was a required check. Set automatically by <see cref="DetectorDefinition"/>.
    /// </summary>
    public bool IsRequired { get; set; }

    //// TODO: This is defined by the detector (not the check) as extra info about what it's looking for as an optional package. We could make this a more complex type, but not sure what other info we want at the moment. This would then change the signature to the DetectorDefinition.Optional method referenced below.
    /// <summary>
    /// Gets or sets the name of the group this check is defined within by the detector. Set automatically.
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Performs the defined check against the provided <see cref="IInputType"/> collection.
    /// </summary>
    /// <param name="detector">The detector requesting the check.</param>
    /// <param name="inputs">Complete set of <see cref="IInputType"/> to process for analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public Task<IDetectorCheckResult> PerformCheckAsync(IDetector detector, IEnumerable<IInputType> inputs, CancellationToken cancellationToken);
}

/// <summary>
/// Runtime record created by a detector which links the specific check extension info (through its extension method to DetectorCheckList) with the specific arguments to be checked against by a particular detector.
/// e.g. WPF needs to look for a specific dll.
/// </summary>
/// <typeparam name="TInput">Type of input arguments struct used by the check when running, e.g. the specific module to search for.</typeparam>
/// <typeparam name="TOutput">Type of output data struct for storing anyout output data from a check.</typeparam>
/// <param name="CheckRegistration">Reference to the specific registration of the check creating this entry.</param>
/// <param name="CheckArguments">Input arguments provided by a detector for this check to be passed in when executed. Included automatically within the <see cref="DetectorCheckResult{TInput,TOutput}"/>.</param>
public record CheckDefinition<TInput,TOutput>(
    CheckRegistrationInfo<TInput,TOutput> CheckRegistration,
    TInput CheckArguments
) : ICheckDefinition where TInput : ICheckArgs
                     where TOutput : struct
{
    /// <inheritdoc/>
    public string Name => CheckRegistration.Name;

    /// <inheritdoc/>
    public string Description => CheckRegistration.Description;

    /// <inheritdoc/>
    public string[] DataSources => CheckRegistration.DataSourceInterfaces.Select(t => t.Name).ToArray();

    private CheckFunction<TInput,TOutput> PerformCheckAsync => CheckRegistration.PerformCheckAsync;

    /// <inheritdoc/>
    public bool IsRequired { get; set; }

    /// <inheritdoc/>
    public string? GroupName { get; set; }

    //// Used to translate between the strongly-typed definition written by check extension author passed in as a delegate and the concreate generalized version the engine will call on the check.
    /// <inheritdoc/>
    async Task<IDetectorCheckResult> ICheckDefinition.PerformCheckAsync(IDetector detector, IEnumerable<IInputType> inputs, CancellationToken cancellationToken)
    {
        // Create initial result holder linking the detector to this check being performed.
        // Auto includes the additional arguments required by the check defined by the detector (and used by the check).
        DetectorCheckResult<TInput,TOutput> result = new(detector, this)
        {
            InputArgs = CheckArguments
        };

        // Call the check extension to perform calculation and update result.
        await PerformCheckAsync.Invoke(this, inputs, result, cancellationToken);

        // If the check forgot to update the status, set it here
        if (result.CheckStatus == DetectorCheckStatus.InProgress)
        {
            result.CheckStatus = cancellationToken.IsCancellationRequested ? DetectorCheckStatus.Canceled : DetectorCheckStatus.CompletedFailed;
        }

        return result;
    }

    public override string ToString()
    {
        return $"({(IsRequired ? "Req" : "Opt")}) {Description}";
    }
}