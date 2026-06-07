using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class EnumSourceGeneratorTests : TestBase
{
    private static IFieldSymbol MakeField(string name, int value)
    {
        IFieldSymbol field = GetSubstitute<IFieldSymbol>();
        field.Name.Returns(name);
        field.ConstantValue.Returns(value);
        field.GetAttributes().Returns([]);
        return field;
    }

    [Fact]
    public void GenerateClassForEnumReturnsExpectedClassName()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: true);
        IFieldSymbol fieldA = MakeField("VALUE_A", 0);
        IFieldSymbol fieldB = MakeField("VALUE_B", 1);

        EnumGeneration enumDeclaration = new(
            accessType: AccessType.PUBLIC,
            name: "TestEnum",
            @namespace: "My.Namespace",
            members: [fieldA, fieldB],
            location: Location.None,
            options: options
        );

        string className = EnumSourceGenerator.GenerateClassForEnum(
            enumDeclaration: enumDeclaration,
            source: out CodeBuilder source
        );

        Assert.Equal(expected: "TestEnumGeneratedExtensions", actual: className);
        Assert.NotNull(source);
    }

    [Fact]
    public void GenerateClassForEnumProducesSourceContainingEnumName()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: true);
        IFieldSymbol fieldA = MakeField("ALPHA", 0);

        EnumGeneration enumDeclaration = new(
            accessType: AccessType.INTERNAL,
            name: "MyEnum",
            @namespace: "Some.NS",
            members: [fieldA],
            location: Location.None,
            options: options
        );

        EnumSourceGenerator.GenerateClassForEnum(enumDeclaration: enumDeclaration, source: out CodeBuilder source);
        string text = source.Text.ToString();

        Assert.Contains("MyEnum", text, System.StringComparison.Ordinal);
        Assert.Contains("Some.NS", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateClassForEnumWithNoMembersStillProducesValidSource()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        EnumGeneration enumDeclaration = new(
            accessType: AccessType.PUBLIC,
            name: "EmptyEnum",
            @namespace: "Empty.NS",
            members: [],
            location: Location.None,
            options: options
        );

        string className = EnumSourceGenerator.GenerateClassForEnum(
            enumDeclaration: enumDeclaration,
            source: out CodeBuilder source
        );

        Assert.Equal(expected: "EmptyEnumGeneratedExtensions", actual: className);
        string text = source.Text.ToString();
        Assert.Contains("EmptyEnum", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateClassForClassReturnsExpectedClassName()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: true);
        IFieldSymbol fieldA = MakeField("ITEM", 0);

        EnumGeneration enumGen = new(
            accessType: AccessType.PUBLIC,
            name: "Colors",
            @namespace: "App",
            members: [fieldA],
            location: Location.None,
            options: options
        );

        ClassEnumGeneration classDeclaration = new(
            accessType: AccessType.PUBLIC,
            name: "ColorExtensions",
            @namespace: "App",
            enums: [enumGen],
            location: Location.None
        );

        string className = EnumSourceGenerator.GenerateClassForClass(
            classDeclaration: classDeclaration,
            source: out CodeBuilder source
        );

        Assert.Equal(expected: "ColorExtensions", actual: className);
        Assert.NotNull(source);
    }

    [Fact]
    public void GenerateClassForClassProducesSourceContainingClassNameAndNamespace()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: true);
        IFieldSymbol fieldA = MakeField("VALUE", 0);

        EnumGeneration enumGen = new(
            accessType: AccessType.PUBLIC,
            name: "Status",
            @namespace: "Domain",
            members: [fieldA],
            location: Location.None,
            options: options
        );

        ClassEnumGeneration classDeclaration = new(
            accessType: AccessType.INTERNAL,
            name: "StatusExtensions",
            @namespace: "Domain",
            enums: [enumGen],
            location: Location.None
        );

        EnumSourceGenerator.GenerateClassForClass(classDeclaration: classDeclaration, source: out CodeBuilder source);
        string text = source.Text.ToString();

        Assert.Contains("StatusExtensions", text, System.StringComparison.Ordinal);
        Assert.Contains("Domain", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateClassForClassWithMultipleEnumsProducesAllMethods()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: true);
        IFieldSymbol fieldA = MakeField("RED", 0);
        IFieldSymbol fieldB = MakeField("OPEN", 0);

        EnumGeneration colorsEnum = new(
            accessType: AccessType.PUBLIC,
            name: "Colors",
            @namespace: "App",
            members: [fieldA],
            location: Location.None,
            options: options
        );

        EnumGeneration statusEnum = new(
            accessType: AccessType.PUBLIC,
            name: "Status",
            @namespace: "App",
            members: [fieldB],
            location: Location.None,
            options: options
        );

        ClassEnumGeneration classDeclaration = new(
            accessType: AccessType.PUBLIC,
            name: "Extensions",
            @namespace: "App",
            enums: [colorsEnum, statusEnum],
            location: Location.None
        );

        EnumSourceGenerator.GenerateClassForClass(classDeclaration: classDeclaration, source: out CodeBuilder source);
        string text = source.Text.ToString();

        Assert.Contains("Colors", text, System.StringComparison.Ordinal);
        Assert.Contains("Status", text, System.StringComparison.Ordinal);
        Assert.Contains("RED", text, System.StringComparison.Ordinal);
        Assert.Contains("OPEN", text, System.StringComparison.Ordinal);
    }
}
