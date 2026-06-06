using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumGetDescriptionReflectionTests
{
    [Fact]
    public void GetDescriptionReflectionReturnsNameWhenNoDescriptionAttribute()
    {
        Assert.Equal(
            expected: nameof(TestEnumWithDescription.WITHOUT_DESCRIPTION),
            actual: TestEnumWithDescription.WITHOUT_DESCRIPTION.GetDescriptionReflection()
        );
    }

    [Fact]
    public void GetDescriptionReflectionReturnsDescriptionWhenAttributePresent()
    {
        Assert.Equal(
            expected: "First described value",
            actual: TestEnumWithDescription.WITH_DESCRIPTION.GetDescriptionReflection()
        );
    }

    [Fact]
    public void GetDescriptionReflectionReturnsEmptyStringForUndefinedValue()
    {
        const TestEnumWithDescription unknown = (TestEnumWithDescription)999;

        Assert.Equal(expected: string.Empty, actual: unknown.GetDescriptionReflection());
    }
}
