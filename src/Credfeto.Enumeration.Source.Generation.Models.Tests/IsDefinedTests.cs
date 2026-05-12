using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Models.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class IsDefinedTests
{
    [Theory]
    [InlineData(ExampleEnumValues.ZERO)]
    [InlineData(ExampleEnumValues.ONE)]
    [InlineData(ExampleEnumValues.THREE)]
    public void IsDefinedForKnownValueReturnsTrue(ExampleEnumValues value)
    {
        Assert.True(value.IsDefined(), $"{value} should be a defined enum member");
    }

    [Fact]
    public void IsDefinedForAliasedValueReturnsTrue()
    {
        Assert.True(ExampleEnumValues.SAME_AS_ONE.IsDefined(), "SAME_AS_ONE is an alias for ONE and should be defined");
    }

    [Fact]
    public void IsDefinedForObsoleteValueReturnsFalse()
    {
        // ExampleEnumValues.TWO (value 2) is excluded from the generated switch
        const ExampleEnumValues twoByValue = (ExampleEnumValues)2;
        Assert.False(
            twoByValue.IsDefined(),
            "TWO (value 2) is excluded from the generated member list and should not be defined"
        );
    }

    [Fact]
    public void IsDefinedForUnknownValueReturnsFalse()
    {
        const ExampleEnumValues unknown = (ExampleEnumValues)72;
        Assert.False(unknown.IsDefined(), "Unknown cast value should not be defined");
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NotFound)]
    public void IsDefinedForKnownHttpStatusCodeReturnsTrue(HttpStatusCode value)
    {
        Assert.True(value.IsDefined(), $"HttpStatusCode.{value} should be defined");
    }

    [Fact]
    public void IsDefinedForUnknownHttpStatusCodeReturnsFalse()
    {
        const HttpStatusCode unknown = (HttpStatusCode)9999;
        Assert.False(unknown.IsDefined(), "Unknown HttpStatusCode cast value should not be defined");
    }
}
