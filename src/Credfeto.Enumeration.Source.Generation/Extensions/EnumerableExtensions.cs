using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> source)
    {
        return from item in source where item is not null select item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        if (enumeration is List<T> list)
        {
            list.ForEach(action);
            return;
        }

        ForEachEnumerable(enumeration: enumeration, action: action);
    }

    private static void ForEachEnumerable<T>(IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (T item in enumeration)
        {
            action(item);
        }
    }
}
