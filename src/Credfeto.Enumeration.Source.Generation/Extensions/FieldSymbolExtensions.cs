using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class FieldSymbolExtensions
{
    public static string BuildClassMemberName(this IFieldSymbol member, string className)
    {
        return $"{className}.{member.Name}";
    }

    public static string FormatMember(
        this IFieldSymbol member,
        string attributeText,
        IFormatConfig formatConfig
    )
    {
        return $"{formatConfig.ClassName}.{member.Name} => {attributeText},";
    }

    public static bool IsSkipEnumValue(this IFieldSymbol member, HashSet<string> names)
    {
        EnumMemberDeclarationSyntax? syntax = FindEnumMemberDeclarationSyntax(member);

        if (syntax?.EqualsValue is null || !syntax.EqualsValue.Value.IsSkipEnumValue(names: names))
        {
            return member.IsSkipConstantValue(names: names);
        }

        // note deliberately ignoring the return value here as we need to record the integer value as being skipped too
        _ = member.IsSkipConstantValue(names: names);

        return true;
    }

    private static bool IsSkipConstantValue(this IFieldSymbol member, HashSet<string> names)
    {
        object? cv = member.ConstantValue;

        return cv is not null && !names.Add(cv.ToString());
    }

    private static EnumMemberDeclarationSyntax? FindEnumMemberDeclarationSyntax(IFieldSymbol member)
    {
        return member.DeclaringSyntaxReferences.Select(dsr => dsr.GetDeclaredSyntax())
                     .RemoveNulls()
                     .FirstOrDefault();
    }
}