using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Models;

public sealed class GenerationOptionsTests : TestBase
{
    [Fact]
    public void ConstructorSetsBothProperties()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: false);

        Assert.True(options.HasDoesNotReturnAttribute, "HasDoesNotReturnAttribute should be true");
        Assert.False(options.SupportsUnreachableException, "SupportsUnreachableException should be false");
    }

    [Fact]
    public void ConstructorWithBothFalse()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        Assert.False(options.HasDoesNotReturnAttribute, "HasDoesNotReturnAttribute should be false");
        Assert.False(options.SupportsUnreachableException, "SupportsUnreachableException should be false");
    }

    [Fact]
    public void ConstructorWithBothTrue()
    {
        GenerationOptions options = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: true);

        Assert.True(options.HasDoesNotReturnAttribute, "HasDoesNotReturnAttribute should be true");
        Assert.True(options.SupportsUnreachableException, "SupportsUnreachableException should be true");
    }

    [Fact]
    public void EqualityReturnsTrueForSameValues()
    {
        GenerationOptions first = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: true);
        GenerationOptions second = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: true);

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public void EqualityReturnsFalseForDifferentValues()
    {
        GenerationOptions first = new(hasDoesNotReturnAttribute: true, supportsUnreachableException: true);
        GenerationOptions second = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: true);

        Assert.NotEqual(expected: first, actual: second);
    }
}
