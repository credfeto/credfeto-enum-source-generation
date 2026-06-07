using Credfeto.Enumeration.Source.Generation.Formatting;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Formatting;

public sealed class ClassWithNamespaceFormatterTests : TestBase
{
    [Fact]
    public void ClassNameIsNamespaceDotName()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);
        EnumGeneration enumGeneration = new(
            accessType: AccessType.PUBLIC,
            name: "MyEnum",
            @namespace: "My.Namespace",
            members: [],
            location: Location.None,
            options: options
        );

        ClassWithNamespaceFormatter formatter = new(enumGeneration);

        Assert.Equal(expected: "My.Namespace.MyEnum", actual: formatter.ClassName);
    }

    [Fact]
    public void ClassNameCombinesNamespaceAndName()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);
        EnumGeneration enumGeneration = new(
            accessType: AccessType.INTERNAL,
            name: "Status",
            @namespace: "App.Domain",
            members: [],
            location: Location.None,
            options: options
        );

        ClassWithNamespaceFormatter formatter = new(enumGeneration);

        Assert.Equal(expected: "App.Domain.Status", actual: formatter.ClassName);
    }
}
