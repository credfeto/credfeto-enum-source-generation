using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Formatting;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class GenerationExtensionsTests : TestBase
{
    private static IFieldSymbol MakeField(string name, int value)
    {
        IFieldSymbol field = GetSubstitute<IFieldSymbol>();
        field.Name.Returns(name);
        field.ConstantValue.Returns(value);
        field.GetAttributes().Returns([]);
        return field;
    }

    private static EnumGeneration MakeEnum(
        string name,
        string ns,
        IReadOnlyList<IFieldSymbol> members,
        bool hasDoesNotReturn = true,
        bool supportsUnreachable = true
    )
    {
        GenerationOptions options = new(
            hasDoesNotReturnAttribute: hasDoesNotReturn,
            supportsUnreachableException: supportsUnreachable
        );

        return new(
            accessType: AccessType.PUBLIC,
            name: name,
            @namespace: ns,
            members: members,
            location: Location.None,
            options: options
        );
    }

    [Fact]
    public void UniqueEnumMemberNamesReturnsDistinctMemberNames()
    {
        IFieldSymbol memberA = MakeField("A", 0);
        IFieldSymbol memberB = MakeField("B", 1);
        IFieldSymbol memberA2 = MakeField("A", 0); // duplicate name

        EnumGeneration enumGen = MakeEnum("TestEnum", "Test", [memberA, memberB, memberA2]);

        HashSet<string> names = enumGen.UniqueEnumMemberNames();

        Assert.Equal(expected: 2, actual: names.Count);
        Assert.Contains("A", names);
        Assert.Contains("B", names);
    }

    [Fact]
    public void GenerateMethodsProducesOutputWithZeroMembers()
    {
        EnumGeneration enumGen = MakeEnum("EmptyEnum", "Test", []);
        ClassNameOnlyFormatter formatter = new(enumGen);
        CodeBuilder builder = new();

        builder.GenerateMethods(attribute: enumGen, formatConfig: formatter);

        string text = builder.Text.ToString();
        Assert.Contains("GetName", text, System.StringComparison.Ordinal);
        Assert.Contains("GetDescription", text, System.StringComparison.Ordinal);
        Assert.Contains("IsDefined", text, System.StringComparison.Ordinal);
        Assert.Contains("ThrowInvalidEnumMemberException", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateMethodsProducesOutputWithOneMember()
    {
        IFieldSymbol memberA = MakeField("VALUE_ONE", 0);
        EnumGeneration enumGen = MakeEnum("SingleEnum", "Test", [memberA]);
        ClassNameOnlyFormatter formatter = new(enumGen);
        CodeBuilder builder = new();

        builder.GenerateMethods(attribute: enumGen, formatConfig: formatter);

        string text = builder.Text.ToString();
        Assert.Contains("VALUE_ONE", text, System.StringComparison.Ordinal);
        // Single member IsDefined uses == comparison
        Assert.Contains("return value == ", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateMethodsProducesOutputWithMultipleMembers()
    {
        IFieldSymbol memberA = MakeField("FIRST", 0);
        IFieldSymbol memberB = MakeField("SECOND", 1);
        IFieldSymbol memberC = MakeField("THIRD", 2);
        EnumGeneration enumGen = MakeEnum("MultiEnum", "Test", [memberA, memberB, memberC]);
        ClassNameOnlyFormatter formatter = new(enumGen);
        CodeBuilder builder = new();

        builder.GenerateMethods(attribute: enumGen, formatConfig: formatter);

        string text = builder.Text.ToString();
        Assert.Contains("FIRST", text, System.StringComparison.Ordinal);
        Assert.Contains("SECOND", text, System.StringComparison.Ordinal);
        Assert.Contains("THIRD", text, System.StringComparison.Ordinal);
        // Multiple members IsDefined uses 'is X or Y' pattern
        Assert.Contains(" or ", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateMethodsWithDoesNotReturnAttributeAddsAnnotation()
    {
        EnumGeneration enumGen = MakeEnum("TestEnum", "Test", [], hasDoesNotReturn: true, supportsUnreachable: false);
        ClassNameOnlyFormatter formatter = new(enumGen);
        CodeBuilder builder = new();

        builder.GenerateMethods(attribute: enumGen, formatConfig: formatter);

        string text = builder.Text.ToString();
        Assert.Contains("[DoesNotReturn]", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateMethodsWithUnreachableExceptionUsesUnreachableException()
    {
        EnumGeneration enumGen = MakeEnum("TestEnum", "Test", [], hasDoesNotReturn: false, supportsUnreachable: true);
        ClassNameOnlyFormatter formatter = new(enumGen);
        CodeBuilder builder = new();

        builder.GenerateMethods(attribute: enumGen, formatConfig: formatter);

        string text = builder.Text.ToString();
        Assert.Contains("UnreachableException", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateMethodsWithoutUnreachableExceptionUsesConditionalCompilation()
    {
        EnumGeneration enumGen = MakeEnum("TestEnum", "Test", [], hasDoesNotReturn: false, supportsUnreachable: false);
        ClassNameOnlyFormatter formatter = new(enumGen);
        CodeBuilder builder = new();

        builder.GenerateMethods(attribute: enumGen, formatConfig: formatter);

        string text = builder.Text.ToString();
        Assert.Contains("#if NET7_0_OR_GREATER", text, System.StringComparison.Ordinal);
        Assert.Contains("#else", text, System.StringComparison.Ordinal);
        Assert.Contains("#endif", text, System.StringComparison.Ordinal);
    }

    [Fact]
    public void UniqueEnumMemberNamesReturnsEmptySetForEmptyMembers()
    {
        EnumGeneration enumGen = MakeEnum("EmptyEnum", "NS", []);
        HashSet<string> names = enumGen.UniqueEnumMemberNames();

        Assert.Empty(names);
    }
}
