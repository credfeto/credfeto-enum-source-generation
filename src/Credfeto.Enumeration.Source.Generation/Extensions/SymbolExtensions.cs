using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

public static class SymbolExtensions
{
    private static readonly Type ObsoleteType = typeof(ObsoleteAttribute);
    private static readonly Type DescriptionType = typeof(DescriptionAttribute);

    public static bool HasObsoleteAttribute(this ISymbol symbol)
    {
        return symbol.GetAttributes().Any(IsObsoleteAttribute);
    }

    public static bool IsObsoleteAttribute(this AttributeData attributeData)
    {
        return IsObsoleteAttribute(attributeData.AttributeClass ?? InvalidAttributeClass());
    }

    private static INamedTypeSymbol InvalidAttributeClass()
    {
        throw new InvalidOperationException("AttributeClass is null");
    }

    private static bool IsObsoleteAttribute(INamedTypeSymbol symbol)
    {
        return MatchesType(type: ObsoleteType, symbol: symbol);
    }

    public static bool IsDescriptionAttribute(this AttributeData attributeData)
    {
        return IsDescriptionAttribute(attributeData.AttributeClass ?? InvalidAttributeClass());
    }

    private static bool IsDescriptionAttribute(INamedTypeSymbol symbol)
    {
        return MatchesType(type: DescriptionType, symbol: symbol);
    }

    private static bool MatchesType(Type type, INamedTypeSymbol symbol)
    {
        return StringComparer.Ordinal.Equals(x: symbol.Name, y: type.Name)
            && StringComparer.Ordinal.Equals(
                symbol.ContainingNamespace.ToDisplayString(),
                y: type.Namespace
            );
    }
}
