using System.Collections.Generic;
using System.Linq;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> source)
    {
        return from item in source where item is not null select item;
    }
}
