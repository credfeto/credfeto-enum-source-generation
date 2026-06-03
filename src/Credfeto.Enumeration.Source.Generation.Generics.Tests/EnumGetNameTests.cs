using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumGetNameTests
{
    [Theory]
    [InlineData(TestEnumWithDescription.WITHOUT_DESCRIPTION, nameof(TestEnumWithDescription.WITHOUT_DESCRIPTION))]
    [InlineData(TestEnumWithDescription.WITH_DESCRIPTION, nameof(TestEnumWithDescription.WITH_DESCRIPTION))]
    public void GetNameReturnsCachedName(TestEnumWithDescription value, string expected)
    {
        string firstResult = value.GetName();
        string secondResult = value.GetName();

        Assert.Equal(expected: expected, actual: firstResult);
        Assert.Equal(expected: expected, actual: secondResult);
    }
}
