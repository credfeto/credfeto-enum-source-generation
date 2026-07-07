# .NET Instructions

> Load when: any `.csproj`, `.sln`, or `.slnx` file is present.

[Back to Global Instructions Index](index.md)

## NuGet Configuration (MANDATORY)

- Never modify `nuget.config` — it is managed by the repo owner, not by AI.

## Running .NET Tools (MANDATORY)

- Always invoke dotnet tools via `dotnet <toolname>` (e.g. `dotnet changelog`, `dotnet buildcheck`).
- Never search for the tool binary, add it to `PATH`, or invoke it directly as `~/.dotnet/tools/toolname` or similar.

## Pre-Work Repository Health Check (MANDATORY)

Before starting any work on an issue or PR in a .NET repository:

1. Find the solution file (prefer `*.slnx` over `*.sln`; look in the repo root and `src/`).
2. Run: `dotnet buildcheck -solution <solutionfilename>`
3. If it fails:
   - Fix all reported issues.
   - Verify with `dotnet build` and `dotnet test`.
   - Commit the fixes with a conventional commit message and push.
   - Only proceed with the original work once buildcheck passes cleanly.
4. If buildcheck still fails after all fixing attempts:
   - For an issue: add a comment and label it `Blocked` — do not start work.
   - For a PR: comment on the PR and label it `Blocked` — do not continue work.

## Build and Test Before Commit (MANDATORY)

