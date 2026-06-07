using System.Linq;
using System.Threading.Tasks;
using Credfeto.Enumeration.Source.Generation.Extensions;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class TypeInfoExtensionsTests : TestBase
{
    private static async Task<(TypeInfo stringType, TypeInfo enumType, TypeInfo intType)> GetTypeInfosAsync()
    {
        const string source = """
            public enum TestEnum { A }
            public class Container
            {
                public string StringField = string.Empty;
                public TestEnum EnumField = TestEnum.A;
                public int IntField = 0;
            }
            """;

        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);
        SyntaxTree tree = compilation.SyntaxTrees[0];
        SemanticModel model = compilation.GetSemanticModel(tree);

        FieldDeclarationSyntax[] fields =
        [
            .. (await tree.GetRootAsync(TestContext.Current.CancellationToken))
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>(),
        ];

        TypeInfo stringTypeInfo = model.GetTypeInfo(fields[0].Declaration.Type, TestContext.Current.CancellationToken);
        TypeInfo enumTypeInfo = model.GetTypeInfo(fields[1].Declaration.Type, TestContext.Current.CancellationToken);
        TypeInfo intTypeInfo = model.GetTypeInfo(fields[2].Declaration.Type, TestContext.Current.CancellationToken);

        return (stringTypeInfo, enumTypeInfo, intTypeInfo);
    }

    [Fact]
    public async Task IsStringReturnsTrueForStringType()
    {
        (TypeInfo stringType, _, _) = await GetTypeInfosAsync();
        Assert.True(stringType.IsString(), "TypeInfo for string should be identified as string");
    }

    [Fact]
    public async Task IsStringReturnsFalseForEnumType()
    {
        (_, TypeInfo enumType, _) = await GetTypeInfosAsync();
        Assert.False(enumType.IsString(), "TypeInfo for enum should not be identified as string");
    }

    [Fact]
    public async Task IsStringReturnsFalseForIntType()
    {
        (_, _, TypeInfo intType) = await GetTypeInfosAsync();
        Assert.False(intType.IsString(), "TypeInfo for int should not be identified as string");
    }

    [Fact]
    public async Task IsEnumReturnsTrueForEnumType()
    {
        (_, TypeInfo enumType, _) = await GetTypeInfosAsync();
        Assert.True(enumType.IsEnum(), "TypeInfo for enum should be identified as enum");
    }

    [Fact]
    public async Task IsEnumReturnsFalseForStringType()
    {
        (TypeInfo stringType, _, _) = await GetTypeInfosAsync();
        Assert.False(stringType.IsEnum(), "TypeInfo for string should not be identified as enum");
    }

    [Fact]
    public async Task IsEnumReturnsFalseForIntType()
    {
        (_, _, TypeInfo intType) = await GetTypeInfosAsync();
        Assert.False(intType.IsEnum(), "TypeInfo for int should not be identified as enum");
    }

    [Fact]
    public void IsStringReturnsFalseForDefaultTypeInfo()
    {
        TypeInfo empty = default;
        Assert.False(empty.IsString(), "Default TypeInfo should not be identified as string");
    }

    [Fact]
    public void IsEnumReturnsFalseForDefaultTypeInfo()
    {
        TypeInfo empty = default;
        Assert.False(empty.IsEnum(), "Default TypeInfo should not be identified as enum");
    }
}
