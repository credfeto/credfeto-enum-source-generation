using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Models.Tests;

[SuppressMessage(category: "FunFair.CodeAnalysis", checkId: "FFS0013: Test classes should be derived from TestBase", Justification = "Not in this case")]
public sealed class EnumGetNameTests
{
    [Theory]
    [InlineData(ExampleEnumValues.ZERO)]
    [InlineData(ExampleEnumValues.ONE)]
    [InlineData(ExampleEnumValues.THREE)]
    public void IsNameNameAsToString(ExampleEnumValues value)
    {
        string name = value.GetName();

        Assert.Equal(value.ToString(), actual: name);
    }

    [Fact]
    public void GetNameForAliased()
    {
        Assert.Equal(expected: "ONE", ExampleEnumValues.ONE.GetName());
        Assert.Equal(expected: "ONE", ExampleEnumValues.SAME_AS_ONE.GetName());
    }

    [Fact]
    public void GetNameForUnAliased()
    {
        Assert.Equal(expected: "ZERO", ExampleEnumValues.ZERO.GetName());
        Assert.Equal(expected: "THREE", ExampleEnumValues.THREE.GetName());
    }

    [Fact]
    public void GetNameForUnknown()
    {
        const ExampleEnumValues unknown = (ExampleEnumValues)72;

        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.GetName());
    }

    [Fact]
    public void GetNameForExternalUnAliased()
    {
        Assert.Equal(expected: "OK", HttpStatusCode.OK.GetName());
        Assert.Equal(expected: "Accepted", HttpStatusCode.Accepted.GetName());
    }
}