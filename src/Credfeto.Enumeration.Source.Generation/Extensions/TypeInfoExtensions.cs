using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

public static class TypeInfoExtensions
{
    public static bool IsString(in this TypeInfo typeInfo)
    {
        return typeInfo.Type?.SpecialType == SpecialType.System_String;
    }

    public static bool IsEnum(in this TypeInfo typeInfo)
    {
        return typeInfo.Type is INamedTypeSymbol { EnumUnderlyingType: not null };
    }
}