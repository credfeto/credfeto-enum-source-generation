using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Benchmark.Tests.Bench;

[SimpleJob]
[MemoryDiagnoser(false)]
[SuppressMessage(category: "codecracker.CSharp", checkId: "CC0091:MarkMembersAsStatic", Justification = "Benchmark")]
[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0012: Make sealed static or abstract",
    Justification = "Benchmark"
)]
public class EqualityBench
{
    private static readonly IReadOnlyList<IFieldSymbol> EmptyMembers = [];

    private static readonly EnumGeneration EnumA = new(
        accessType: AccessType.PUBLIC,
        name: "Status",
        @namespace: "TestNs",
        members: EmptyMembers,
        location: Location.None,
        options: new GenerationOptions(hasDoesNotReturnAttribute: true, supportsUnreachableException: true)
    );

    private static readonly EnumGeneration EnumB = new(
        accessType: AccessType.PUBLIC,
        name: "Status",
        @namespace: "TestNs",
        members: EmptyMembers,
        location: Location.None,
        options: new GenerationOptions(hasDoesNotReturnAttribute: true, supportsUnreachableException: true)
    );

    private static readonly ClassEnumGeneration ClassA = new(
        accessType: AccessType.PUBLIC,
        name: "EnumText",
        @namespace: "TestNs",
        enums: [EnumA],
        location: Location.None
    );

    private static readonly ClassEnumGeneration ClassB = new(
        accessType: AccessType.PUBLIC,
        name: "EnumText",
        @namespace: "TestNs",
        enums: [EnumB],
        location: Location.None
    );

    [Benchmark]
    public int EnumGenerationGetHashCode()
    {
        return EnumA.GetHashCode();
    }

    [Benchmark]
    public bool EnumGenerationEquals()
    {
        return EnumA.Equals(EnumB);
    }

    [Benchmark]
    public int ClassEnumGenerationGetHashCode()
    {
        return ClassA.GetHashCode();
    }

    [Benchmark]
    public bool ClassEnumGenerationEquals()
    {
        return ClassA.Equals(ClassB);
    }
}
