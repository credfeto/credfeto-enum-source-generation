using System.Diagnostics.CodeAnalysis;
using System.Net;
using BenchmarkDotNet.Attributes;
using Credfeto.Enumeration.Source.Generation.Generics;
using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation.Benchmarks.Bench;

[SimpleJob]
[MemoryDiagnoser(false)]
[SuppressMessage(category: "FunFair.CodeAnalysis", checkId: "FFS0012: Make sealed static or abstract", Justification = "Benchmark")]
public class EnumBench : BenchBase
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
    public void GetName2CodeGenerated()
    {
        this.Test(HttpStatusCode.Accepted.GetName());
    }

    [Benchmark]
    public void GetName2ToString()
    {
        this.Test(HttpStatusCode.Accepted.GetNameToString());
    }

    [Benchmark]
    public void GetName2Reflection()
    {
        this.Test(HttpStatusCode.Accepted.GetNameReflection());
    }

    [Benchmark]
    public void GetName2CachedReflection()
    {
        this.Test(HttpStatusCode.Accepted.GetNameReflectionCached());
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

    [Benchmark]
    public void IsDefinedCodeReflection()
    {
        this.Test(ExampleEnumValues.ONE.IsDefinedReflection());
    }

    [Benchmark]
    public void IsDefinedCodeReflectionCached()
    {
        this.Test(ExampleEnumValues.ONE.IsDefinedReflectionCached());
    }

    [Benchmark]
    public void IsDefinedCodeGenerated()
    {
        this.Test(ExampleEnumValues.ONE.IsDefined());
    }
}
