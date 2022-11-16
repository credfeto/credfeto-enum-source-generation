using System;
using System.Threading.Tasks;
using Credfeto.Enumeration.Source.Generation.Tests.Verifiers;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class EnumGeneratorSourceGenerationTests : GeneratorVerifierTestsBase<EnumGenerator>
{
    [Fact]
    public Task NoEnumsGeneratesNothingAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public static class EmptyClass
        {
        }
    }";

        return VerifyAsync(code: test, Array.Empty<(string filename, string generated)>());
    }

    [Fact]
    public Task ExampleEnumGeneratesCodeAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public enum ExampleEnum
        {
            HELLO,
            WORLD
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ExampleEnumGenerated", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;

[GeneratedCode(tool: ""Credfeto.Enumeration.Source.Generation.EnumGenerator"", version: ""1.0.0"")]
public static class ExampleEnumGeneratedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this ExampleEnum value)
    {
        return value switch
        {
            // ExampleEnum.HELLO => nameof(ExampleEnum.HELLO)
            ExampleEnum.HELLO => nameof(ExampleEnum.HELLO),
            // ExampleEnum.WORLD => nameof(ExampleEnum.WORLD)
            ExampleEnum.WORLD => nameof(ExampleEnum.WORLD),
            _ => ThrowArgumentOutOfRangeException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this ExampleEnum value)
    {
        return GetName(value);
    }

    public static string ThrowArgumentOutOfRangeException(this ExampleEnum value)
    {
        throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: ""Unknown enum member"");
    }
}

")
        };

        return VerifyAsync(code: test, expected: expected);
    }
}