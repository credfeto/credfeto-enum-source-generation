using System;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Extensions;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class SymbolExtensionsTests : TestBase
{
    private static INamedTypeSymbol GetSymbol(string source, string typeName)
    {
        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);
        return compilation.GetTypeByMetadataName(typeName)
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in compilation");
    }

    private static IFieldSymbol GetEnumMemberSymbol(string source, string enumName, string memberName)
    {
        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName(enumName);
        return (type ?? throw new InvalidOperationException($"Type '{enumName}' not found in compilation"))
            .GetMembers(memberName)
            .OfType<IFieldSymbol>()
            .First();
    }

    [Fact]
    public void HasObsoleteAttributeReturnsFalseWhenNoObsoleteAttribute()
    {
        const string source = "public enum MyEnum { A, B }";
        INamedTypeSymbol symbol = GetSymbol(source, "MyEnum");
        Assert.False(symbol.HasObsoleteAttribute(), "Symbol should not have Obsolete attribute");
    }

    [Fact]
    public void HasObsoleteAttributeReturnsTrueWhenObsoleteAttributePresent()
    {
        const string source = """
            using System;
            [Obsolete]
            public enum MyEnum { A, B }
            """;
        INamedTypeSymbol symbol = GetSymbol(source, "MyEnum");
        Assert.True(symbol.HasObsoleteAttribute(), "Symbol should have Obsolete attribute");
    }

    [Fact]
    public void IsObsoleteAttributeReturnsTrueForObsoleteAttributeData()
    {
        const string source = """
            using System;
            [Obsolete]
            public enum MyEnum { A }
            """;
        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName("MyEnum");
        AttributeData attr = (type ?? throw new InvalidOperationException("Type not found")).GetAttributes().First();

        Assert.True(attr.IsObsoleteAttribute(), "AttributeData for ObsoleteAttribute should be identified as obsolete");
    }

    [Fact]
    public void IsObsoleteAttributeReturnsFalseForOtherAttribute()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            [Description("desc")]
            public enum MyEnum { A }
            """;
        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName("MyEnum");
        AttributeData attr = (type ?? throw new InvalidOperationException("Type not found")).GetAttributes().First();

        Assert.False(
            attr.IsObsoleteAttribute(),
            "AttributeData for DescriptionAttribute should not be identified as obsolete"
        );
    }

    [Fact]
    public void IsDescriptionAttributeReturnsTrueForDescriptionAttributeData()
    {
        const string source = """
            using System.ComponentModel;
            public enum MyEnum { [Description("a label")] A }
            """;
        IFieldSymbol field = GetEnumMemberSymbol(source, "MyEnum", "A");
        AttributeData attr = field.GetAttributes().First();

        Assert.True(
            attr.IsDescriptionAttribute(),
            "AttributeData for DescriptionAttribute should be identified as description"
        );
    }

    [Fact]
    public void IsDescriptionAttributeReturnsFalseForOtherAttribute()
    {
        const string source = """
            using System;
            public enum MyEnum { [Obsolete] A }
            """;
        IFieldSymbol field = GetEnumMemberSymbol(source, "MyEnum", "A");
        AttributeData attr = field.GetAttributes().First();

        Assert.False(
            attr.IsDescriptionAttribute(),
            "AttributeData for ObsoleteAttribute should not be identified as description"
        );
    }

    [Fact]
    public void IsObsoleteAttributeOnFieldReturnsTrueWhenObsolete()
    {
        const string source = """
            using System;
            public enum MyEnum { [Obsolete] A }
            """;
        IFieldSymbol field = GetEnumMemberSymbol(source, "MyEnum", "A");

        Assert.True(field.HasObsoleteAttribute(), "Field with Obsolete attribute should be identified as obsolete");
    }

    [Fact]
    public void IsObsoleteAttributeOnFieldReturnsFalseWhenNotObsolete()
    {
        const string source = "public enum MyEnum { A }";
        IFieldSymbol field = GetEnumMemberSymbol(source, "MyEnum", "A");

        Assert.False(
            field.HasObsoleteAttribute(),
            "Field without Obsolete attribute should not be identified as obsolete"
        );
    }
}
