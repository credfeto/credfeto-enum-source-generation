using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Credfeto.Enumeration.Source.Generation.Models;

[GeneratedCode(tool: "Credfeto.Enumeration.Source.Generation.EnumGenerator", version: "1.0.0")]
public static class ExampleEnumValuesGeneratedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this ExampleEnumValues value)
    {
        return value switch
        {
            // ExampleEnumValues.ZERO => nameof(ExampleEnumValues.ZERO)
            ExampleEnumValues.ZERO => nameof(ExampleEnumValues.ZERO),
            // ExampleEnumValues.ONE => nameof(ExampleEnumValues.ONE)
            ExampleEnumValues.ONE => nameof(ExampleEnumValues.ONE),
            // ExampleEnumValues.THREE => nameof(ExampleEnumValues.THREE)
            ExampleEnumValues.THREE => nameof(ExampleEnumValues.THREE),
            _ => throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: "Unknown enum member")
        };
        
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this ExampleEnumValues value)
    {
        return value switch
        {
            ExampleEnumValues.ONE => "One \"1\"",
            ExampleEnumValues.THREE => "Two but one better!",
            _ => GetName(value)
        };
        
    }
    
}

