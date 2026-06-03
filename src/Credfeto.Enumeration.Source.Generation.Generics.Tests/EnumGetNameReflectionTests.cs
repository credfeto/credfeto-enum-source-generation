using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumGetNameReflectionTests
{
    [Theory]
    [InlineData(TestEnumWithDescription.WITHOUT_DESCRIPTION, nameof(TestEnumWithDescription.WITHOUT_DESCRIPTION))]
    [InlineData(TestEnumWithDescription.WITH_DESCRIPTION, nameof(TestEnumWithDescription.WITH_DESCRIPTION))]
    public void GetNameReflectionReturnsNameForDefinedValue(TestEnumWithDescription value, string expected)
    {
        Assert.Equal(expected: expected, actual: value.GetNameReflection());
    }

    [Fact]
    public void GetNameReflectionReturnsEmptyStringForUndefinedValue()
    {
        const TestEnumWithDescription unknown = (TestEnumWithDescription)999;

        Assert.Equal(expected: string.Empty, actual: unknown.GetNameReflection());
    }
}
