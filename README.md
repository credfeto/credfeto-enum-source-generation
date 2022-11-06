# credfeto-enum-source-generation

C# Source generator for enums

## Using

Add a reference to the Credfeto.Enumeration.Source.Generation package in each project.

```csproj
<ItemGroup>
  <PackageReference Include="Credfeto.Enumeration.Source.Generation" Version="0.0.2.3" PrivateAssets="All" />
</ItemGroup>
```

For each enum in the project, generates a class with the following extension methods:

* public static string GetName(this MyEnum value)
* public static string GetDescription(this MyEnum value)

Given an the example enum defined below:

```csharp
public enum ExampleEnumValues
{
    ZERO = 0,

    [Description("One \"1\"")]
    ONE = 1,

    SAME_AS_ONE = ONE,
}
```

To get the name and value of the enum values. In release mode this can be practically instant.

```csharp
 ExampleEnumValues.ONE.GetName(); // ONE
 ExampleEnumValues.ONE.GetDescription(); // One "1"
```

## Benchmarks

```
|                         Method |          Mean |      Error |     StdDev |        Median | Allocated |
|------------------------------- |--------------:|-----------:|-----------:|--------------:|----------:|
|              GetNameReflection |    42.7801 ns |  0.3769 ns |  0.3341 ns |    42.6507 ns |      24 B |
|        GetNameCachedReflection |    32.0044 ns |  0.4048 ns |  0.3787 ns |    31.9477 ns |      24 B |
|           GetNameCodeGenerated |     0.0000 ns |  0.0000 ns |  0.0000 ns |     0.0000 ns |         - |
|       GetDescriptionReflection | 1,434.1271 ns | 24.4345 ns | 23.9980 ns | 1,429.0309 ns |     264 B |
| GetDescriptionCachedReflection |    31.2730 ns |  0.6310 ns |  0.7512 ns |    30.9345 ns |      24 B |
|    GetDescriptionCodeGenerated |     0.0017 ns |  0.0049 ns |  0.0044 ns |     0.0000 ns |         - |

// * Warnings *
ZeroMeasurement
  EnumBench.GetNameCodeGenerated: Default        -> The method duration is indistinguishable from the empty method duration
  EnumBench.GetDescriptionCodeGenerated: Default -> The method duration is indistinguishable from the empty method duration
```

## Changelog

View [changelog](CHANGELOG.md)

