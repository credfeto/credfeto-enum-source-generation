using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Enumerable = System.Linq.Enumerable;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

public abstract partial class DiagnosticVerifier
{
    #region To be implemented by Test classes

    protected virtual DiagnosticAnalyzer? GetCSharpDiagnosticAnalyzer()
    {
        return null;
    }

    #endregion

    #region Formatting Diagnostics

    private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
    {
        StringBuilder builder = new();

        foreach (Diagnostic diagnostic in diagnostics)
        {
            builder = builder.Append("// ")
                             .Append(diagnostic)
                             .AppendLine();

            Type analyzerType = analyzer.GetType();
            IReadOnlyList<DiagnosticDescriptor> rules = analyzer.SupportedDiagnostics;

            DiagnosticDescriptor? rule = Enumerable.FirstOrDefault(source: rules, predicate: rule => StringComparer.Ordinal.Equals(x: rule.Id, y: diagnostic.Id));

            if (rule is null)
            {
                continue;
            }

            Location location = diagnostic.Location;

            if (location == Location.None)
            {
                builder = builder.Append("GetGlobalResult(")
                                 .Append(analyzerType.Name)
                                 .Append('.')
                                 .Append(rule.Id)
                                 .Append(')');
            }
            else
            {
                Assert.True(condition: location.IsInSource, $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostic}\r\n");

                string resultMethodName = GetResultMethodName(diagnostic);
                LinePosition linePosition = GetStartLinePosition(diagnostic);

                builder = builder.Append(resultMethodName)
                                 .Append('(')
                                 .Append(linePosition.Line + 1)
                                 .Append(", ")
                                 .Append(linePosition.Character + 1)
                                 .Append(", ")
                                 .Append(analyzerType.Name)
                                 .Append('.')
                                 .Append(rule.Id)
                                 .Append(')');
            }

            builder = builder.Append(value: ',')
                             .AppendLine();
        }

        return builder.ToString()
                      .TrimEnd()
                      .TrimEnd(',') + Environment.NewLine;
    }

    private static LinePosition GetStartLinePosition(Diagnostic diagnostic)
    {
        return diagnostic.Location.GetLineSpan()
                         .StartLinePosition;
    }

    private static string GetResultMethodName(Diagnostic diagnostic)
    {
        SyntaxTree? syntaxTree = diagnostic.Location.SourceTree;

        if (syntaxTree is null)
        {
            return MissingSourceTree();
        }

        return syntaxTree.FilePath.EndsWith(value: ".cs", comparisonType: StringComparison.Ordinal)
            ? "GetCSharpResultAt"
            : "GetBasicResultAt";
    }

    private static string MissingSourceTree()
    {
        throw new InvalidOperationException("Source tree is null");
    }

    #endregion

    #region Verifier wrappers

