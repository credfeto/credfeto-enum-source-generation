using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation.Models.Tests;

[SuppressMessage(category: "FunFair.CodeAnalysis", checkId: "FFS0013: Test classes should be derived from TestBase", Justification = "Not in this case")]
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

    [Fact]
    public void GetDescriptionForUnknown()
    {
        const ExampleEnumValues unknown = (ExampleEnumValues)72;
#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => unknown.GetDescription());
        #else
        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.GetDescription());
#endif
    }

    [Fact]
    public void GetDescriptionForExternalUnAliased()
    {
        Assert.Equal(expected: "OK", HttpStatusCode.OK.GetDescription());
        Assert.Equal(expected: "Accepted", HttpStatusCode.Accepted.GetDescription());
    }
}