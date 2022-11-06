using System;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

/// <summary>
///     Location where the diagnostic appears, as determined by path, line number, and column number.
/// </summary>
[SuppressMessage(category: "Microsoft.Performance", checkId: "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Test code")]
public readonly struct DiagnosticResultLocation
{
    public DiagnosticResultLocation(string path, int line, int column)
    {
        if (line < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(line), message: "line must be >= -1");
        }

        if (column < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(column), message: "column must be >= -1");
        }

        this.Path = path;
        this.Line = line;
        this.Column = column;
    }

    public string Path { get; }

    public int Line { get; }

    public int Column { get; }
}