    protected Task VerifyCSharpDiagnosticAsync(string source, params DiagnosticResult[] expected)
    {
        return this.VerifyCSharpDiagnosticAsync(source: source, Array.Empty<MetadataReference>(), expected: expected);
    }

    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0109:Use span", Justification = "doesn't need to be")]
    protected Task VerifyCSharpDiagnosticAsync(string source, MetadataReference[] references, params DiagnosticResult[] expected)
    {
        DiagnosticAnalyzer? diagnostic = this.GetCSharpDiagnosticAnalyzer();
        Assert.NotNull(diagnostic);

        return VerifyDiagnosticsAsync([
                                          source
                                      ],
                                      references: references,
                                      language: LanguageNames.CSharp,
                                      analyzer: diagnostic,
                                      expected: expected);
    }

    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "May be needed in future")]
    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0109:Use span", Justification = "doesn't need to be")]
    protected Task VerifyCSharpDiagnosticAsync(string[] sources, params DiagnosticResult[] expected)
    {
        return this.VerifyCSharpDiagnosticAsync(sources: sources, Array.Empty<MetadataReference>(), expected: expected);
    }

    private Task VerifyCSharpDiagnosticAsync(string[] sources, MetadataReference[] references, params DiagnosticResult[] expected)
    {
        DiagnosticAnalyzer? diagnostic = this.GetCSharpDiagnosticAnalyzer();
        Assert.NotNull(diagnostic);

        return VerifyDiagnosticsAsync(sources: sources, references: references, language: LanguageNames.CSharp, analyzer: diagnostic, expected: expected);
    }

    private static async Task VerifyDiagnosticsAsync(string[] sources, MetadataReference[] references, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
    {
        IReadOnlyList<Diagnostic> diagnostics = await GetSortedDiagnosticsAsync(sources: sources, references: references, language: language, analyzer: analyzer);

        VerifyDiagnosticResults(actualResults: diagnostics, analyzer: analyzer, expectedResults: expected);
    }

    #endregion

    #region Actual comparisons and verifications

    private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
    {
        Diagnostic[] results = Enumerable.ToArray(actualResults);
        int expectedCount = expectedResults.Length;
        int actualCount = results.Length;

        if (expectedCount != actualCount)
        {
            string diagnosticsOutput = results.Length != 0
                ? FormatDiagnostics(analyzer: analyzer, Enumerable.ToArray(results))
                : "    NONE.";

            Assert.Fail($"Mismatch between number of diagnostics returned, expected \"{expectedCount}\" actual \"{actualCount}\"\r\n\r\nDiagnostics:\r\n{diagnosticsOutput}\r\n");
        }

        for (int i = 0; i < expectedResults.Length; i++)
        {
            Diagnostic actual = results[i];
            DiagnosticResult expected = expectedResults[i];

            VerifyOneResult(analyzer: analyzer, expected: expected, actual: actual);
        }
    }

    private static void VerifyOneResult(DiagnosticAnalyzer analyzer, in DiagnosticResult expected, Diagnostic actual)
    {
        if (expected.Line == -1 && expected.Column == -1)
        {
            Assert.True(actual.Location == Location.None, $"Expected:\nA project diagnostic with No location\nActual:\n{FormatDiagnostics(analyzer: analyzer, actual)}");
        }
        else
        {
            VerifyDiagnosticLocation(analyzer: analyzer, diagnostic: actual, actual: actual.Location, expected.Locations[0]);
            VerifyAdditionalDiagnosticLocations(analyzer: analyzer, actual: actual, expected: expected);
        }

        Assert.True(StringComparer.Ordinal.Equals(x: actual.Id, y: expected.Id),
                    $"Expected diagnostic id to be \"{expected.Id}\" was \"{actual.Id}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, actual)}\r\n");

        Assert.True(actual.Severity == expected.Severity,
                    $"Expected diagnostic severity to be \"{expected.Severity}\" was \"{actual.Severity}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, actual)}\r\n");

        Assert.True(StringComparer.Ordinal.Equals(actual.GetMessage(), y: expected.Message),
                    $"Expected diagnostic message to be \"{expected.Message}\" was \"{actual.GetMessage()}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, actual)}\r\n");
    }

    private static void VerifyAdditionalDiagnosticLocations(DiagnosticAnalyzer analyzer, Diagnostic actual, in DiagnosticResult expected)
    {
        Location[] additionalLocations = Enumerable.ToArray(actual.AdditionalLocations);

        if (additionalLocations.Length != expected.Locations.Count - 1)
        {
            Assert.Fail(
                $"Expected {expected.Locations.Count - 1} additional locations but got {additionalLocations.Length} for Diagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, actual)}\r\n");
        }

        for (int j = 0; j < additionalLocations.Length; ++j)
        {
            VerifyDiagnosticLocation(analyzer: analyzer, diagnostic: actual, additionalLocations[j], expected.Locations[j + 1]);
        }
    }

    private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, in DiagnosticResultLocation expected)
    {
        FileLinePositionSpan actualSpan = actual.GetLineSpan();

        Assert.True(StringComparer.Ordinal.Equals(x: actualSpan.Path, y: expected.Path) || actualSpan.Path.Contains(value: "Test0.") && expected.Path.Contains(value: "Test."),
                    $"Expected diagnostic to be in file \"{expected.Path}\" was actually in file \"{actualSpan.Path}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, diagnostic)}\r\n");

        LinePosition actualLinePosition = actualSpan.StartLinePosition;

        // Only check line position if there is an actual line in the real diagnostic
        if (actualLinePosition.Line > 0)
        {
            if (actualLinePosition.Line + 1 != expected.Line)
            {
                Assert.Fail(
                    $"Expected diagnostic to be on line \"{expected.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, diagnostic)}\r\n");
            }
        }

        // Only check column position if there is an actual column position in the real diagnostic
        if (actualLinePosition.Character > 0)
        {
            if (actualLinePosition.Character + 1 != expected.Column)
            {
                Assert.Fail(
                    $"Expected diagnostic to start at column \"{expected.Column}\" was actually at column \"{actualLinePosition.Character + 1}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer: analyzer, diagnostic)}\r\n");
            }
        }
    }

    #endregion
}