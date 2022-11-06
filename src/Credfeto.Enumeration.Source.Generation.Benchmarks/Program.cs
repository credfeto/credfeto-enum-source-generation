using BenchmarkDotNet.Running;

namespace Credfeto.Enumeration.Source.Generation.Benchmarks;

public static class Program
{
    private static void Main()
    {
        RunEnumBenchmarks();
    }

    private static void RunEnumBenchmarks()
    {
        BenchmarkRunner.Run<EnumBench>();
    }
}