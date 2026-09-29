# .NET Examples

[Back to .NET Instructions](dotnet.instructions.md) | [Back to Global Instructions Index](index.md)

Load this file when writing DI setup tests deriving from `DependencyInjectionTestsBase`, or source-generated logging methods that need an `IsEnabled` guard.

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

## Source-Generated Logging with Guards

Guard expensive log arguments inside the logging extensions class, with a `public` wrapper that checks `IsEnabled` and a `private` `[LoggerMessage]` method of the same name:

```csharp
// ❌ Wrong: ToString() and Successful() run even when Information is disabled (CA1873)
this._logger.LogDecryptCompleted(address.ToString(), timer.Successful().TotalMilliseconds);

// ❌ Wrong: the guard sits in business logic
if (this._logger.IsEnabled(LogLevel.Information))
{
    this._logger.LogDecryptCompleted(address.ToString(), timer.Successful().TotalMilliseconds);
}
```

```csharp
// ✅ Correct: the caller logs unconditionally
this._logger.LogDecryptCompleted(address, timer);
```

```csharp
// ✅ Correct: the guard lives in the logging extensions class
namespace MyApp.Wallets.LoggingExtensions;

internal static partial class WalletDecryptorLoggingExtensions
{
    public static void LogDecryptCompleted(this ILogger<WalletDecryptor> logger, AccountAddress address, SimpleExecutionTimer timer)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogDecryptCompleted(address.ToString(), timer.Successful().TotalMilliseconds);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Decrypted wallet {Address} in {ElapsedMilliseconds} ms")]
    private static partial void LogDecryptCompleted(this ILogger<WalletDecryptor> logger, string address, double elapsedMilliseconds);
}
```
