using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumGetDescriptionTests
{
    [Fact]
    public void GetDescriptionReturnsCachedNameWhenNoDescriptionAttribute()
    {
        string firstResult = TestEnumWithDescription.WITHOUT_DESCRIPTION.GetDescription();
        string secondResult = TestEnumWithDescription.WITHOUT_DESCRIPTION.GetDescription();

        Assert.Equal(expected: nameof(TestEnumWithDescription.WITHOUT_DESCRIPTION), actual: firstResult);
        Assert.Equal(expected: nameof(TestEnumWithDescription.WITHOUT_DESCRIPTION), actual: secondResult);
    }

    [Fact]
    public void GetDescriptionReturnsCachedDescriptionWhenAttributePresent()
    {
        string firstResult = TestEnumWithDescription.WITH_DESCRIPTION.GetDescription();
        string secondResult = TestEnumWithDescription.WITH_DESCRIPTION.GetDescription();

        Assert.Equal(expected: "First described value", actual: firstResult);
        Assert.Equal(expected: "First described value", actual: secondResult);
    }
}
