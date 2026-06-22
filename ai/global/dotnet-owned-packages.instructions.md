# Owned .NET Packages

[Back to Global Instructions Index](index.md)

> Load when: any `.csproj`, `.sln`, or `.slnx` file is present, or when researching a package that begins with `Credfeto.` or `FunFair.`.

## Rule

**Never decompile or reverse-engineer source code for packages listed here.** These packages are owned by the same organisation. Read the source directly from the linked GitHub repository instead.

## Package Registry

| NuGet Package | GitHub Repository | Notes |
| --- | --- | --- |
| `Credfeto.Database.Source.Generation` | [credfeto-database-source-generator](https://github.com/credfeto/credfeto-database-source-generator) | Source generator for database access |
| `Credfeto.Enum.Source.Generator` | [credfeto-enum-source-generation](https://github.com/credfeto/credfeto-enum-source-generation) | Source generator for enum helpers |
| `Credfeto.Exceptions.SourceGenerator` | [credfeto-exception-source-generator](https://github.com/credfeto/credfeto-exception-source-generator) | Source generator for exception constructors |
| `Credfeto.Changelog.Cmd` | [credfeto-changelog-cmd](https://github.com/credfeto/credfeto-changelog-cmd) | `dotnet changelog` CLI tool |
| `FunFair.Test.Common` | [funfair-test-common](https://github.com/funfair-tech/funfair-test-common) | Shared test base classes and helpers |
| `FunFair.Test.Source.Generator` | [funfair-test-source-generator](https://github.com/funfair-tech/funfair-test-source-generator) | Source generator for test projects |
| `FunFair.BuildCheck` | [funfair-build-check](https://github.com/funfair-tech/funfair-build-check) | `dotnet buildcheck` CLI tool |

## General Guidance

- If you encounter a `Credfeto.*` or `FunFair.*` package **not listed above**, add it to this table before proceeding — do not decompile. Ask the user for the correct GitHub repository URL if unknown.
- These packages may be referenced as analyzers (`PrivateAssets="All"`), runtime dependencies, or dotnet tools — the rule applies regardless of reference type.
- When source is needed to understand behaviour (e.g. to fix a bug or write a test), clone or browse the linked repository directly.
