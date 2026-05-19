using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Formatting;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class FieldSymbolExtensions
{
    public static string BuildClassMemberName(this IFieldSymbol member, string className)
    {
        return $"{className}.{member.Name}";
    }

    public static string FormatMember(this IFieldSymbol member, string attributeText, IFormatConfig formatConfig)
    {
        return $"{formatConfig.ClassName}.{member.Name} => {attributeText},";
    }

    public static bool IsSkipEnumValue(
        this IFieldSymbol member,
        HashSet<string> names,
        IReadOnlyDictionary<string, string>? equalsValueIdentifiers
    )
    {
        if (
            equalsValueIdentifiers is not null
            && equalsValueIdentifiers.TryGetValue(member.Name, out string? identifierName)
            && names.Contains(identifierName)
        )
        {
            // note deliberately ignoring the return value here as we need to record the integer value as being skipped too
            _ = member.IsSkipConstantValue(names: names);

            return true;
        }

        return member.IsSkipConstantValue(names: names);
    }

    private static bool IsSkipConstantValue(this IFieldSymbol member, HashSet<string> names)
    {
        object? cv = member.ConstantValue;

        return cv is not null && !names.Add(cv.ToString());
    }
}
