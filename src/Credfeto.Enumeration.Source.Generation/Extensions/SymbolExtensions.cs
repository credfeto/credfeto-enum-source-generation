using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

public static class SymbolExtensions
{
    private static readonly Type ObsoleteType = typeof(ObsoleteAttribute);

    public static bool IsObsolete(this ISymbol symbol)
    {
        return symbol.GetAttributes()
                     .Any(x => IsObsoleteAttribute(x.AttributeClass!));
    }

    private static bool IsObsoleteAttribute(INamedTypeSymbol symbol)
    {
        return symbol.Name == ObsoleteType.Name && symbol.ContainingNamespace.ToDisplayString() == ObsoleteType.Namespace;
    }
}