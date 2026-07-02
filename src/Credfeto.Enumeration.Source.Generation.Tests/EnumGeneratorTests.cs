using System.Collections.Immutable;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Models;
using Credfeto.Enumeration.Source.Generation.Receivers;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class EnumGeneratorTests : TestBase
{
    private static CSharpCompilation CreateCompilationForGeneration(string source)
    {
        return CompilationHelpers.CreateCompilationWithEnumTextAttribute(source);
    }

    [Fact]
    public void GeneratorSkipsStaticPartialClassWithNoEnumTextAttribute()
    {
        const string source = """
            namespace TestNs
            {
                public static partial class NoAttributeExtensions { }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.DoesNotContain("NoAttributeExtensions", allGenerated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratorProducesNoOutputForEmptySource()
    {
        CSharpCompilation compilation = CreateCompilationForGeneration("// empty");
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver.RunGeneratorsAndUpdateCompilation(
            compilation: compilation,
            outputCompilation: out Compilation outputCompilation,
            diagnostics: out ImmutableArray<Diagnostic> diagnostics,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.NotNull(outputCompilation);
    }

    [Fact]
    public void GeneratorProducesOutputForSimplePublicEnum()
    {
        const string source = """
            using Credfeto.Enumeration.Source.Generation.Attributes;
            namespace TestNs
            {
                public enum Colors { RED, GREEN, BLUE }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.NotEmpty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratorProducesSourceContainingGetNameForPublicEnum()
    {
        const string source = """
            namespace TestNs
            {
                public enum Status { OPEN, CLOSED }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.Contains("GetName", allGenerated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratorSkipsPrivateEnum()
    {
        const string source = """
            namespace TestNs
            {
                public class Outer
                {
                    private enum Hidden { A, B }
                }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.DoesNotContain("Hidden", allGenerated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratorSkipsObsoleteEnum()
    {
        const string source = """
            using System;
            namespace TestNs
            {
                [Obsolete]
                public enum OldEnum { A, B }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.DoesNotContain("OldEnumGeneratedExtensions", allGenerated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratorHandlesEnumWithAliasedMembers()
    {
        const string source = """
            namespace TestNs
            {
                public enum Direction { NORTH, SOUTH, UP = NORTH, DOWN = SOUTH }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.NotEmpty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratorHandlesClassWithEnumTextAttribute()
    {
        const string source = """
            using Credfeto.Enumeration.Source.Generation.Attributes;
            namespace TestNs
            {
                public enum MyColors { RED, GREEN }

                [EnumText(typeof(MyColors))]
                public static partial class MyColorsExtensions { }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.NotEmpty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratorSkipsNonStaticClass()
    {
        const string source = """
            using Credfeto.Enumeration.Source.Generation.Attributes;
            namespace TestNs
            {
                public enum Colors { RED }

                [EnumText(typeof(Colors))]
                public partial class NotStaticExtensions { }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.DoesNotContain("NotStaticExtensions", allGenerated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratorSkipsNonPartialClass()
    {
        const string source = """
            using Credfeto.Enumeration.Source.Generation.Attributes;
            namespace TestNs
            {
                public enum Colors { RED }

                [EnumText(typeof(Colors))]
                public static class NotPartialExtensions { }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.DoesNotContain("NotPartialExtensions", allGenerated, System.StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    public void DetectGenerationOptionsReturnsCorrectFlag(
        bool useMinimalCompilation,
        bool checkUnreachable,
        bool expected
    )
    {
        CSharpCompilation compilation = useMinimalCompilation
            ? CompilationHelpers.CreateMinimalCompilation("// empty")
            : CompilationHelpers.CreateCompilation("// empty");
        GenerationOptions options = SyntaxExtractor.DetectGenerationOptions(compilation);

        bool actual = checkUnreachable ? options.SupportsUnreachableException : options.HasDoesNotReturnAttribute;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("[DoesNotReturn]", null)]
    [InlineData("throw new UnreachableException", "#if NET7_0_OR_GREATER")]
    public void GeneratorEmitsExpectedToken(string mustContain, string? mustNotContain)
    {
        const string source = """
            namespace TestNs
            {
                public enum Status { OPEN, CLOSED }
            }
            """;

        CSharpCompilation compilation = CreateCompilationForGeneration(source);
        EnumGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        GeneratorDriverRunResult result = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out ImmutableArray<Diagnostic> diagnostics,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetRunResult();

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        string allGenerated = string.Join('\n', result.GeneratedTrees.Select(t => t.ToString()));
        Assert.Contains(mustContain, allGenerated, System.StringComparison.Ordinal);

        if (mustNotContain is not null)
        {
            Assert.DoesNotContain(mustNotContain, allGenerated, System.StringComparison.Ordinal);
        }
    }
}
