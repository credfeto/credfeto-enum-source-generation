using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{Location} {Exception}")]
public readonly record struct ErrorInfo
{
    public ErrorInfo(Location location, Exception exception)
    {
        this.Location = location;
        this.Exception = exception;
    }

    public Location Location { get; }

    public Exception Exception { get; }
}