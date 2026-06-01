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

## Code Coverage (MANDATORY)

Testing uses **Microsoft.Testing.Platform** (not VSTest). To collect coverage, run **one unit test project at a time** — this gives a clear picture of how well each assembly is covered by its own tests.

**Two working invocation patterns** (the `cd` step is required for both):

Project form — target one test project directly:

```bash
cd {solution-src-dir}
dotnet test {AssemblyName}.Tests/{AssemblyName}.Tests.csproj \
  -c Release \
  -p:SolutionDir={solution-src-dir}/ \
  -- --coverage --coverage-output-format cobertura \
     --coverage-output {repo-root}/coverage/{AssemblyName}.coverage.cobertura.xml
```

Critical rules:

- **`cd` to the solution `src/` directory first** and use a **relative** project path — absolute paths trigger the legacy VSTest bridge, which MTP 2.0+ rejects on .NET 10 SDK.
- **`--` separator is required** — coverage flags must be passed to the test host, not the `dotnet test` CLI. Passing `--coverage` before `--` conflicts with xunit's argument parsing.
- **`-c Release`** — always run coverage in Release mode.
- **`-p:SolutionDir=`** — must be an absolute path ending with `/` so `UnitTests.props` is found via `$(SolutionDir)`.
- **`--coverage-output`** — use an absolute path pointing into the repo's `/coverage/` directory (gitignored). Name the file `{AssemblyName}.coverage.cobertura.xml`.

**Only run unit test projects (`<AssemblyName>.Tests`) for coverage.** Exclude:

- `<AssemblyName>.Integration.Tests` — integration tests inflate coverage numbers and test external dependencies, not isolated units.
- `<AssemblyName>.Benchmark.Tests` — benchmarks are not functionality tests and must never be included in coverage runs.

`UnitTests.props` **must** contain the coverage extension package. If it is missing, stop and demand it is added:

```xml
<PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="*" />
```

Do not install `coverlet.collector`, `coverlet.msbuild`, or any VSTest data collector — they do not work with Microsoft.Testing.Platform. Do not switch to `dotnet-coverage` or any wrapper tool. If coverage is still not collected, verify that `UnitTests.props` is imported by the test project and that `Microsoft.Testing.Extensions.CodeCoverage` is present.

## Coverage Reporting with reportgenerator

After collecting `.cobertura.xml` files, generate reports using `dotnet reportgenerator`.

**Always generate one report per assembly** — pass only that assembly's `.cobertura.xml` as input:

```bash
dotnet reportgenerator \
  -reports:{repo-root}/coverage/{AssemblyName}.coverage.cobertura.xml \
  -targetdir:{repo-root}/coverage/{AssemblyName} \
  -reporttypes:Html
```

**Do not pass multiple assemblies' `.cobertura.xml` files into a single `reportgenerator` run.** If assembly A references types from assembly B, running them together causes B's components to appear in A's coverage report, which falsely lowers A's measured coverage.

When a combined view is needed (e.g. for a summary dashboard), run each assembly individually first, then produce one additional combined report as a separate step:

```bash
dotnet reportgenerator \
  -reports:"{repo-root}/coverage/*.coverage.cobertura.xml" \
  -targetdir:{repo-root}/coverage/combined \
  -reporttypes:Html
```

The per-assembly reports remain the authoritative measure of test quality for each project.

### Specific Coverage Rules (MANDATORY)

- If a source generator is used then it is because we _WANT_ the source generated version. Don't turn it off to get 100% code coverage.

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

## Naming Conventions

| Identifier | Convention | Example |
| ---------- | ---------- | ------- |
| Private constants (`const`) | `UPPER_SNAKE_CASE` | `private const int MAX_RETRY_COUNT = 3;` |
| Enum members | `UPPER_SNAKE_CASE` | `NO_REFERRER_WHEN_DOWNGRADE` |
| Private instance fields | `_camelCase` | `private readonly string _emailAddress;` |
| Private static readonly fields | `PascalCase` | `private static readonly Regex HostRegex = ...;` |
| Public/internal constants (`const`) | `PascalCase` | `public const int MaximumStringLength = 255;` |
| Public/internal properties | `PascalCase` | `public int PageSize { get; }` |
| Public/internal methods | `PascalCase` | `public bool TryParse(...)` |
| Local variables | `camelCase` | `int retryCount = 0;` |
| Method parameters | `camelCase` | `void Method(int retryCount, ...)` |
| Interfaces | `IPascalCase` | `IHostedBackgroundService` |
| Type parameters (generics) | `TPascalCase` | `<TKey, TValue>` |

## String Comparison

- Prefer `StringComparer.<type>.Equals(x, y)` over `string.Equals(x, y, StringComparison.<type>)` — enforced by FFS0050.
- This applies to all `StringComparison` variants (`Ordinal`, `OrdinalIgnoreCase`, etc.).
- Do not use `StringComparison.InvariantCulture`, `StringComparison.InvariantCultureIgnoreCase`, `StringComparison.CurrentCulture`, or `StringComparison.CurrentCultureIgnoreCase` — enforced by FFS0045–FFS0048.

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
