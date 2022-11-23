using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NonBlocking;

namespace Credfeto.Enumeration.Source.Generation.Generics;

public static class EnumHelpers
{
    private static readonly ConcurrentDictionary<Enum, string> CachedNames = new();

    private static readonly ConcurrentDictionary<Enum, string> CachedDescriptions = new();

    private static readonly ConcurrentDictionary<Enum, bool> CachedDefined = new();

    [SuppressMessage(category: "Credfeto.Enumeration.Source.Generation", checkId: "ENUM001: Use GetName", Justification = "Enum is a value type")]
    [SuppressMessage(category: "ToStringWithoutOverrideAnalyzer", checkId: "ExplicitToStringWithoutOverrideAnalyzer: Use GetName", Justification = "Enum is a value type")]
    public static string GetNameToString<T>(this T value)
        where T : Enum
    {
        return value.ToString();
    }

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

    public static bool IsDefinedReflection<T>(this T value)
        where T : Enum
    {
        return Enum.IsDefined(value.GetType(), value: value);
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

    public static bool IsDefined<T>(this T value)
        where T : Enum
    {
        if (CachedDefined.TryGetValue(key: value, out bool defined))
        {
            return defined;
        }

        return CachedDefined.GetOrAdd(key: value, value.IsDefinedReflection());
    }
}