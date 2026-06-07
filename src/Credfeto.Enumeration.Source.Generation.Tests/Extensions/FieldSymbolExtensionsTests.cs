using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Formatting;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class FieldSymbolExtensionsTests : TestBase
{
    private static IFieldSymbol MakeField(string name, object? constantValue = null)
    {
        IFieldSymbol field = GetSubstitute<IFieldSymbol>();
        field.Name.Returns(name);
        field.ConstantValue.Returns(constantValue);
        field.GetAttributes().Returns([]);
        return field;
    }

    [Fact]
    public void BuildClassMemberNameReturnsClassDotMemberName()
    {
        IFieldSymbol field = MakeField("MY_VALUE");
        string result = field.BuildClassMemberName("MyClass");

        Assert.Equal(expected: "MyClass.MY_VALUE", actual: result);
    }

    [Fact]
    public void FormatMemberReturnsFormattedCaseLine()
    {
        IFieldSymbol field = MakeField("VALUE_A");
        IFormatConfig formatConfig = GetSubstitute<IFormatConfig>();
        formatConfig.ClassName.Returns("MyClass");

        string result = field.FormatMember(attributeText: "\"Description\"", formatConfig: formatConfig);

        Assert.Equal(expected: "MyClass.VALUE_A => \"Description\",", actual: result);
    }

    [Fact]
    public void IsSkipEnumValueReturnsFalseForFirstUniqueValue()
    {
        IFieldSymbol field = MakeField("A", constantValue: 0);
        HashSet<string> names = new(System.StringComparer.Ordinal);

        bool result = field.IsSkipEnumValue(names: names, equalsValueIdentifiers: null);

        Assert.False(result, "IsSkipEnumValue should return false for first unique value");
        Assert.Contains("0", names);
    }

    [Fact]
    public void IsSkipEnumValueReturnsTrueForDuplicateConstantValue()
    {
        IFieldSymbol fieldA = MakeField("A", constantValue: 0);
        IFieldSymbol fieldB = MakeField("B", constantValue: 0);
        HashSet<string> names = new(System.StringComparer.Ordinal);

        bool firstResult = fieldA.IsSkipEnumValue(names: names, equalsValueIdentifiers: null);
        bool secondResult = fieldB.IsSkipEnumValue(names: names, equalsValueIdentifiers: null);

        Assert.False(firstResult, "IsSkipEnumValue should return false for first occurrence");
        Assert.True(secondResult, "IsSkipEnumValue should return true for duplicate constant value");
    }

    [Fact]
    public void IsSkipEnumValueReturnsTrueWhenEqualsValueIdentifierPointsToExistingName()
    {
        // B = A pattern: B has equalsValueIdentifier pointing to A, and A was already seen as a member name
        IFieldSymbol fieldB = MakeField("B", constantValue: 0);

        // Pre-populate names with "A" — simulating that A was already resolved as a member name
        HashSet<string> names = new(System.StringComparer.Ordinal) { "A" };
        Dictionary<string, string> equalsIdentifiers = new(System.StringComparer.Ordinal) { ["B"] = "A" };

        bool result = fieldB.IsSkipEnumValue(names: names, equalsValueIdentifiers: equalsIdentifiers);

        Assert.True(result, "IsSkipEnumValue should return true when equalsValueIdentifier points to existing name");
    }

    [Fact]
    public void IsSkipEnumValueReturnsFalseWhenEqualsValueIdentifierPointsToUnseenName()
    {
        IFieldSymbol fieldB = MakeField("B", constantValue: 1);
        HashSet<string> names = new(System.StringComparer.Ordinal);
        Dictionary<string, string> equalsIdentifiers = new(System.StringComparer.Ordinal) { ["B"] = "UnknownName" };

        // "UnknownName" has never been added to names, so it's not skipped via identifier
        // But it still checks the constant value path
        bool result = fieldB.IsSkipEnumValue(names: names, equalsValueIdentifiers: equalsIdentifiers);

        // "1" wasn't in names, so adds it and returns false
        Assert.False(result, "IsSkipEnumValue should return false when equalsValueIdentifier points to unseen name");
    }

    [Fact]
    public void IsSkipEnumValueReturnsFalseWhenNullConstantValue()
    {
        IFieldSymbol field = MakeField("A", constantValue: null);
        HashSet<string> names = new(System.StringComparer.Ordinal);

        bool result = field.IsSkipEnumValue(names: names, equalsValueIdentifiers: null);

        Assert.False(result, "IsSkipEnumValue should return false when constant value is null");
    }
}
