using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

[SuppressMessage(category: "Microsoft.Performance", checkId: "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Test code")]
public sealed class DiagnosticResult
{
    public DiagnosticResultLocation[] Locations { get; set; } = null!;

    public DiagnosticSeverity Severity { get; set; }

    public string Id { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int Line =>
        this.Locations.Length > 0
            ? this.Locations[0]
                  .Line
            : -1;

    public int Column =>
        this.Locations.Length > 0
            ? this.Locations[0]
                  .Column
            : -1;
}