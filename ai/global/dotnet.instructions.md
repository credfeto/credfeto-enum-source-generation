# .NET Instructions

> Load when: any `.csproj`, `.sln`, or `.slnx` file is present.

[Back to Global Instructions Index](index.md)

## NuGet Configuration (MANDATORY)

- Never modify `nuget.config` — it is managed by the repo owner, not by AI.

## Running .NET Tools (MANDATORY)

- Always invoke dotnet tools via `dotnet <toolname>` (e.g. `dotnet changelog`, `dotnet buildcheck`).
- Never search for the tool binary, add it to `PATH`, or invoke it directly as `~/.dotnet/tools/toolname` or similar.

## Build and Test Before Commit (MANDATORY)

Run `dotnet build` and `dotnet test` before every commit — see [git.instructions.md](git.instructions.md#build-and-test-verification-mandatory-before-any-commit-or-push).

## Source-Generated Logging

- Prefer `LoggerMessage` source generators over runtime string-based logging — faster, allocation-free, and compile-time structured.
- Logging methods must be in a dedicated `internal static` class:
  - Placed in a `LoggingExtensions` sub-namespace relative to the class it serves.
  - Named `<ClassName>LoggingExtensions` (e.g. `FooLoggingExtensions` for `Foo`).

## Asynchronous Code

See [code-quality.instructions.md](code-quality.instructions.md) for general async rules. .NET-specific:

- Prefer `ValueTask`/`ValueTask<T>` over `Task`/`Task<T>` — avoids heap allocations on synchronous-completion paths.
- Only use `Task`/`Task<T>` where `ValueTask` is unsupported or the method always completes asynchronously.

## Cancellation

- All async methods must accept and pass down a `CancellationToken`.
- Never create a new `CancellationToken` when one has been provided, unless combining with a timeout via `CancellationTokenSource.CreateLinkedTokenSource`.
- Prefer overloads that accept a `CancellationToken`.
- Do not pass `CancellationToken.None` without explicit documented reason.

## Project and Solution Structure

- All projects must be added to the solution file (`.slnx` or `.sln`).
- All projects must pass `FunFair.BuildCheck` before committing: `dotnet buildcheck` (run from solution root; `dotnet buildcheck --help` for options).

## Test Assembly Naming

| Test type | Assembly name pattern |
| --------- | --------------------- |
| Unit tests | `<AssemblyName>.Tests` |
| Integration tests | `<AssemblyName>.Integration.Tests` |
| Benchmarks | `<AssemblyName>.Benchmark.Tests` |

## Test Dependencies

- All test projects must reference the latest release of `FunFair.Test.Common`.
- All test projects must import the latest release of `FunFair.Test.Source.Generator`.
- All test projects must include `<Import Project="$(SolutionDir)UnitTests.props" Condition="Exists('$(SolutionDir)UnitTests.props')" />`.
- Test fixture classes must derive from `FunFair.Test.Common.TestBase`.

## Benchmark Guidance

- For .NET benchmark implementation and threshold assertions, follow [performance.instructions.md](performance.instructions.md#benchmarks-and-optimisation).

## NSubstitute and FunFair.Test.Common Patterns

| Instead of | Use |
| ---------- | --- |
| `Substitute.For<IMyInterface>()` | `GetSubstitute<IMyInterface>()` (static — no `this.`) |
| `Substitute.For<ILogger<MyClass>>()` | `this.GetTypedLogger<MyClass>()` (instance — requires `this.`) |

- Never call `Substitute.For<T>()` in classes deriving from `TestBase` or `DependencyInjectionTestsBase`.
- Remove unused `using NSubstitute;` after replacing all `Substitute.For<>()` calls.

## DI Setup Test Patterns

Use `AddMockedService<T>()` in tests deriving from `DependencyInjectionTestsBase` — see [dotnet.examples.md](dotnet.examples.md) for `AddMockedService` and `IOptions` patterns.

- Never create concrete no-op inner classes to satisfy DI mocking.
- `GetSubstitute<T>()` is safe in `static` Configure methods.

## Source File Organisation

- One type per file — class, record, struct, interface, or enum.
- File name must match the type name exactly (e.g. `FooBar.cs` for `class FooBar`).

## Value Types (struct / record struct)

- Prefer `struct` or `record struct` over `class` for small, short-lived, immutable data — avoids heap allocations.
- Use `struct` when: the type is logically a value (not an identity), is generally small, and is frequently created/discarded.
- Prefer `readonly struct` or `readonly record struct` to enforce immutability and enable compiler optimisations.
- Avoid mutable structs — unexpected copy semantics cause subtle bugs.
- Do not use `struct` for types that need inheritance or will be boxed frequently.

## Debugger Diagnostics

- All value types (`struct`, `record struct`, records with positional parameters) must have `[DebuggerDisplay("...")]` showing key fields.
- All configuration/options classes must have `[DebuggerDisplay("...")]` showing key properties.

## Time Abstraction

- Use `System.TimeProvider` (.NET 8+) for all time abstractions.
- Never use `Credfeto.Date.ICurrentTimeSource` or `FunFair.Common.Services.IDateTimeSource` — these are obsolete.
- In tests, use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` — never roll a custom mock.
- Migrate any code touching `ICurrentTimeSource` or `IDateTimeSource` to `TimeProvider`/`FakeTimeProvider` as part of that work.

## Warning Suppression and Errors

- Every project must build with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Never use `#pragma warning disable <ID>`, `<NoWarn>`, `<WarningsNotAsErrors>`, or `[SuppressMessage]` without **explicit written permission from the repo owner**. Exception: per-advisory NuGet audit suppressions (see below).
- If a warning fires, fix the root cause. If the fix is non-obvious, raise a GitHub issue rather than suppressing the warning.
- Test projects are **not** exempt from this rule — suppressing warnings in test code is equally prohibited without explicit permission.

## NuGet Vulnerability Suppression

Suppress per-project using the advisory URL — never globally in shared `.props` files. Track each suppression in a GitHub issue.

```xml
<ItemGroup>
  <!-- Reason: <why accepted> -->
  <NuGetAuditSuppress Include="https://github.com/advisories/GHSA-xxxx-xxxx-xxxx" />
</ItemGroup>
```
