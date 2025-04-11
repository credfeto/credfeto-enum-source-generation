using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Credfeto.Enumeration.Source.Generation.Benchmarks;

public abstract class BenchBase
{
    [SuppressMessage(
        category: "codecracker.CSharp",
        checkId: "CC0091:MarkMembersAsStatic",
        Justification = "Benchmark"
    )]
    [SuppressMessage(
        category: "Microsoft.Performance",
        checkId: "CA1822:Mark methods static",
        Justification = "Benchmark"
    )]
    [SuppressMessage(
        category: "ReSharper",
        checkId: "UnusedParameter.Global",
        Justification = "Simplifies benchmarks"
    )]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Test<T>(T value)
    {
        // Doesn't do anything
    }
}
