# Owned .NET Packages

[Back to Global Instructions Index](index.md)

> Load when: any `.csproj`, `.sln`, or `.slnx` file is present, or when researching a package that begins with `Credfeto.` or `FunFair.`.

## Rule

**Never decompile or reverse-engineer source code for packages listed here.** These packages are owned by the same organisation. Read the source directly from the linked GitHub repository instead.

## Package Registry

| NuGet Package | GitHub Repository | Notes |
| --- | --- | --- |
| `Credfeto.Changelog.Cmd` | [credfeto-changlog-manager](https://github.com/credfeto/credfeto-changlog-manager) | `dotnet changelog` CLI tool: repo name is misspelled on GitHub (`changlog`, not `changelog`) |
| `Credfeto.Database.Source.Generation` | [credfeto-database-source-generator](https://github.com/credfeto/credfeto-database-source-generator) | Source generator for database access |
| `Credfeto.Enum.Source.Generator` | [credfeto-enum-source-generation](https://github.com/credfeto/credfeto-enum-source-generation) | Source generator for enum helpers |
| `Credfeto.Exceptions.SourceGenerator` | [credfeto-exception-source-generator](https://github.com/credfeto/credfeto-exception-source-generator) | Source generator for exception constructors |
| `FunFair.BuildCheck` | [funfair-build-check](https://github.com/funfair-tech/funfair-build-check) | `dotnet buildcheck` CLI tool |
| `FunFair.CodeAnalysis` | [funfair-server-code-analysis](https://github.com/funfair-tech/funfair-server-code-analysis) | Roslyn code analysis rules for FunFair projects (diagnostics use the `FFS####` prefix, e.g. `FFS0050`) |
| `FunFair.Test.Common`, `FunFair.Test.Infrastructure`, `FunFair.Test.Source.Generator` | [funfair-server-test](https://github.com/funfair-tech/funfair-server-test) | Shared test base classes, HTTP mocking helpers, and source generators for test projects |

## General Guidance

- If you encounter a `Credfeto.*` or `FunFair.*` package **not listed above**, add it to this table before proceeding; do not decompile. Ask the user for the correct GitHub repository URL if unknown.
- These packages may be referenced as analyzers (`PrivateAssets="All"`), runtime dependencies, or dotnet tools; the rule applies regardless of reference type.
- When source is needed to understand behaviour (e.g. to fix a bug or write a test), clone or browse the linked repository directly.
