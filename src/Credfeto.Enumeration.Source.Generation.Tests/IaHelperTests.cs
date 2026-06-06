using System.Collections.Immutable;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class IaHelperTests : TestBase
{
    [Fact]
    public void ForReturnsImmutableArrayContainingAnalyzer()
    {
        DiagnosticAnalyzer analyzer = new ProhibitEnumToStringsDiagnosticsAnalyzer();
        ImmutableArray<DiagnosticAnalyzer> result = IaHelper.For(analyzer);

        Assert.Single(result);
        Assert.Same(expected: analyzer, actual: result[0]);
    }
}
