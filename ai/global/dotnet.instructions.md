# .NET Instructions

[Back to Global Instructions Index](index.md)

These rules apply to all .NET solutions derived from this template.

## Build and Test Before Commit (MANDATORY)

- Before every commit, run:
  ```
  dotnet build
  dotnet test
  ```
- If `dotnet build` fails, **do not commit**. Fix all build errors first.
- If `dotnet test` fails, **do not commit**. Fix all failing tests first.
- If build or test errors cannot be resolved, stop and ask the user for guidance.

## Asynchronous Code

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

```
dotnet buildcheck
```

For available options:

```
dotnet buildcheck --help
```

## Test Assembly Naming

Test projects must follow a consistent naming convention relative to the assembly under test:

| Test type | Assembly name pattern |
|-----------|----------------------|
| Unit tests | `<AssemblyName>.Tests` |
| Integration tests | `<AssemblyName>.Integration.Tests` |
| Benchmarks | `<AssemblyName>.Benchmark.Tests` |

For example, for an assembly `This.Test.Example`:
- Unit tests → `This.Test.Example.Tests`
- Integration tests → `This.Test.Example.Integration.Tests`
- Benchmarks → `This.Test.Example.Benchmark.Tests`

## Build Before Push (MANDATORY)

Before committing changes to any .NET repository:

1. Run `dotnet build` from the solution root and verify it succeeds with no errors.
2. Run `dotnet test` if a test project exists and verify all tests pass.
3. Do NOT commit or push if the build or tests fail — fix the issues first.

Never push code that does not compile. If you cannot fix a build error, report it instead of pushing broken code.

## Test Dependencies

- All test projects must reference the latest release version of `FunFair.Test.Common`.
- All test projects must import the latest release version of the `FunFair.Test.Source.Generator` source generator package.
- Test fixture classes must derive from `FunFair.Test.Common.TestBase`.

## Source File Organisation

- **One type per file**: every C# source file must contain exactly one type (class, record, struct, interface, or enum). Do not define multiple types in a single file.
- The file name must match the type name exactly (e.g. `FooBar.cs` for `class FooBar`).

## Debugger Diagnostics

- All `struct` types must have a `[DebuggerDisplay("...")]` attribute that shows their key fields.
- All value types (records declared with positional parameters or `record struct`) must have a `[DebuggerDisplay("...")]` attribute.
- All configuration/options classes (typically bound from `appsettings.json`) must have a `[DebuggerDisplay("...")]` attribute showing their key properties.
- The `DebuggerDisplay` format string should show the most useful identifying information — typically a name, identifier, or URL.

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
