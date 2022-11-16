using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

public abstract class GeneratorVerifierTestsBase<TSourceGenerator> : TestBase
    where TSourceGenerator : ISourceGenerator, new()
{
    protected static Task VerifyAsync(string code, IReadOnlyList<(string filename, string generated)> expected)
    {
        CSharpSourceGeneratorVerifier<TSourceGenerator>.Test t = new() { TestState = { Sources = { code } } };

        foreach ((string filename, string generated) in expected)
        {
            (Type sourceGeneratorType, string filename, SourceText content) item = (sourceGeneratorType: typeof(TSourceGenerator), filename,
                content: SourceText.From(generated.ReplaceLineEndings(), encoding: Encoding.UTF8, checksumAlgorithm: SourceHashAlgorithm.Sha256));

            t.TestState.GeneratedSources.Add(item);
        }

        return t.RunAsync();
    }
}