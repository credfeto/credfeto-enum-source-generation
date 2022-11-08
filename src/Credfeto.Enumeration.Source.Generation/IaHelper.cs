using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Credfeto.Enumeration.Source.Generation;

public static class IaHelper
{
    public static ImmutableArray<DiagnosticAnalyzer> For(DiagnosticAnalyzer analyzer)
    {
        return ImmutableArray.Create(analyzer);
    }
}