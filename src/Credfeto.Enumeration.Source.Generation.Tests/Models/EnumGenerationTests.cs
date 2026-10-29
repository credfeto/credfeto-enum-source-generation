using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Models;

public sealed class EnumGenerationTests : TestBase
{
    private static EnumGeneration Create(string name)
    {
        return new(
            accessType: AccessType.PUBLIC,
            name: name,
            @namespace: "Bar",
            members: [],
            location: Location.None,
            options: default
        );
    }

    [Fact]
    public void EqualsObjectReturnsTrueForEqualValue()
    {
        EnumGeneration a = Create("Foo");
        object b = Create("Foo");

        Assert.True(a.Equals(b), userMessage: "Value should equal an equal boxed value");
    }

    [Fact]
    public void EqualsObjectReturnsFalseForNull()
    {
        EnumGeneration a = Create("Foo");

        Assert.False(a.Equals(null), userMessage: "Value should not equal null");
    }

    [Fact]
    public void EqualsObjectReturnsFalseForDifferentType()
    {
        EnumGeneration a = Create("Foo");

        Assert.False(a.Equals(new object()), userMessage: "Value should not equal an object of a different type");
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentValue()
    {
        EnumGeneration a = Create("Foo");
        EnumGeneration b = Create("Different");

        Assert.False(a.Equals(b), userMessage: "Different values should not be equal");
        Assert.False(a == b, userMessage: "Equality operator should return false for different values");
        Assert.True(a != b, userMessage: "Inequality operator should return true for different values");
    }

    [Fact]
    public void OperatorsReturnTrueForEqualValue()
    {
        EnumGeneration a = Create("Foo");
        EnumGeneration b = Create("Foo");

        Assert.True(a == b, userMessage: "Equality operator should return true for equal values");
        Assert.False(a != b, userMessage: "Inequality operator should return false for equal values");
    }

    [Fact]
    public void GetHashCodeIsSameForEqualValues()
    {
        EnumGeneration a = Create("Foo");
        EnumGeneration b = Create("Foo");

        Assert.Equal(expected: a.GetHashCode(), actual: b.GetHashCode());
    }

    [Fact]
    public void ConstructorSetsAllPropertiesWithoutEqualsValueIdentifiers()
    {
        const AccessType accessType = AccessType.PUBLIC;
        const string name = "MyEnum";
        const string ns = "My.Namespace";
        IReadOnlyList<IFieldSymbol> members = [];
        Location location = Location.None;
        GenerationOptions options = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: false);

        EnumGeneration enumGeneration = new(
            accessType: accessType,
            name: name,
            @namespace: ns,
            members: members,
            location: location,
            options: options
        );

        Assert.Equal(expected: accessType, actual: enumGeneration.AccessType);
        Assert.Equal(expected: name, actual: enumGeneration.Name);
        Assert.Equal(expected: ns, actual: enumGeneration.Namespace);
        Assert.Same(expected: members, actual: enumGeneration.Members);
        Assert.Same(expected: location, actual: enumGeneration.Location);
        Assert.Equal(expected: options, actual: enumGeneration.Options);
        Assert.Null(enumGeneration.EqualsValueIdentifiers);
    }

    [Fact]
    public void ConstructorSetsEqualsValueIdentifiersWhenProvided()
    {
        IReadOnlyList<IFieldSymbol> members = [];
        Location location = Location.None;
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);
        IReadOnlyDictionary<string, string> identifiers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["B"] = "A",
        };

        EnumGeneration enumGeneration = new(
            accessType: AccessType.INTERNAL,
            name: "TestEnum",
            @namespace: "Test",
            members: members,
            location: location,
            options: options,
            equalsValueIdentifiers: identifiers
        );

        Assert.NotNull(enumGeneration.EqualsValueIdentifiers);
        Assert.Equal(expected: "A", actual: enumGeneration.EqualsValueIdentifiers["B"]);
    }

    [Fact]
    public void EqualityReturnsTrueForSameValues()
    {
        IReadOnlyList<IFieldSymbol> members = [];
        Location location = Location.None;
        GenerationOptions options = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: true);

        EnumGeneration first = new(
            accessType: AccessType.PUBLIC,
            name: "Foo",
            @namespace: "Bar",
            members: members,
            location: location,
            options: options
        );

        EnumGeneration second = new(
            accessType: AccessType.PUBLIC,
            name: "Foo",
            @namespace: "Bar",
            members: members,
            location: location,
            options: options
        );

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public void EqualityReturnsFalseForSameMemberNamesButDifferentConstantValues()
    {
        CSharpCompilation comp1 = CompilationHelpers.CreateCompilation(
            "namespace N { public enum E { A = 0, B = 0 } }"
        );
        CSharpCompilation comp2 = CompilationHelpers.CreateCompilation(
            "namespace N { public enum E { A = 0, B = 1 } }"
        );

        IReadOnlyList<IFieldSymbol> members1 =
        [
            .. (comp1.GetTypeByMetadataName("N.E")?.GetMembers().OfType<IFieldSymbol>() ?? []),
        ];
        IReadOnlyList<IFieldSymbol> members2 =
        [
            .. (comp2.GetTypeByMetadataName("N.E")?.GetMembers().OfType<IFieldSymbol>() ?? []),
        ];

        Assert.NotEmpty(members1);
        Assert.Equal(expected: members1.Count, actual: members2.Count);

        Location location = Location.None;
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        EnumGeneration first = new(
            accessType: AccessType.PUBLIC,
            name: "E",
            @namespace: "N",
            members: members1,
            location: location,
            options: options
        );

        EnumGeneration second = new(
            accessType: AccessType.PUBLIC,
            name: "E",
            @namespace: "N",
            members: members2,
            location: location,
            options: options
        );

        Assert.NotEqual(expected: first, actual: second);
    }

    [Fact]
    public void EqualityReturnsTrueForDifferentMembersListReferencesWithSameContent()
    {
        IReadOnlyList<IFieldSymbol> members1 = [];
        IReadOnlyList<IFieldSymbol> members2 = [];
        Location location = Location.None;
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        EnumGeneration first = new(
            accessType: AccessType.PUBLIC,
            name: "Foo",
            @namespace: "Bar",
            members: members1,
            location: location,
            options: options
        );

        EnumGeneration second = new(
            accessType: AccessType.PUBLIC,
            name: "Foo",
            @namespace: "Bar",
            members: members2,
            location: location,
            options: options
        );

        Assert.Equal(expected: first, actual: second);
    }
}
