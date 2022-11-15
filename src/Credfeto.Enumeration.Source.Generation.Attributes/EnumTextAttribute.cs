using System;

namespace Credfeto.Enumeration.Source.Generation.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class EnumTextAttribute : Attribute
{
    public EnumTextAttribute(Type enumType)
    {
        this.Enum = enumType;

        if (!enumType.IsEnum)
        {
            throw new ArgumentException(message: "The type must be an enum.", nameof(enumType));
        }
    }

    public Type Enum { get; }
}