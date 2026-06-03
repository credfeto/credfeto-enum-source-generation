using System;
using System.Diagnostics.CodeAnalysis;
using Credfeto.Enumeration.Source.Generation.Attributes;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Attributes.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumTextAttributeTests
{
    [Fact]
    public void ConstructorWithValidEnumTypeSetsEnumProperty()
    {
        EnumTextAttribute attribute = new(typeof(DayOfWeek));
        Assert.Equal(expected: typeof(DayOfWeek), actual: attribute.Enum);
    }

    [Fact]
    public void ConstructorWithNonEnumTypeThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new EnumTextAttribute(typeof(string)));
    }
}
