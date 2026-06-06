using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Extensions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class EnumerableExtensionsTests : TestBase
{
    [Fact]
    public void RemoveNullsFiltersOutNullsFromList()
    {
        List<string?> source = ["a", null, "b", null, "c"];
        IEnumerable<string> result = source.RemoveNulls();

        Assert.Equal(expected: ["a", "b", "c"], actual: result.ToList());
    }

    [Fact]
    public void RemoveNullsFiltersOutNullsFromIEnumerable()
    {
        IEnumerable<string?> source = GetEnumerableWithNulls();
        IEnumerable<string> result = source.RemoveNulls();

        Assert.Equal(expected: ["x", "y"], actual: result.ToList());
    }

    [Fact]
    public void RemoveNullsReturnsEmptyForAllNulls()
    {
        List<string?> source = [null, null];
        IEnumerable<string> result = source.RemoveNulls();

        Assert.Empty(result);
    }

    [Fact]
    public void ForEachIteratesOverList()
    {
        List<int> source = [1, 2, 3];
        List<int> seen = [];
        source.ForEach(seen.Add);

        Assert.Equal(expected: [1, 2, 3], actual: seen);
    }

    [Fact]
    public void ForEachIteratesOverNonListEnumerable()
    {
        IEnumerable<int> source = GetEnumerable();
        List<int> seen = [];
        source.ForEach(seen.Add);

        Assert.Equal(expected: [10, 20, 30], actual: seen);
    }

    [Fact]
    public void ForEachDoesNothingForEmptyList()
    {
        List<int> source = [];
        int count = 0;
        source.ForEach(_ => count++);

        Assert.Equal(expected: 0, actual: count);
    }

    private static IEnumerable<string?> GetEnumerableWithNulls()
    {
        yield return "x";
        yield return null;
        yield return "y";
    }

    private static IEnumerable<int> GetEnumerable()
    {
        yield return 10;
        yield return 20;
        yield return 30;
    }
}
