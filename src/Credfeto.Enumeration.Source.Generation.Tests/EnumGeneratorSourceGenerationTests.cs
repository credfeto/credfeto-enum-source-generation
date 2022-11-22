using System;
using System.Diagnostics.CodeAnalysis;
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
            (filename: "ConsoleApplication1.ExampleEnumGeneratedExtensions.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
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
            (filename: "ConsoleApplication1.ExampleEnumGeneratedExtensions.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
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
    public Task ExampleEnumGeneratesCodeSkippingObsoleteAsync()
    {
        const string test = @"
    using System;

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
            (filename: "ConsoleApplication1.ExampleEnumGeneratedExtensions.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
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
    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
    public Task ExternalSingleEnumAsync()
    {
        const string test = @"
    using System;
    using System.Runtime;

    namespace Credfeto.Enumeration.Source.Generation.Attributes
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
        public sealed class EnumTextAttribute : Attribute
        {
            public EnumTextAttribute(Type enumType)
            {
                this.Enum = enumType;
            }

            public Type Enum { get; }
        }
    }

    namespace ConsoleApplication1
    {
        [Credfeto.Enumeration.Source.Generation.Attributes.EnumText(typeof(System.DateTimeKind))]
        public static partial class EnumExtensions
        {
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.EnumExtensions.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;

[GeneratedCode(tool: ""Credfeto.Enumeration.Source.Generation.EnumGenerator"", version: """ + VersionInformation.Version() + @""")]
public static partial class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this System.DateTimeKind value)
    {
        return value switch
        {
            System.DateTimeKind.Unspecified => nameof(System.DateTimeKind.Unspecified),
            System.DateTimeKind.Utc => nameof(System.DateTimeKind.Utc),
            System.DateTimeKind.Local => nameof(System.DateTimeKind.Local),
            _ => ThrowInvalidEnumMemberException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this System.DateTimeKind value)
    {
        return GetName(value);
    }

    public static string ThrowInvalidEnumMemberException(this System.DateTimeKind value)
    {
        #if NET7_0_OR_GREATER
        throw new UnreachableException(message: ""DateTimeKind: Unknown enum member"");
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
    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
    public Task ExternalMultipleSingleEnumAsync()
    {
        const string test = @"
    using System;
    using System.Runtime;

    namespace Credfeto.Enumeration.Source.Generation.Attributes
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
        public sealed class EnumTextAttribute : Attribute
        {
            public EnumTextAttribute(Type enumType)
            {
                this.Enum = enumType;
            }

            public Type Enum { get; }
        }
    }

    namespace ConsoleApplication1
    {
        [Credfeto.Enumeration.Source.Generation.Attributes.EnumText(typeof(DateTimeKind))]
        [Credfeto.Enumeration.Source.Generation.Attributes.EnumText(typeof(StringComparison))]
        public static partial class EnumExtensions
        {
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.EnumExtensions.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;

[GeneratedCode(tool: ""Credfeto.Enumeration.Source.Generation.EnumGenerator"", version: """ + VersionInformation.Version() + @""")]
public static partial class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this System.DateTimeKind value)
    {
        return value switch
        {
            System.DateTimeKind.Unspecified => nameof(System.DateTimeKind.Unspecified),
            System.DateTimeKind.Utc => nameof(System.DateTimeKind.Utc),
            System.DateTimeKind.Local => nameof(System.DateTimeKind.Local),
            _ => ThrowInvalidEnumMemberException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this System.DateTimeKind value)
    {
        return GetName(value);
    }

    public static string ThrowInvalidEnumMemberException(this System.DateTimeKind value)
    {
        #if NET7_0_OR_GREATER
        throw new UnreachableException(message: ""DateTimeKind: Unknown enum member"");
        #else
        throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: ""Unknown enum member"");
        #endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this System.StringComparison value)
    {
        return value switch
        {
            System.StringComparison.CurrentCulture => nameof(System.StringComparison.CurrentCulture),
            System.StringComparison.CurrentCultureIgnoreCase => nameof(System.StringComparison.CurrentCultureIgnoreCase),
            System.StringComparison.InvariantCulture => nameof(System.StringComparison.InvariantCulture),
            System.StringComparison.InvariantCultureIgnoreCase => nameof(System.StringComparison.InvariantCultureIgnoreCase),
            System.StringComparison.Ordinal => nameof(System.StringComparison.Ordinal),
            System.StringComparison.OrdinalIgnoreCase => nameof(System.StringComparison.OrdinalIgnoreCase),
            _ => ThrowInvalidEnumMemberException(value: value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this System.StringComparison value)
    {
        return GetName(value);
    }

    public static string ThrowInvalidEnumMemberException(this System.StringComparison value)
    {
        #if NET7_0_OR_GREATER
        throw new UnreachableException(message: ""StringComparison: Unknown enum member"");
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