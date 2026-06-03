using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class IsDefinedTests
{
    [Theory]
    [InlineData(TestEnumWithDescription.WITHOUT_DESCRIPTION)]
    [InlineData(TestEnumWithDescription.WITH_DESCRIPTION)]
    public void IsDefinedReturnsTrueForDefinedValue(TestEnumWithDescription value)
    {
        bool firstResult = value.IsDefined();
        bool secondResult = value.IsDefined();

        Assert.True(firstResult, $"{value} should be a defined enum member");
        Assert.True(secondResult, $"{value} should still be a defined enum member on second lookup");
    }

    [Fact]
    public void IsDefinedReturnsFalseForUndefinedValue()
    {
        const TestEnumWithDescription unknown = (TestEnumWithDescription)999;

        bool firstResult = unknown.IsDefined();
        bool secondResult = unknown.IsDefined();

        Assert.False(firstResult, "Cast value 999 is not a defined enum member");
        Assert.False(secondResult, "Cast value 999 is still not a defined enum member on second lookup");
    }
}
