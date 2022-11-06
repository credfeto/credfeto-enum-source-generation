using BenchmarkDotNet.Attributes;
using Credfeto.Enumeration.Source.Generation.Generics;
using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation.Benchmarks;

[SimpleJob]
[MemoryDiagnoser(false)]
public abstract class EnumBench : BenchBase
{
    [Benchmark]
    public void GetNameToString()
    {
        this.Test(ExampleEnumValues.ONE.GetNameToString());
    }

    [Benchmark]
    public void GetNameReflection()
    {
        this.Test(ExampleEnumValues.ONE.GetNameReflection());
    }

    [Benchmark]
    public void GetNameCachedReflection()
    {
        this.Test(ExampleEnumValues.ONE.GetNameReflectionCached());
    }

    [Benchmark]
    public void GetNameCodeGenerated()
    {
        this.Test(ExampleEnumValues.ONE.GetName());
    }

    [Benchmark]
    public void GetDescriptionReflection()
    {
        this.Test(ExampleEnumValues.ONE.GetDescriptionReflection());
    }

    [Benchmark]
    public void GetDescriptionCachedReflection()
    {
        this.Test(ExampleEnumValues.ONE.GetDescriptionReflectionCached());
    }

    [Benchmark]
    public void GetDescriptionCodeGenerated()
    {
        this.Test(ExampleEnumValues.ONE.GetDescription());
    }
}