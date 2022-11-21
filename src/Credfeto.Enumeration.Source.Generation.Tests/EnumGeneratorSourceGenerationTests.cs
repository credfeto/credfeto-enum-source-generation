using System;
using System.Threading.Tasks;
using Credfeto.Enumeration.Source.Generation.Helpers;
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
            (filename: "ConsoleApplication1.ExampleEnumGeneratedExtensions.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;

[GeneratedCode(tool: ""Credfeto.Enumeration.Source.Generation.EnumGenerator"", version: """ + VersionInformation.Version() + @""")]
public static class ExampleEnumGeneratedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this ExampleEnum value)
    {
        return value switch
        {
            ExampleEnum.HELLO => nameof(ExampleEnum.HELLO),
            ExampleEnum.WORLD => nameof(ExampleEnum.WORLD),
            _ => ThrowInvalidEnumMemberException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this ExampleEnum value)
    {
        return GetName(value);
    }

    public static string ThrowInvalidEnumMemberException(this ExampleEnum value)
    {
        #if NET7_0_OR_GREATER
        throw new UnreachableException(message: ""ExampleEnum: Unknown enum member"");
        #else
        throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: ""Unknown enum member"");
        #endif
    }
}
")
        };

        return VerifyAsync(code: test, expected: expected);
    }

    [Fact]
    public Task ExampleEnumGeneratesCodeWithAliasAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public enum ExampleEnum
        {
            HELLO,
            WORLD,
            EVERYONE = WORLD
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.ExampleEnumGeneratedExtensions.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;

[GeneratedCode(tool: ""Credfeto.Enumeration.Source.Generation.EnumGenerator"", version: """ + VersionInformation.Version() + @""")]
public static class ExampleEnumGeneratedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this ExampleEnum value)
    {
        return value switch
        {
            ExampleEnum.HELLO => nameof(ExampleEnum.HELLO),
            ExampleEnum.WORLD => nameof(ExampleEnum.WORLD),
            _ => ThrowInvalidEnumMemberException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this ExampleEnum value)
    {
        return GetName(value);
    }

    public static string ThrowInvalidEnumMemberException(this ExampleEnum value)
    {
        #if NET7_0_OR_GREATER
        throw new UnreachableException(message: ""ExampleEnum: Unknown enum member"");
        #else
        throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: ""Unknown enum member"");
        #endif
    }
}
")
        };

        return VerifyAsync(code: test, expected: expected);
    }

    [Fact]
    [Obsolete("This is a test of the old way of doing things, it should be removed when the old way is removed")]
    public Task ExampleEnumGeneratesCodeSkippingObsoleteAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public enum ExampleEnum
        {
            HELLO,
            WORLD,
            [Obsolete(""This is a test of the old way of doing things, it should be removed when the old way is removed"")]
            EVERYONE
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.ExampleEnumGeneratedExtensions.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;

[GeneratedCode(tool: ""Credfeto.Enumeration.Source.Generation.EnumGenerator"", version: """ + VersionInformation.Version() + @""")]
public static class ExampleEnumGeneratedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this ExampleEnum value)
    {
        return value switch
        {
            ExampleEnum.HELLO => nameof(ExampleEnum.HELLO),
            ExampleEnum.WORLD => nameof(ExampleEnum.WORLD),
            _ => ThrowInvalidEnumMemberException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this ExampleEnum value)
    {
        return GetName(value);
    }

    public static string ThrowInvalidEnumMemberException(this ExampleEnum value)
    {
        #if NET7_0_OR_GREATER
        throw new UnreachableException(message: ""ExampleEnum: Unknown enum member"");
        #else
        throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: ""Unknown enum member"");
        #endif
    }
}
")
        };

        return VerifyAsync(code: test, expected: expected);
    }
}