using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Credfeto.Enumeration.Source.Generation.Generics;

namespace Credfeto.Enumeration.Source.Generation.Benchmarks;

internal static class EnumWrappers
{
    [SuppressMessage(category: "ReSharper", checkId: "InvokeAsExtensionMethod", Justification = "This is a benchmark.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetNameReflectionCached<T>(this T value)
        where T : Enum
    {
        return EnumHelpers.GetName(value);
    }
}