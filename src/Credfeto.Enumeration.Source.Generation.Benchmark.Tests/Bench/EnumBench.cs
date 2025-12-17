using System.Diagnostics.CodeAnalysis;
using System.Net;
using BenchmarkDotNet.Attributes;
using Credfeto.Enumeration.Source.Generation.Generics;
using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation.Benchmark.Tests.Bench;

[SimpleJob]
[MemoryDiagnoser(false)]
[SuppressMessage(category: "codecracker.CSharp", checkId: "CC0091:MarkMembersAsStatic", Justification = "Benchmark")]
[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0012: Make sealed static or abstract",
    Justification = "Benchmark"
)]
public class EnumBench
{
    [Benchmark]
    public string GetNameToString()
    {
        return ExampleEnumValues.ONE.GetNameToString();
    }

    [Benchmark]
    public string GetNameReflection()
    {
        return ExampleEnumValues.ONE.GetNameReflection();
    }

    [Benchmark]
    public string GetNameCachedReflection()
    {
        return ExampleEnumValues.ONE.GetNameReflectionCached();
    }

    [Benchmark]
    public string GetName2CodeGenerated()
    {
        return HttpStatusCode.Accepted.GetName();
    }

    [Benchmark]
    public string GetName2ToString()
    {
        return HttpStatusCode.Accepted.GetNameToString();
    }

    [Benchmark]
    public string GetName2Reflection()
    {
        return HttpStatusCode.Accepted.GetNameReflection();
    }

    [Benchmark]
    public string GetName2CachedReflection()
    {
        return HttpStatusCode.Accepted.GetNameReflectionCached();
    }

    [Benchmark]
    public string GetNameCodeGenerated()
    {
        return ExampleEnumValues.ONE.GetName();
    }

    [Benchmark]
    public string GetDescriptionReflection()
    {
        return ExampleEnumValues.ONE.GetDescriptionReflection();
    }

    [Benchmark]
    public string GetDescriptionCachedReflection()
    {
        return ExampleEnumValues.ONE.GetDescriptionReflectionCached();
    }

    [Benchmark]
    public string GetDescriptionCodeGenerated()
    {
        return ExampleEnumValues.ONE.GetDescription();
    }

    [Benchmark]
    public bool IsDefinedCodeReflection()
    {
        return ExampleEnumValues.ONE.IsDefinedReflection();
    }

    [Benchmark]
    public bool IsDefinedCodeReflectionCached()
    {
        return ExampleEnumValues.ONE.IsDefinedReflectionCached();
    }

    [Benchmark]
    public bool IsDefinedCodeGenerated()
    {
        return ExampleEnumValues.ONE.IsDefined();
    }
}
