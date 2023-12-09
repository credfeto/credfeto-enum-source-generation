using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

[SuppressMessage(category: "Microsoft.Performance", checkId: "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Test code")]
[DebuggerDisplay("{Id}: {Message} ({Line}, {Column})")]
public sealed record DiagnosticResult(IReadOnlyList<DiagnosticResultLocation> Locations, DiagnosticSeverity Severity, string Id, string Message)
{
    public int Line =>
        this.Locations.Count > 0
            ? this.Locations[0].Line
            : -1;

    public int Column =>
        this.Locations.Count > 0
            ? this.Locations[0].Column
            : -1;
}