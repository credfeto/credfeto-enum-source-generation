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
public sealed class EnumGetDescriptionTests
{
    [Theory]
    [InlineData(ExampleEnumValues.ZERO, nameof(ExampleEnumValues.ZERO))]
    [InlineData(ExampleEnumValues.ONE, "One \"1\"")]
    [InlineData(ExampleEnumValues.THREE, "Two but one better!")]
    public void GetDescriptionReturnsExpected(ExampleEnumValues value, string expected)
    {
        Assert.Equal(expected: expected, actual: value.GetDescription());
    }

    [Fact]
    public void GetDescriptionForAliasedMatchesOriginal()
    {
        Assert.Equal(
            expected: ExampleEnumValues.ONE.GetDescription(),
            actual: ExampleEnumValues.SAME_AS_ONE.GetDescription()
        );
    }

    [Fact]
    public void GetDescriptionForObsoleteValueThrows()
    {
        // ExampleEnumValues.TWO (value 2) is excluded from the generated switch — falls through to GetName which throws
        const ExampleEnumValues twoByValue = (ExampleEnumValues)2;
#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => twoByValue.GetDescription());
#else
        Assert.Throws<ArgumentOutOfRangeException>(() => twoByValue.GetDescription());
#endif
    }

    [Fact]
    public void GetDescriptionForUnknownValueThrows()
    {
        const ExampleEnumValues unknown = (ExampleEnumValues)72;
#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => unknown.GetDescription());
#else
        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.GetDescription());
#endif
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, nameof(HttpStatusCode.OK))]
    [InlineData(HttpStatusCode.Accepted, nameof(HttpStatusCode.Accepted))]
    public void GetDescriptionForExternalEnumReturnsName(HttpStatusCode value, string expected)
    {
        Assert.Equal(expected: expected, actual: value.GetDescription());
    }
}
