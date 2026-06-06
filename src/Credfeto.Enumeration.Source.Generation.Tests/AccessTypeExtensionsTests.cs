using System;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class AccessTypeExtensionsTests : TestBase
{
    [Theory]
    [InlineData(AccessType.PUBLIC, "public")]
    [InlineData(AccessType.PRIVATE, "private")]
    [InlineData(AccessType.PROTECTED, "protected")]
    [InlineData(AccessType.PROTECTED_INTERNAL, "protected internal")]
    [InlineData(AccessType.INTERNAL, "internal")]
    public void ConvertAccessTypeReturnsExpectedString(AccessType accessType, string expected)
    {
        string result = accessType.ConvertAccessType();
        Assert.Equal(expected: expected, actual: result);
    }

    [Fact]
    public void ConvertAccessTypeThrowsForUnknownValue()
    {
        const AccessType invalid = (AccessType)999;
        Assert.Throws<ArgumentOutOfRangeException>(() => invalid.ConvertAccessType());
    }
}
