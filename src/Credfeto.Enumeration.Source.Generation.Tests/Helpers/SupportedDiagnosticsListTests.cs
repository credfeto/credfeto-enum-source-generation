using System.Collections.Immutable;
using Credfeto.Enumeration.Source.Generation.Helpers;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Helpers;

public sealed class SupportedDiagnosticsListTests : TestBase
{
    [Fact]
    public void BuildReturnsSingleElementArray()
    {
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TST001",
            category: "Test",
            title: "Title",
            message: "Message"
        );

        ImmutableArray<DiagnosticDescriptor> result = SupportedDiagnosticsList.Build(rule);

        Assert.Single(result);
    }

    [Fact]
    public void BuildReturnsArrayContainingSuppliedRule()
    {
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TST001",
            category: "Test",
            title: "Title",
            message: "Message"
        );

        ImmutableArray<DiagnosticDescriptor> result = SupportedDiagnosticsList.Build(rule);

        Assert.Same(expected: rule, actual: result[0]);
    }
}
