using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Models;

public sealed class EnumGenerationTests : TestBase
{
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
        IReadOnlyDictionary<string, string> identifiers = new Dictionary<string, string>(System.StringComparer.Ordinal)
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
}
