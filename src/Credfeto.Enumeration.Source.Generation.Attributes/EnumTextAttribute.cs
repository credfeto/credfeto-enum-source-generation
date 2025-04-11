using System;

namespace Credfeto.Enumeration.Source.Generation.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class EnumTextAttribute : Attribute
{
    public EnumTextAttribute(Type enumType)
    {
        this.Enum = ValidateEnumType(enumType);
    }

    public Type Enum { get; }

    private static Type ValidateEnumType(Type enumType)
    {
        return enumType.IsEnum
            ? enumType
            : NotAnEnum(enumType);
    }

    private static Type NotAnEnum(Type enumType)
    {
        throw new ArgumentException($"The type ({enumType.FullName}) must be an enum.", nameof(enumType));
    }
}