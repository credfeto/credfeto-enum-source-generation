using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class IsDefinedReflectionTests
{
    [Theory]
    [InlineData(TestEnumWithDescription.WITHOUT_DESCRIPTION)]
    [InlineData(TestEnumWithDescription.WITH_DESCRIPTION)]
    public void IsDefinedReflectionReturnsTrueForDefinedValue(TestEnumWithDescription value)
    {
        Assert.True(value.IsDefinedReflection(), $"{value} should be a defined enum member");
    }

    [Fact]
    public void IsDefinedReflectionReturnsFalseForUndefinedValue()
    {
        const TestEnumWithDescription unknown = (TestEnumWithDescription)999;

        Assert.False(unknown.IsDefinedReflection(), "Cast value 999 is not a defined enum member");
    }
}
