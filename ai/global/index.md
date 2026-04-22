# Global instructions

This is an index of global instructions that apply to all projects.
- Ensure consistency across all projects.
- This file should be considered an index of global instructions.
- Each file other than this one should be named in the format `<category>.instructions.md` Where `<category>` is the category of the file and all related rules should be listed there.
- `<category>.instructions.md` files should be placed in this directory.
- `<category>.instructions.md` files should maintain a backlink to this file.
- This folder should be maintained ONLY in the git@github.com:credfeto/cs-template.git repository.
- Updates to this folder will be distributed to using an external mechanism.

## Instruction Files

| File | Description |
|------|-------------|
| [git.instructions.md](git.instructions.md) | Language/runtime prerequisites, build and test verification before commit/push, GitHub issue management, branching strategy, branch naming, commit size, commit message format (Conventional Commits, prompt in body) |
| [documentation.instructions.md](documentation.instructions.md) | README, CHANGELOG (KeepAChangeLog, `Credfeto.Changelog.Cmd`), and docs folder conventions |
| [code-quality.instructions.md](code-quality.instructions.md) | 100% code coverage, pre-commit test requirements, dead code removal, immutability, parameterised tests, test quality, refactoring, compile-time configuration testing |
| [packages.instructions.md](packages.instructions.md) | Secure package versions, managed vs native libraries, avoiding deprecated or obsolete packages |
| [api.instructions.md](api.instructions.md) | `.http` test files for exposed API endpoints |
| [performance.instructions.md](performance.instructions.md) | Performance considerations: speed as priority, reducing memory allocations |
| [sql.instructions.md](sql.instructions.md) | SQL linting requirements before every commit |
| [gitignore.instructions.md](gitignore.instructions.md) | `.gitignore` ownership, additional ignore files, and consistency checks |
| [language.instructions.md](language.instructions.md) | UK English for documentation and comments; platform convention for code identifiers |
| [security.instructions.md](security.instructions.md) | No secrets in code, input validation, output sanitisation, threat modelling, vulnerability scanning |
| [error-handling.instructions.md](error-handling.instructions.md) | Explicit error handling, no swallowed exceptions, propagation and safe surfacing of errors |
| [logging.instructions.md](logging.instructions.md) | Structured logging, log levels, what to log and what not to log (no PII, no secrets) |
| [dotnet.instructions.md](dotnet.instructions.md) | .NET-specific: build and test before commit (`dotnet build`/`dotnet test`), solution structure, test assembly naming, FunFair.Test.Common, FunFair.BuildCheck, project-level NuGetAuditSuppress (no global NU19xx suppression) |
| [task-workflow.instructions.md](task-workflow.instructions.md) | Issue/PR assignment, splitting large tasks into sub-issues worked one at a time, file status tracking, commit/push cadence, multi-agent routing (Code Writer → Code Tester → Code Reviewer → Changelog → Committer), resuming interrupted work |
 