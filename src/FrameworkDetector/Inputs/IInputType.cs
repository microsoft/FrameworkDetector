// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

using FrameworkDetector.DataSources;

namespace FrameworkDetector.Inputs;

/// <summary>
/// Base interface for all input types to be used by the <see cref="DetectionEngine"/>. Used to group inputs within a <see cref="ToolRunResult"/>.
/// Inputs store data and provide one or more <see cref="IDataSource"/>s which can then be cast out by <see cref="ICheckDefinition.PerformCheckAsync(IDetector, System.Collections.Generic.IEnumerable{IInputType}, System.Threading.CancellationToken)"/>.
/// </summary>
/// <remarks>
/// Input types should also implement <see cref="IInputTypeFactory{T}"/>.
/// </remarks>
public interface IInputType
{
    /// <summary>
    /// An identifier to group similar inputs within a <see cref="ToolRunResult"/>.
    /// </summary>
    public string InputGroup { get; }
}

/// <summary>
/// <see cref="IInputType{T}"/>s are <see cref="IInputType"/>s which wrap a base .NET <see cref="T"/> type. They contain all necessary (meta)data from the <see cref="T"/> type without holding a reference to the original type instance, and expose that (meta)data in the form of <see cref="IDataSource"/>s.
/// While it may be possible for an <see cref="T"/> type to reach other widely-used (non-primitive) .NET types, it's typically preferable scope <see cref="IInputType{T}"/>s to the exclusive data of <see cref="T"/>.
/// Then define a separate <see cref="IInputType{T}"/> type to wrap any child data of <see cref="T"/>. Let the calling parent application create the multiple <see cref="IInputType{T}"/>s as needed.
/// Therefore let each <see cref="IInputType{T}"/> produce only the <see cref="IDataSource"/>s than can be accessible solely from <see cref="T"/>.
/// </summary>
/// <remarks>
/// <see cref="IInputType{T}"/> types typically implement <see cref="IInputTypeFactory{T}"/> to facilitate their creation and <see cref="ICustomDataSource"/> so that plugin authors can extend them with custom data derived from the wrapped <see cref="T"/>.
/// </remarks>
public interface IInputType<T> : IInputType
{

}
