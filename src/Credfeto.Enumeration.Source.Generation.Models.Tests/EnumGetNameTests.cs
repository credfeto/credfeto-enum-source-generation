using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Models.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumGetNameTests
{
    [Theory]
    [InlineData(ExampleEnumValues.ZERO, nameof(ExampleEnumValues.ZERO))]
    [InlineData(ExampleEnumValues.ONE, nameof(ExampleEnumValues.ONE))]
    [InlineData(ExampleEnumValues.THREE, nameof(ExampleEnumValues.THREE))]
    public void GetNameReturnsExpectedName(ExampleEnumValues value, string expected)
    {
        string name = value.GetName();

        Assert.Equal(expected: expected, actual: name);
    }

    [Fact]
    public void GetNameForAliasedMatchesOriginal()
    {
        Assert.Equal(expected: ExampleEnumValues.ONE.GetName(), actual: ExampleEnumValues.SAME_AS_ONE.GetName());
    }

    [Fact]
    public void GetNameForObsoleteValueThrows()
    {
        // ExampleEnumValues.TWO (value 2) is not included in the generated switch — treat as out-of-range
        const ExampleEnumValues twoByValue = (ExampleEnumValues)2;
#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => twoByValue.GetName());
#else
        Assert.Throws<ArgumentOutOfRangeException>(() => twoByValue.GetName());
#endif
    }

    [Fact]
    public void GetNameForUnknownValueThrows()
    {
        const ExampleEnumValues unknown = (ExampleEnumValues)72;

#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => unknown.GetName());
#else
        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.GetName());
#endif
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, nameof(HttpStatusCode.OK))]
    [InlineData(HttpStatusCode.Accepted, nameof(HttpStatusCode.Accepted))]
    [InlineData(HttpStatusCode.NotFound, nameof(HttpStatusCode.NotFound))]
    public void GetNameForExternalEnumReturnsMemberName(HttpStatusCode value, string expected)
    {
        Assert.Equal(expected: expected, actual: value.GetName());
    }
}
