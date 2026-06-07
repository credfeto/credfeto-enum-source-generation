using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class ProhibitEnumToStringsDiagnosticsAnalyzerTests : TestBase
{
    private static async Task<IReadOnlyCollection<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);

        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            [new ProhibitEnumToStringsDiagnosticsAnalyzer()]
        );

        return await compilationWithAnalyzers.GetAllDiagnosticsAsync(System.Threading.CancellationToken.None);
    }

    [Fact]
    public void SupportedDiagnosticsContainsEnum001()
    {
        ProhibitEnumToStringsDiagnosticsAnalyzer analyzer = new();
        Assert.Contains(analyzer.SupportedDiagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task EnumToStringMethodRaisesEnum001()
    {
        const string source = """
            public enum Status { OPEN, CLOSED }
            public class C
            {
                public string M(Status s) => s.ToString();
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.Contains(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task StringToStringDoesNotRaiseEnum001()
    {
        const string source = """
            public class C
            {
                public string M(string s) => s.ToString();
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task EnumInInterpolatedStringRaisesEnum001()
    {
        const string source = """
            public enum Status { OPEN, CLOSED }
            public class C
            {
                public string M(Status s) => $"Status: {s}";
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.Contains(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task NonEnumInInterpolatedStringDoesNotRaiseEnum001()
    {
        const string source = """
            public class C
            {
                public string M(int x) => $"Value: {x}";
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task StringConcatWithEnumOnRightRaisesEnum001()
    {
        const string source = """
            public enum Status { OPEN, CLOSED }
            public class C
            {
                public string M(Status s) => "Status: " + s;
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.Contains(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task StringConcatWithEnumOnLeftRaisesEnum001()
    {
        const string source = """
            public enum Status { OPEN, CLOSED }
            public class C
            {
                public string M(Status s) => s + " suffix";
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.Contains(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task IntConcatWithStringDoesNotRaiseEnum001()
    {
        const string source = """
            public class C
            {
                public string M(int x) => "Value: " + x;
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task EnumStringConcatInAttributeArgumentDoesNotRaiseEnum001()
    {
        // String + non-enum inside attribute argument should not be flagged
        const string source = """
            using System;
            public class MyAttr : Attribute { public MyAttr(string s) { } }
            [MyAttr("hello" + " world")]
            public class C { }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task PointerMemberAccessToStringOnNonEnumDoesNotRaiseEnum001()
    {
        // Exercises the PointerMemberAccessExpression path in CheckMemberAccessExpression
        const string source = """
            public struct MyStruct { public int Value; }
            public class C
            {
                public unsafe string M(MyStruct* s) => s->ToString();
            }
            """;

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestPointer",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
            references: CompilationHelpers.CreateCompilation("// empty").References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );

        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            [new ProhibitEnumToStringsDiagnosticsAnalyzer()]
        );
        IReadOnlyCollection<Diagnostic> diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync(
            System.Threading.CancellationToken.None
        );

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task NonToStringMethodAccessOnEnumDoesNotRaiseEnum001()
    {
        // Accessing a non-ToString member on an enum should not be flagged
        const string source = """
            public enum Status { OPEN, CLOSED }
            public class C
            {
                public bool M(Status s) => s.HasFlag(Status.OPEN);
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }

    [Fact]
    public async Task InterpolatedStringWithNoEnumDoesNotRaiseEnum001()
    {
        const string source = """
            public class C
            {
                public string M(string name, int count) => $"Name: {name}, Count: {count}";
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "ENUM001"));
    }
}
