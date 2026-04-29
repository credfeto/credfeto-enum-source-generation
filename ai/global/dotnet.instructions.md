# .NET Instructions

[Back to Global Instructions Index](index.md)

These rules apply to all .NET solutions derived from this template.

## Build and Test Before Commit (MANDATORY)

- Before every commit, run:

  ```bash
  dotnet build
  dotnet test
  ```

- If `dotnet build` fails, **do not commit**. Fix all build errors first.
- If `dotnet test` fails, **do not commit**. Fix all failing tests first.
- If build or test errors cannot be resolved, stop and ask the user for guidance.

## Asynchronous Code

See [code-quality.instructions.md](code-quality.instructions.md) for the general async rules (prefer async over sync, never block, propagate async through the call stack). .NET-specific additions:

- Prefer `ValueTask` and `ValueTask<T>` over `Task` and `Task<T>` wherever possible — they avoid heap allocations in the common synchronous-completion path.
- Only use `Task`/`Task<T>` where `ValueTask` is not supported or where the method is known to always complete asynchronously.

## Cancellation

- All async methods must accept and pass down a `CancellationToken` from the caller wherever the API supports it.
- Never create a new `CancellationToken` internally when one has been provided by the caller, unless there is an explicit and documented reason (e.g. combining the caller's token with a timeout using `CancellationTokenSource.CreateLinkedTokenSource`).
- Prefer overloads that accept a `CancellationToken` over those that do not.
- Do not pass `CancellationToken.None` unless there is an explicit and documented reason why cancellation must be suppressed.

## Project and Solution Structure

- All projects must be added to the solution file (`.slnx` or `.sln`).
- All projects must pass the latest release of the `FunFair.BuildCheck` dotnet tool before committing.

### Running FunFair.BuildCheck

Run from the solution root:

```bash
dotnet buildcheck
```

For available options:

```bash
dotnet buildcheck --help
```

## Test Assembly Naming

Test projects must follow a consistent naming convention relative to the assembly under test:

| Test type | Assembly name pattern |
| --------- | --------------------- |
| Unit tests | `<AssemblyName>.Tests` |
| Integration tests | `<AssemblyName>.Integration.Tests` |
| Benchmarks | `<AssemblyName>.Benchmark.Tests` |

For example, for an assembly `This.Test.Example`:

- Unit tests → `This.Test.Example.Tests`
- Integration tests → `This.Test.Example.Integration.Tests`
- Benchmarks → `This.Test.Example.Benchmark.Tests`

## Test Dependencies

- All test projects must reference the latest release version of `FunFair.Test.Common`.
- All test projects must import the latest release version of the `FunFair.Test.Source.Generator` source generator package.
- Test fixture classes must derive from `FunFair.Test.Common.TestBase`.

## NSubstitute and FunFair.Test.Common Patterns

When writing tests with `FunFair.Test.Common.TestBase`, use the helper methods provided by the base class instead of calling NSubstitute directly:

| Instead of | Use |
| ---------- | --- |
| `Substitute.For<IMyInterface>()` | `GetSubstitute<IMyInterface>()` (static — no `this.`) |
| `Substitute.For<ILogger<MyClass>>()` | `this.GetTypedLogger<MyClass>()` (instance — requires `this.`) |

### Rules

- Never call `Substitute.For<T>()` directly in test classes that derive from `TestBase` or `DependencyInjectionTestsBase`.
- Use `GetSubstitute<T>()` (no `this.` prefix — it is a static method) for all interface/class mocks.
- Use `this.GetTypedLogger<T>()` (with `this.` prefix — it is an instance method) for `ILogger<T>` mocks.
- Remove unused `using NSubstitute;` statements from files where all `Substitute.For<>()` calls have been replaced.

## DI Setup Test Patterns

When writing DI setup tests that derive from `DependencyInjectionTestsBase`, use `AddMockedService<T>()` from `FunFair.Test.Common` to register mocks.

### Registering mocked services

Use `.AddMockedService<T>()` instead of creating concrete inner classes or calling `Substitute.For<T>()` manually:

```csharp
// ✅ Correct
private static IServiceCollection Configure(IServiceCollection services)
{
    return services.AddMyModule()
                   .AddMockedService<IFoo>()
                   .AddMockedService<IBar>();
}
```

### Registering mocked IOptions\<T\>

Use `.AddMockedService<IOptions<TOptions>>(static o => o.Value.Returns(new TOptions()))` instead of `Options.Create(...)`:

```csharp
// ✅ Correct
.AddMockedService<IOptions<MyOptions>>(static o => o.Value.Returns(new MyOptions()))

// ❌ Wrong
.AddSingleton<IOptions<MyOptions>>(Options.Create(new MyOptions()))
```

- Never create concrete no-op inner classes to satisfy DI mocking in setup tests.
- `GetSubstitute<T>()` is safe to call from `static` Configure methods (it is a static method).

## Source File Organisation

- **One type per file**: every C# source file must contain exactly one type (class, record, struct, interface, or enum). Do not define multiple types in a single file.
- The file name must match the type name exactly (e.g. `FooBar.cs` for `class FooBar`).

## Debugger Diagnostics

- All `struct` types must have a `[DebuggerDisplay("...")]` attribute that shows their key fields.
- All value types (records declared with positional parameters or `record struct`) must have a `[DebuggerDisplay("...")]` attribute.
- All configuration/options classes (typically bound from `appsettings.json`) must have a `[DebuggerDisplay("...")]` attribute showing their key properties.
- The `DebuggerDisplay` format string should show the most useful identifying information — typically a name, identifier, or URL.

## Time Abstraction

- Use `System.TimeProvider` (built into .NET 8+) in production code for all time-related abstractions.
- **Never use** `Credfeto.Date.ICurrentTimeSource` or `FunFair.Common.Services.IDateTimeSource` — these are obsolete custom abstractions being removed.
- In tests, use `FakeTimeProvider` from the `Microsoft.Extensions.TimeProvider.Testing` NuGet package — never roll a custom time mock or fake.
- Whenever work touches code that uses `ICurrentTimeSource` or `IDateTimeSource`, migrate it to `TimeProvider`/`FakeTimeProvider` as part of that work.

## Warning Suppression

- **Never use `#pragma warning disable <ID>`** — inline suppression hides problems without explanation and is invisible in code review.
- If a warning genuinely cannot be fixed and must be suppressed, use `[SuppressMessage("category", "ID", Justification = "reason")]` at the narrowest possible scope (method or property, never assembly-level unless unavoidable).
- The `Justification` parameter is **mandatory** — a suppression without a justification must be treated as a build error.

## Warnings as Errors

- Every project must build with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in its project file or a shared `Directory.Build.props`.
- `<NoWarn>` and `<WarningsNotAsErrors>` must not be used to silence warnings — fix the code or use `[SuppressMessage]` with a justification instead. See [NuGet Vulnerability Suppression](#nuget-vulnerability-suppression) for the one narrow exception (per-advisory NuGet audit suppressions).

## NuGet Vulnerability Suppression

Accepted NuGet security advisories must be suppressed **at the project level** using the specific advisory URL, not suppressed globally in shared props.

### Required: project-level suppression

Suppress each accepted advisory in the affected `.csproj` file using `<NuGetAuditSuppress>`:

```xml
<ItemGroup>
  <!-- Reason: <brief explanation of why this advisory is accepted> -->
  <NuGetAuditSuppress Include="https://github.com/advisories/GHSA-xxxx-xxxx-xxxx" />
</ItemGroup>
```

### Prohibited: global suppression

Do **not** suppress NuGet vulnerability warnings globally in shared `.props` files or `Directory.Build.props`. The following patterns are **explicitly prohibited**:

```xml
<!-- PROHIBITED: broad suppression in shared props -->
<WarningsNotAsErrors>NU1901;NU1902;NU1903;NU1904</WarningsNotAsErrors>
<NoWarn>NU1901;NU1902;NU1903;NU1904</NoWarn>
```

### Rationale

Global suppression hides newly introduced advisories as packages are updated. Per-advisory project-level suppression ensures that:

- Only explicitly reviewed and accepted vulnerabilities are suppressed.
- New advisories on updated packages are not silently ignored.
- Each suppression is traceable to a specific advisory URL and kept in the project file where the affected package is referenced.
