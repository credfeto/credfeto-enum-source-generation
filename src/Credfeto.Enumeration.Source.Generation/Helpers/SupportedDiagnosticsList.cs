using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Helpers;

internal static class SupportedDiagnosticsList
{
    public static ImmutableArray<DiagnosticDescriptor> Build(DiagnosticDescriptor rule)
    {
        return ImmutableArray<DiagnosticDescriptor>.Empty.Add(item: rule);
    }
}