Run `dotnet build` and `dotnet test` before every commit — see [git.instructions.md](git.instructions.md#build-and-test-verification-mandatory-before-any-commit-or-push).

## Identifying Test Projects (MANDATORY)

A project is a test project **only** if its assembly name ends with one of these suffixes:

| Suffix | Type |
| ------ | ---- |
| `.Tests` | Unit tests |
| `.Integration.Tests` | Integration tests |
| `.Benchmark.Tests` | Benchmarks |

**Rules that must never be broken:**

- **Never** use "contains 'Test'" in a project name as a heuristic — a project named `*.TestHarness`, `*.Tests.Mocks`, or `*.Tests.Common` is NOT a test project.
- **Never** run `dotnet test` or `dotnet run` on `*.Benchmark.Tests` projects — BenchmarkDotNet performs real measurements that take hours and spawn dozens of build processes. CI handles benchmarks; never run them manually.
- **Never** target a project with `dotnet test` if its csproj contains `<IsTestProject>false</IsTestProject>` or `<IsTestingPlatformApplication>false</IsTestingPlatformApplication>`.
- **Do not** rely on `OutputType` or the project SDK as a discriminator — with Microsoft.Testing.Platform, legitimate test projects also use `OutputType=Exe`, and some test projects use `Microsoft.NET.Sdk.Web`.
- **Always** verify `IsTestingPlatformApplication` in the csproj — this is the property `dotnet test` in .NET 10 uses for discovery, not `IsTestProject`. The naming convention and `IsTestingPlatformApplication` are the only reliable signals.

Test support libraries (e.g. `*.Tests.Mocks`, `*.Tests.Common`) exist to be referenced by test projects. They are **not** test projects themselves and must never be targeted for test runs.

### Setting Up a Test Support Library (MANDATORY)

When a project is a test support library (provides mocks, helpers, or base types for test projects) but is **not** itself a test runner, it must have all four of the following properties set explicitly:

```xml
<IsTestProject>false</IsTestProject>
<IsTestingPlatformApplication>false</IsTestingPlatformApplication>
<UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
<TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
```

It must also import `UnitTests.props` (required by `FunFair.BuildCheck` until [funfair-tech/funfair-build-check#417](https://github.com/funfair-tech/funfair-build-check/issues/417) is resolved):

```xml
<Import Project="$(SolutionDir)UnitTests.props" Condition="Exists('$(SolutionDir)UnitTests.props')" />
```

- `IsTestProject=false` — tells `FunFair.BuildCheck` this is not a test project; without it, buildcheck errors because a project referencing test packages that lacks this flag is expected to be a test runner.
- `IsTestingPlatformApplication=false` — overrides the implicit `true` set by `FunFair.Test.Common`, xunit, and similar packages; without it, `dotnet test` on .NET 10 attempts to run the project as an executable and fails because `OutputType=Library`.
- `UseMicrosoftTestingPlatformRunner=true` — required by `FunFair.BuildCheck` for any project that references test packages, even when `IsTestProject=false`.
- `TestingPlatformDotnetTestSupport=true` — required by `dotnet buildcheck` for any project that references test packages (e.g. `xunit.v3.extensibility.core`, `FunFair.Test.Common`), even when `IsTestProject=false`; without it, buildcheck reports `Should specify TestingPlatformDotnetTestSupport as true`.

These projects keep `OutputType=Library`.

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

**Only run unit test projects (`<AssemblyName>.Tests`) for coverage** — see [Identifying Test Projects](#identifying-test-projects-mandatory) for the authoritative definition. Exclude:

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

- If a source generator is used then it is because we **WANT** the source generated version. Don't turn it off to get 100% code coverage. Source-generated code (classes decorated with `[GeneratedCode]`) should be excluded from coverage measurements — it is considered tested by the generator's author, not by us.
- For methods whose success-path requires live infrastructure (database connections, network sockets, file handles), follow the [Infrastructure-Dependent Success Paths](code-quality.instructions.md#infrastructure-dependent-success-paths) guidance in `code-quality.instructions.md` — do not suppress PH2140 or add `[ExcludeFromCodeCoverage]`.

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

`FunFair.BuildCheck` is an org-owned package — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) for the GitHub source repository.

- All projects must be added to the solution file (`.slnx` or `.sln`).
- All projects must pass `FunFair.BuildCheck` before committing: `dotnet buildcheck` (run from solution root; `dotnet buildcheck --help` for options).
- If a build fails because `$(SolutionDir)` is undefined (e.g. imports guarded by `Exists('$(SolutionDir)...')` are silently skipped, leading to errors such as `NU5017`), fix it by ensuring the solution's `src/` directory has a `Directory.Build.props` that sets a fallback:

  ```xml
  <Project>
      <PropertyGroup>
          <SolutionDir Condition="'$(SolutionDir)' == ''">$(MSBuildThisFileDirectory)</SolutionDir>
      </PropertyGroup>
  </Project>
  ```

  This makes `$(SolutionDir)` resolve correctly in solution-less build contexts (e.g. BenchmarkDotNet host processes, `dotnet watch`, direct project builds) without changing any project files.

## Source Generator Test Projects

When writing unit tests that directly reference a source generator project via `<ProjectReference>` on .NET 10+, add the following MSBuild target to the test project's `.csproj`:

```xml
<Target Name="RemoveGeneratorNuGetCompileDependencies"
        AfterTargets="ResolveProjectReferences"
        BeforeTargets="ResolveAssemblyReferences">
  <ItemGroup>
    <_ResolvedProjectReferencePaths Remove="@(_ResolvedProjectReferencePaths)"
        Condition="'%(_ResolvedProjectReferencePaths.NuGetPackageId)' != '' And '%(_ResolvedProjectReferencePaths.IncludeRuntimeDependency)' == 'false'" />
  </ItemGroup>
</Target>
```

This prevents the generator's Roslyn NuGet dependencies (e.g. `System.Collections.Immutable 9.0` exported via `GetDependencyTargetPaths`) from appearing alongside the .NET 10 in-box versions in the test project's reference list, which would cause CS1685 ("predefined type defined in multiple assemblies").

## Test Assembly Naming

| Test type | Assembly name pattern |
| --------- | --------------------- |
| Unit tests | `<AssemblyName>.Tests` |
| Integration tests | `<AssemblyName>.Integration.Tests` |
| Benchmarks | `<AssemblyName>.Benchmark.Tests` |

## Test Dependencies

The `FunFair.Test.*` packages are org-owned — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) for the GitHub source repository.

- All test projects must reference the latest release of `FunFair.Test.Common`.
- All test projects must import the latest release of `FunFair.Test.Source.Generator`.
- All test projects must include `<Import Project="$(SolutionDir)UnitTests.props" Condition="Exists('$(SolutionDir)UnitTests.props')" />`.
- Test fixture classes must derive from `FunFair.Test.Common.TestBase` or one of its derivatives — see the table below for guidance.:

| Test type | Base class |
| --------- | ---------- |
| General unit tests | `TestBase` |
| Dependency injection registration tests | `DependencyInjectionTestsBase` |
| Validator tests | `ComplexValidatorTestBase` |
| Simple validator tests | `ValidatorTestBase` |
| Comparable object type tests | `ComparableObjectTestBase` |
| Comparable value type tests | `ComparableValueTestBase` |
| Equatable object type tests | `EquatableObjectTestBase` |
| Equatable value type tests | `EquatableValueTestBase` |
| Integration tests | `IntegrationTestBase` |
| General unit tests where we want logging | `LoggingTestBase` |
| Value type JSON converters | `JsonConverterStructTestBase` |
| Object type JSON converters | `JsonConverterTestBase` |
| Unit tests where we want to write temp files to disk and have them cleaned up | `LoggingFolderCleanupTestBase` |
| Tests on model binders | `ModelBinderTestsBase` |

## Benchmark Guidance

- For .NET benchmark implementation and threshold assertions, follow [performance.instructions.md](performance.instructions.md#benchmarks-and-optimisation).

## NSubstitute and FunFair.Test.Common Patterns

| Instead of | Use |
| ---------- | --- |
| `Substitute.For<IMyInterface>()` | `GetSubstitute<IMyInterface>()` (static — no `this.`) |
| `Substitute.For<ILogger<MyClass>>()` | `this.GetTypedLogger<MyClass>()` (instance — requires `this.`) |

- Never call `Substitute.For<T>()` in classes deriving from `TestBase` or `DependencyInjectionTestsBase`.
- Remove unused `using NSubstitute;` after replacing all `Substitute.For<>()` calls.
- All instance method calls on `TestBase` (including `GetTypedLogger<T>()`, `GetSubstitute<T>()`, `CancellationToken()`, and custom helpers) require explicit `this.` — IDE0009 is enforced as an error. Static helpers (e.g. `GetSubstitute<T>()`) must remain `static` to avoid this requirement.

## FunFair.Test.* — Prefer Library Code Over Custom Implementations (MANDATORY)

The `FunFair.Test.*` packages in this section are org-owned — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) for the GitHub source repository.

**Do not write code that FunFair.Test.* already provides.** Before implementing a custom test helper, check what `FunFair.Test.Common` and `FunFair.Test.Infrastructure` offer.

### FunFair.Test.Infrastructure

Add `FunFair.Test.Infrastructure` as a package reference when your tests need HTTP mocking beyond what `FunFair.Test.Common` provides. It supplies:

**`HttpClientFactoryExtensions`** (`FunFair.Test.Infrastructure.Extensions`):

Use `MockCreateClientWithResponse` to set up a named `IHttpClientFactory` substitute with a fixed response — do NOT hand-write `GetSubstitute<IHttpClientFactory>()` + `.Returns()` for simple cases:

```csharp
IHttpClientFactory factory = GetSubstitute<IHttpClientFactory>();
factory.MockCreateClientWithResponse(
    clientName: "MyClientName",
    httpStatusCode: HttpStatusCode.OK,
    responseMessage: """{"data":[]}"""
);
```

Overloads accept `string`, typed object (serialised to JSON), or `HttpStatusCode`-only. Use the typed overload when the response is a .NET object to avoid manual serialisation.

**Limitation**: `MockCreateClientWithResponse` sets up one fixed response per named client. For tests that need sequential different responses from the same client, use `NSubstitute`'s `.Returns()` with a delegate or a custom `HttpMessageHandler`.

### General rule

If `FunFair.Test.Common` or `FunFair.Test.Infrastructure` provides a helper for a pattern you are about to write from scratch, use the library version. Examples of patterns NOT to reinvent:

- Custom `ILogger<T>` mocks → `this.GetTypedLogger<T>()`
- Custom `TimeProvider` fakes → `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`
- Custom `IHttpClientFactory` setups → `MockCreateClientWithResponse`

## xunit Assertion Patterns

- `Assert.Single(collection)` returns the single element — capture it directly instead of asserting then indexing:

  ```csharp
  // WRONG
  Assert.Single(collection);
  var item = collection[0];

  // CORRECT
  var item = Assert.Single(collection);
  ```

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

`FunFair.CodeAnalysis` is an org-owned package that enforces FFS rules in this section — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) for the GitHub source repository.

- Prefer `StringComparer.<type>.Equals(x, y)` over `string.Equals(x, y, StringComparison.<type>)` — enforced by FFS0050.
- This applies to all `StringComparison` variants (`Ordinal`, `OrdinalIgnoreCase`, etc.).
- Do not use `StringComparison.InvariantCulture`, `StringComparison.InvariantCultureIgnoreCase`, `StringComparison.CurrentCulture`, or `StringComparison.CurrentCultureIgnoreCase` — enforced by FFS0045–FFS0048.

## Source File Organisation

- One type per file — class, record, struct, interface, or enum.
- File name must match the type name exactly (e.g. `FooBar.cs` for `class FooBar`).

## Data Types: Prefer Records over Classes

Use a positional `record` (or `readonly record struct`) instead of a hand-written data class wherever the type is a pure carrier of data with no behaviour.

```csharp
// WRONG — hand-written data class
public sealed class GlobalJsonInfo
{
    public GlobalJsonInfo(string? sdkVersion, string? rollForward, bool? allowPrerelease)
    {
        this.SdkVersion = sdkVersion;
        this.RollForward = rollForward;
        this.AllowPrerelease = allowPrerelease;
    }

    public string? SdkVersion { get; }
    public string? RollForward { get; }
    public bool? AllowPrerelease { get; }
}

// CORRECT — positional record
[DebuggerDisplay("SdkVersion={SdkVersion}, RollForward={RollForward}, AllowPrerelease={AllowPrerelease}")]
public sealed record GlobalJsonInfo(string? SdkVersion, string? RollForward, bool? AllowPrerelease);
```

- Always add `[DebuggerDisplay("...")]` showing all key properties (see [Debugger Diagnostics](#debugger-diagnostics)).
- If the type is a pure value (no identity semantics, small, immutable) prefer `readonly record struct` over `record class`.
- If the target framework does **not** support records (e.g. `netstandard2.0`), continue using a `class` or `struct`, but manually implement everything a record would provide: a constructor that sets all properties, read-only auto-properties, `Equals`, `GetHashCode`, `ToString`, and `IEquatable<T>`.

## Value Types (struct / record struct)

- Prefer `struct` or `record struct` over `class` for small, short-lived, immutable data — avoids heap allocations.
- Use `struct` when: the type is logically a value (not an identity), is generally small, and is frequently created/discarded.
- Prefer `readonly struct` or `readonly record struct` to enforce immutability and enable compiler optimisations.
- Avoid mutable structs — unexpected copy semantics cause subtle bugs.
- Do not use `struct` for types that need inheritance or will be boxed frequently.

## Exception Classes

`Credfeto.Exceptions.SourceGenerator` is an org-owned package — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) for the GitHub source repository.

Use `Credfeto.Exceptions.SourceGenerator` to define exception types — it generates all required constructors automatically.

1. Add the package to the project (analyzer only — not a runtime dependency):

   ```xml
   <PackageReference Include="Credfeto.Exceptions.SourceGenerator" Version="0.0.1.30" PrivateAssets="All" ExcludeAssets="runtime" />
   ```

2. Declare the exception as a `sealed partial class` with a `[Description]` attribute for the default message:

   ```csharp
   [Description("Default message")]
   public sealed partial class MyException : Exception;
   ```

- Always use the **latest stable release** of `Credfeto.Exceptions.SourceGenerator`.
- Never hand-write exception constructors when this generator is available.
- Apply the same rule to any project that already defines exceptions: add the package and convert existing hand-written exception classes to `partial`.

## Debugger Diagnostics

- All value types (`struct`, `record struct`, records with positional parameters) must have `[DebuggerDisplay("...")]` showing key fields.
- All configuration/options classes must have `[DebuggerDisplay("...")]` showing key properties.

## Time Abstraction

- Use `System.TimeProvider` (.NET 8+) for all time abstractions.
- Never use `Credfeto.Date.ICurrentTimeSource` or `FunFair.Common.Services.IDateTimeSource` — these are obsolete.
- In tests, use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` — never roll a custom mock.
- Migrate any code touching `ICurrentTimeSource` or `IDateTimeSource` to `TimeProvider`/`FakeTimeProvider` as part of that work.

### Test Date Values (MANDATORY)

Never use hardcoded literal dates (e.g. `new DateTime(2024, 1, 1)`) in tests. Use the `MockDateTimeSources` helpers instead:

| Scenario | Use |
| -------- | --- |
| A date in the past | `MockDateTimeSources.Past` |
| A date in the future | `MockDateTimeSources.Future` |
| A date that advances over time (use sparingly) | `MockDateTimeSources.AdvancingDateTimeUseWithCaution` |

- `MockDateTimeSources.AdvancingDateTimeUseWithCaution` advances the clock as the test runs — only use it when the test genuinely requires elapsed time. Prefer `Past` or `Future` for all other cases.

## Conditional Compilation and Dead Code

When all target frameworks listed in a project file are .NET 9 or later, framework version guards whose condition is unconditionally true become dead structure:

- `#if NET9_0_OR_GREATER`, `#if NET8_0_OR_GREATER`, `#if NET7_0_OR_GREATER`, `#if NET6_0_OR_GREATER`, and any earlier `OR_GREATER` variants are always true — remove the directive, keep the body, and delete the `#else` branch and fallback implementation.
- The corresponding negated guards (`#if !NET9_0_OR_GREATER`, etc.) are always false — remove the entire block including the body.
- Delete any source files that exist solely as pre-.NET 9 fallback implementations (e.g. files named `*.net6.cs` or `*SourceGenerated.net6.cs` containing a `new Regex(...)` fallback for the `[GeneratedRegex]` source generator).
- After removing the conditional blocks, verify the project still builds and all tests still pass — treat this as a separate commit from any feature or fix work.

## Warning Suppression and Errors

- Every project must build with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Never use `#pragma warning disable <ID>`, `<NoWarn>`, `<WarningsNotAsErrors>`, or `[SuppressMessage]` without **explicit written permission from the repo owner**. Exception: per-advisory NuGet audit suppressions (see below).
- If a warning fires, fix the root cause. If the fix is non-obvious, raise a GitHub issue rather than suppressing the warning.
- Test projects are **not** exempt from this rule — suppressing warnings in test code is equally prohibited without explicit permission.

**Exception — project-specific local instruction files:** A project's `ai/local/` instruction file may explicitly document approved suppressions for that repository. Approval must be granted via a PR comment from the repo owner — the local instruction file alone is not sufficient to grant permission. When a repo owner approves a suppression via a PR comment, the local instruction file must be updated in that same PR to document: the specific warning ID, the affected class of code, and the reasoning for the exception. Once documented in the local instruction file following an explicit PR comment approval, that entry satisfies the "explicit written permission from the repo owner" requirement for future suppressions of that warning ID in that class of code. Local instructions take precedence over this global rule per the `.ai-instructions` precedence hierarchy.

## NuGet Vulnerability Suppression

Suppress per-project using the advisory URL — never globally in shared `.props` files. Track each suppression in a GitHub issue.

```xml
<ItemGroup>
  <!-- Reason: <why accepted> -->
  <NuGetAuditSuppress Include="https://github.com/advisories/GHSA-xxxx-xxxx-xxxx" />
</ItemGroup>
```

## Publishing Executables (Trimming and AOT)

When working on a .NET project that produces a publishable executable (`OutputType=Exe` or `OutputType=WinExe`), follow these steps in order:

1. **Enable trimming first** — add `<PublishTrimmed>true</PublishTrimmed>` to the project file and verify the project builds without trim warnings or errors.
   - Fix all `IL2xxx` trim-analysis warnings before committing.
   - Replace reflection-based patterns with source-generated equivalents — for example, replace `JsonSerializer` usage with a `JsonSerializerContext` annotated with `[JsonSerializable]`.
   - Apply `[DynamicallyAccessedMembers]` only where reflection is genuinely unavoidable and cannot be replaced with a source generator.
   - Do not suppress trim warnings — treat them as blocking, consistent with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

2. **Enable AOT only after trimming is clean** — once `<PublishTrimmed>true</PublishTrimmed>` builds without warnings, replace it with `<PublishAot>true</PublishAot>` (AOT implies trimming; both properties do not need to be set simultaneously).
   - Fix all `IL3xxx` AOT-compatibility warnings.
   - Remove any runtime code generation: `Emit`, `DynamicMethod`, `Expression.Compile`, `CSharpCodeProvider`, etc.
   - Verify that every third-party package used by the executable has AOT-compatible code paths. Check for `IsAotCompatible=true` in the package metadata or a corresponding `[RequiresUnreferencedCode]` annotation indicating the incompatibility.
   - Do not suppress AOT warnings — treat them as blocking.

3. **If either step is blocked by an incompatible third-party dependency** — raise a GitHub issue in the current repository describing the incompatibility (package name, version, and the specific warning or error), then stop. Do not work around the incompatibility by suppressing warnings or downgrading the property.
