using System;

namespace Credfeto.Enumeration.Source.Generation.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class EnumTextAttribute : Attribute
{
    public EnumTextAttribute(Type @enum)
    {
        this.Enum = @enum;
    }

    public Type Enum { get; }
}