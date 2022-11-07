using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Credfeto.Enumeration.Source.Generation.Models;

[GeneratedCode(tool: "Credfeto.Enumeration.Source.Generation.EnumGenerator", version: "0.0.0.1-do-not-distribute")]
public static class ExampleEnumValuesGeneratedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this ExampleEnumValues value)
    {
        return value switch
        {
            // ExampleEnumValues.ZERO => 0
            ExampleEnumValues.ZERO => "ZERO",
            // ExampleEnumValues.ONE => 1
            ExampleEnumValues.ONE => "ONE",
            // ExampleEnumValues.THREE => 3
            ExampleEnumValues.THREE => "THREE",
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

