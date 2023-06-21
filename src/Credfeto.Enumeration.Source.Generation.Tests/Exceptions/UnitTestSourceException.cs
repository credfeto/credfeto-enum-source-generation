using System;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Tests.Exceptions;

public sealed class UnitTestSourceException : Exception
{
    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "Standard exception constructor")]
    public UnitTestSourceException()
        : this(message: "House not ready")
    {
    }

    public UnitTestSourceException(string message)
        : base(message)
    {
    }

    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "Standard exception constructor")]
    public UnitTestSourceException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}