# .NET Examples

[Back to .NET Instructions](dotnet.instructions.md) | [Back to Global Instructions Index](index.md)

Load this file when writing DI setup tests deriving from `DependencyInjectionTestsBase`.

## Registering Mocked Services

Use `.AddMockedService<T>()` instead of concrete inner classes or `Substitute.For<T>()`:

```csharp
// ✅ Correct
private static IServiceCollection Configure(IServiceCollection services)
{
    return services.AddMyModule()
                   .AddMockedService<IFoo>()
                   .AddMockedService<IBar>();
}
```

## Registering Mocked IOptions\<T\>

Use `.AddMockedService<IOptions<TOptions>>(static o => o.Value.Returns(new TOptions()))`:

```csharp
// ✅ Correct
.AddMockedService<IOptions<MyOptions>>(static o => o.Value.Returns(new MyOptions()))

// ❌ Wrong
.AddSingleton<IOptions<MyOptions>>(Options.Create(new MyOptions()))
```
