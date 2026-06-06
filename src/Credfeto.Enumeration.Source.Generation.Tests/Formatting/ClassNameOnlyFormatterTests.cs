using Credfeto.Enumeration.Source.Generation.Formatting;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Formatting;

public sealed class ClassNameOnlyFormatterTests : TestBase
{
    [Fact]
    public void ClassNameIsSetToEnumName()
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

        ClassNameOnlyFormatter formatter = new(enumGeneration);

        Assert.Equal(expected: "MyEnum", actual: formatter.ClassName);
    }

    [Fact]
    public void ClassNameDoesNotIncludeNamespace()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);
        EnumGeneration enumGeneration = new(
            accessType: AccessType.INTERNAL,
            name: "StatusCode",
            @namespace: "Some.Deep.Namespace",
            members: [],
            location: Location.None,
            options: options
        );

        ClassNameOnlyFormatter formatter = new(enumGeneration);

        Assert.Equal(expected: "StatusCode", actual: formatter.ClassName);
        Assert.DoesNotContain(".", formatter.ClassName, System.StringComparison.Ordinal);
    }
}
