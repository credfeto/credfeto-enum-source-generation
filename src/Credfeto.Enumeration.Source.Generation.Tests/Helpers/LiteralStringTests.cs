using Credfeto.Enumeration.Source.Generation.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Helpers;

public sealed class LiteralStringTests : TestBase
{
    [Fact]
    public void ToStringReturnsUnderlyingValue()
    {
        LiteralString literal = new("hello world");

        // LocalizableString.ToString() calls the protected GetText() method
        string result = literal.ToString(formatProvider: null);

        Assert.Equal(expected: "hello world", actual: result);
    }

    [Fact]
    public void ToStringWithFormatProviderReturnsUnderlyingValue()
    {
        LiteralString literal = new("test value");

        string result = literal.ToString(System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(expected: "test value", actual: result);
    }

    [Fact]
    public void GetHashCodeReturnsDeterministicValue()
    {
        LiteralString literal1 = new("same");
        LiteralString literal2 = new("same");

        Assert.Equal(expected: literal1.GetHashCode(), actual: literal2.GetHashCode());
    }

    [Fact]
    public void GetHashCodeDiffersForDifferentValues()
    {
        LiteralString literal1 = new("abc");
        LiteralString literal2 = new("xyz");

        // Not guaranteed to be different by contract, but for distinct short strings it should differ
        Assert.NotEqual(literal1.GetHashCode(), literal2.GetHashCode());
    }

    [Fact]
    public void EqualsSameCaseReturnsTrue()
    {
        LiteralString literal1 = new("hello");
        LiteralString literal2 = new("hello");

        Assert.True(literal1.Equals(literal2), "Same-case LiteralStrings should be equal");
    }

    [Fact]
    public void EqualsDifferentCaseReturnsTrue()
    {
        // AreEqual uses OrdinalIgnoreCase
        LiteralString literal1 = new("Hello");
        LiteralString literal2 = new("hello");

        Assert.True(literal1.Equals(literal2), "Different-case LiteralStrings should be equal (OrdinalIgnoreCase)");
    }

    [Fact]
    public void EqualsDifferentValueReturnsFalse()
    {
        LiteralString literal1 = new("hello");
        LiteralString literal2 = new("world");

        Assert.False(literal1.Equals(literal2), "LiteralStrings with different values should not be equal");
    }

    [Fact]
    public void EqualsNonLiteralStringReturnsFalse()
    {
        LiteralString literal = new("hello");

        Assert.False(literal.Equals("hello"), "LiteralString should not equal a plain string");
    }
}
