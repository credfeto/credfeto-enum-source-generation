using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using Credfeto.Enumeration.Source.Generation.Benchmark.Tests.Bench;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Benchmark.Tests;

public sealed class EnumTests : LoggingTestBase
{
    public EnumTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void Run_Benchmarks()
    {
        (Summary _, AccumulationLogger logger) = Benchmark<EnumBench>();

        this.Output.WriteLine(logger.GetLog());
    }
}
