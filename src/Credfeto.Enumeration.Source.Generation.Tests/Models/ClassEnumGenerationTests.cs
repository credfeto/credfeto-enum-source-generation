using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Models;

public sealed class ClassEnumGenerationTests : TestBase
{
    [Fact]
    public void ConstructorSetsAllProperties()
    {
        const AccessType accessType = AccessType.PUBLIC;
        const string name = "MyClass";
        const string ns = "My.Namespace";
        IReadOnlyList<EnumGeneration> enums = [];
        Location location = Location.None;

        ClassEnumGeneration classEnumGeneration = new(
            accessType: accessType,
            name: name,
            @namespace: ns,
            enums: enums,
            location: location
        );

        Assert.Equal(expected: accessType, actual: classEnumGeneration.AccessType);
        Assert.Equal(expected: name, actual: classEnumGeneration.Name);
        Assert.Equal(expected: ns, actual: classEnumGeneration.Namespace);
        Assert.Same(expected: enums, actual: classEnumGeneration.Enums);
        Assert.Same(expected: location, actual: classEnumGeneration.Location);
    }

    [Fact]
    public void EqualityReturnsTrueForSameValues()
    {
        IReadOnlyList<EnumGeneration> enums = [];
        Location location = Location.None;

        ClassEnumGeneration first = new(
            accessType: AccessType.INTERNAL,
            name: "Foo",
            @namespace: "Bar",
            enums: enums,
            location: location
        );

        ClassEnumGeneration second = new(
            accessType: AccessType.INTERNAL,
            name: "Foo",
            @namespace: "Bar",
            enums: enums,
            location: location
        );

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public void EqualityReturnsFalseForDifferentName()
    {
        IReadOnlyList<EnumGeneration> enums = [];
        Location location = Location.None;

        ClassEnumGeneration first = new(
            accessType: AccessType.PUBLIC,
            name: "First",
            @namespace: "NS",
            enums: enums,
            location: location
        );

        ClassEnumGeneration second = new(
            accessType: AccessType.PUBLIC,
            name: "Second",
            @namespace: "NS",
            enums: enums,
            location: location
        );

        Assert.NotEqual(expected: first, actual: second);
    }

    [Fact]
    public void EqualityReturnsTrueForDifferentEnumListReferencesSameContent()
    {
        Location location = Location.None;
        EnumGeneration enumGen = new(
            accessType: AccessType.PUBLIC,
            name: "TestEnum",
            @namespace: "Test.NS",
            members: [],
            location: Location.None,
            options: default
        );
        IReadOnlyList<EnumGeneration> enums1 = [enumGen];
        IReadOnlyList<EnumGeneration> enums2 = [enumGen];

        Assert.NotSame(expected: enums1, actual: enums2);

        ClassEnumGeneration first = new(
            accessType: AccessType.INTERNAL,
            name: "Foo",
            @namespace: "Bar",
            enums: enums1,
            location: location
        );

        ClassEnumGeneration second = new(
            accessType: AccessType.INTERNAL,
            name: "Foo",
            @namespace: "Bar",
            enums: enums2,
            location: location
        );

        Assert.Equal(expected: first, actual: second);
    }
}
