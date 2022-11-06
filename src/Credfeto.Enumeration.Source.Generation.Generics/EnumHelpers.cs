﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NonBlocking;

namespace Credfeto.Enumeration.Source.Generation.Generics;

public static class EnumHelpers
{
    private static readonly ConcurrentDictionary<Enum, string> CachedNames = new();

    private static readonly ConcurrentDictionary<Enum, string> CachedDescriptions = new();

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

    public static string GetDescriptionReflection<T>(this T value)
        where T : Enum
    {
        string valueAsString = value.GetNameReflection();
        FieldInfo? m = value.GetType()
                            .GetField(valueAsString);

        return m?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)
                .SingleOrDefault() is not DescriptionAttribute attribute
            ? valueAsString
            : attribute.Description;
    }

    public static string GetDescription<T>(this T value)
        where T : Enum
    {
        if (CachedDescriptions.TryGetValue(key: value, out string? name))
        {
            return name;
        }

        return CachedDescriptions.GetOrAdd(key: value, value.GetDescriptionReflection());
    }
}