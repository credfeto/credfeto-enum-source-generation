# .NET Instructions

[Back to Global Instructions Index](index.md)

These rules apply to all .NET solutions derived from this template.

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

## Test Dependencies

- All test projects must reference the latest release version of `FunFair.Test.Common`.
- All test projects must import the latest release version of the `FunFair.Test.Source.Generator` source generator package.
- Test fixture classes must derive from `FunFair.Test.Common.TestBase`.
