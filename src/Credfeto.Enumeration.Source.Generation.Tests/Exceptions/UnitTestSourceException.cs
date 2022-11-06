using System;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Tests.Exceptions;

/// <summary>
///     Raised when the verified code is invalid.
/// </summary>
public sealed class UnitTestSourceException : Exception
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "Standard exception constructor")]
    public UnitTestSourceException()
        : this(message: "House not ready")
    {
    }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="message">The message to return.</param>
    public UnitTestSourceException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="message">The message to return.</param>
    /// <param name="innerException">The inner exception.</param>
    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "Standard exception constructor")]
    public UnitTestSourceException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}