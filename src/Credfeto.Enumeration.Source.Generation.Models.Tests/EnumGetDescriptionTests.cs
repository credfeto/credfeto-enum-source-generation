using System.Net;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Models.Tests;

public sealed class EnumGetDescriptionTests : TestBase
{
    [Fact]
    public void GetDescriptionForAliased()
    {
        Assert.Equal(expected: "One \"1\"", ExampleEnumValues.ONE.GetDescription());
        Assert.Equal(expected: "One \"1\"", ExampleEnumValues.SAME_AS_ONE.GetDescription());
    }

    [Fact]
    public void GetDescriptionForUnAliased()
    {
        Assert.Equal(expected: "ZERO", ExampleEnumValues.ZERO.GetDescription());
        Assert.Equal(expected: "Two but one better!", ExampleEnumValues.THREE.GetDescription());
    }

    [Fact]
    public void GetDescriptionForExternalUnAliased()
    {
        Assert.Equal(expected: "OK", HttpStatusCode.OK.GetDescription());
        Assert.Equal(expected: "Accepted", HttpStatusCode.Accepted.GetDescription());
    }
}