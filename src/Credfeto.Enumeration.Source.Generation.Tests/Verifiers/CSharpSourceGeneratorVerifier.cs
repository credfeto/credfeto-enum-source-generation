using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : ISourceGenerator, new()
{
    internal sealed class Test : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
    {
        private LanguageVersion LanguageVersion { get; } = LanguageVersion.Default;

        protected override CompilationOptions CreateCompilationOptions()
        {
            CompilationOptions compilationOptions = base.CreateCompilationOptions();

            string[] args =
            {
                "/warnaserror:nullable"
            };
            CSharpCommandLineArguments commandLineArguments =
                CSharpCommandLineParser.Default.Parse(args: args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);

            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(commandLineArguments.CompilationOptions.SpecificDiagnosticOptions));
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(this.LanguageVersion);
        }
    }
}