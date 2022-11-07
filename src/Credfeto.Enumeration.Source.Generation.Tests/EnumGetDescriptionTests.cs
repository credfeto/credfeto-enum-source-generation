using Credfeto.Enumeration.Source.Generation.Models;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class EnumGetDescriptionTests
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
}