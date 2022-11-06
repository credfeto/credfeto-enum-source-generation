using System.Threading.Tasks;
using Credfeto.Enumeration.Source.Generation.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class ProhibitEnumToStringsDiagnosticsAnalyzerTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new ProhibitEnumToStringsDiagnosticsAnalyzer();
    }

    [Fact]
    public Task EnumToStringIsBannedInConstructorsAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public enum ExampleEnum
        {
            HELLO
        }

        public class TypeName
        {
            private readonly string _value;

            public TypeName(ExampleEnum value)
            {
                _value = value.ToString();
            }
        }
    }";
        DiagnosticResult expected = new()
                                    {
                                        Id = "ENUM001",
                                        Message = @"Do not use ToString() on an enum use EnumHelpers.GetName(this Enum value) instead",
                                        Severity = DiagnosticSeverity.Error,
                                        Locations = new[]
                                                    {
                                                        new DiagnosticResultLocation(path: "Test0.cs", line: 15, column: 26)
                                                    }
                                    };

        return this.VerifyCSharpDiagnosticAsync(source: test, expected);
    }

    [Fact]
    public Task EnumToStringIsBannedInMethodsAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public enum ExampleEnum
        {
            HELLO
        }

        public static class TypeName
        {
            public static string Format(ExampleEnum value)
            {
                return value.ToString();
            }
        }
    }";
        DiagnosticResult expected = new()
                                    {
                                        Id = "ENUM001",
                                        Message = @"Do not use ToString() on an enum use EnumHelpers.GetName(this Enum value) instead",
                                        Severity = DiagnosticSeverity.Error,
                                        Locations = new[]
                                                    {
                                                        new DiagnosticResultLocation(path: "Test0.cs", line: 13, column: 24)
                                                    }
                                    };

        return this.VerifyCSharpDiagnosticAsync(source: test, expected);
    }

    [Fact]
    public Task EnumToStringIsBannedInPropertyAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public enum ExampleEnum
        {
            HELLO
        }

        public class TypeName
        {
            public ExampleEnum Value {get;set;}

            public string Formatted => this.Value.ToString();

        }
    }";
        DiagnosticResult expected = new()
                                    {
                                        Id = "ENUM001",
                                        Message = @"Do not use ToString() on an enum use EnumHelpers.GetName(this Enum value) instead",
                                        Severity = DiagnosticSeverity.Error,
                                        Locations = new[]
                                                    {
                                                        new DiagnosticResultLocation(path: "Test0.cs", line: 13, column: 40)
                                                    }
                                    };

        return this.VerifyCSharpDiagnosticAsync(source: test, expected);
    }
}