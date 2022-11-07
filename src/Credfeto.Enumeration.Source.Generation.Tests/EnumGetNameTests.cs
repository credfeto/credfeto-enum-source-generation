using Credfeto.Enumeration.Source.Generation.Models;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class EnumGetNameTests
{
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
}