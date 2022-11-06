using System;
using NonBlocking;

namespace Credfeto.Enumeration.Source.Generation.Generics;

public static class EnumHelpers
{
    private static readonly ConcurrentDictionary<Enum, string> CachedNames = new();

    public static string GetNameReflection<T>(this T value)
        where T : Enum
    {
        return Enum.GetName(value.GetType(), value: value)!;
    }

    public static string GetName<T>(this T value)
        where T : Enum
    {
        if (CachedNames.TryGetValue(key: value, out string? name))
        {
            return name;
        }

        return CachedNames.GetOrAdd(key: value, value.GetNameReflection());
    }
}