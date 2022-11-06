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
                                        Id = "FFS0001",
                                        Message = @"Call IDateTimeSource.UtcNow() rather than DateTime.Now",
                                        Severity = DiagnosticSeverity.Error,
                                        Locations = new[]
                                                    {
                                                        new DiagnosticResultLocation(path: "Test0.cs", line: 12, column: 25)
                                                    }
                                    };

        return this.VerifyCSharpDiagnosticAsync(source: test, expected);
    }
}