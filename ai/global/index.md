# Global instructions

This is an index of global instructions that apply to all projects.

- Ensure consistency across all projects.
- This folder should be maintained ONLY in the `git@github.com:credfeto/cs-template.git` repository.
- Updates to this folder will be distributed using an external mechanism.
- Rule files are named `<category>.instructions.md`; code reference files are named `<category>.examples.md`.
- All files must maintain a backlink to this index.
- When adding a rule, check all other files for conflicts or duplication.

## Always Load

Read all of these before starting any task, regardless of language or context.

| File | Covers |
| --- | --- |
| [git.instructions.md](git.instructions.md) | Prerequisites, build/test verification, git identity/GPG, branching, commits, GitHub issues |
| [task-workflow.instructions.md](task-workflow.instructions.md) | Agent routing table, model selection, failure handling, issue/PR assignment, commit cadence, resuming work |
| [code-quality.instructions.md](code-quality.instructions.md) | Code coverage, tests, async, immutability, parameterised tests, refactoring |
| [documentation.instructions.md](documentation.instructions.md) | README, CHANGELOG conventions |
| [security.instructions.md](security.instructions.md) | No secrets in code, input validation, output sanitisation |
| [error-handling.instructions.md](error-handling.instructions.md) | Explicit error handling, propagation, safe surfacing |
| [logging.instructions.md](logging.instructions.md) | Structured logging, log levels, no PII/secrets |
| [packages.instructions.md](packages.instructions.md) | Secure versions, managed vs native, deprecated packages |

## Load When Applicable

Load these only when the work involves the relevant technology or context.

| File | Load When | Covers |
| --- | --- | --- |
| [dotnet.instructions.md](dotnet.instructions.md) | Any `.csproj`, `.sln`, or `.slnx` file is present | Build/test, solution structure, test patterns, ValueTask, CancellationToken, NuGet audit |
| [sql.instructions.md](sql.instructions.md) | Any `.sql` file or SQL project is present | SQL linting, local DB connection, performance optimisation |
| [shell-scripts.instructions.md](shell-scripts.instructions.md) | Any `.sh` file is present or shell script work is needed | Shebang, linting, output helper conventions (`die`/`success`/`info`) |
| [shell.firewall.instructions.md](shell.firewall.instructions.md) | Firewall rule management is needed | `firewall-cmd` rules, private network constants |
| [github-workflows.instructions.md](github-workflows.instructions.md) | Any `.github/workflows/*.yml` file is present or being created | Action policy, composite actions, step ordering, permissions, version pinning |
| [api.instructions.md](api.instructions.md) | An HTTP API is being created or modified | `.http` test file requirements |
| [performance.instructions.md](performance.instructions.md) | Performance-critical code is being written or optimised | Design principles, benchmarks, optimisation workflow |
| [agent-roles.instructions.md](agent-roles.instructions.md) | You are acting as a named agent (Orchestrator, Code Writer, Code Tester, etc.) | Detailed per-agent responsibilities and behaviour |
| [changelog.instructions.md](changelog.instructions.md) | You need to add or update a changelog entry, or you are the Changelog agent | Format, tooling (`dotnet changelog`), when to add entries, add/remove commands |
| [git-commits.instructions.md](git-commits.instructions.md) | You are about to commit, or you are the Committer agent | Commit size rules, empty commit check, push cadence, Conventional Commits format |
| [gitignore.instructions.md](gitignore.instructions.md) | Any `.gitignore` file is being created or modified | IDE file exclusions, root `.gitignore` ownership, consistency checks |
| [language.instructions.md](language.instructions.md) | Writing code, documentation, comments, or commit messages | UK English for docs/comments; platform convention for identifiers |

## Reference Files (Load on Demand)

These contain code examples only. Load them when actively writing or modifying the scripts they describe — not as part of routine rule-loading.

| File | Load When |
| --- | --- |
| [shell-scripts.examples.md](shell-scripts.examples.md) | Writing or modifying shell scripts — provides `die`, `success`, `info`, `is_ai_agent` implementations |
| [shell.firewall.examples.md](shell.firewall.examples.md) | Writing firewall scripts — provides `allow_ipv4`, `allow_ipv6`, `open_port_for_private_networks` implementations |
| [github-workflows.examples.md](github-workflows.examples.md) | Creating or scaffolding a local composite action — provides action template, explicit inputs, env-var validation step |
| [sql.examples.md](sql.examples.md) | Writing SQL or database connection scripts — provides `.database` file format, `sqlcmd` invocation, and `SET STATISTICS` baseline template |
| [dotnet.examples.md](dotnet.examples.md) | Writing .NET DI setup tests — provides `AddMockedService` and `IOptions` patterns |
| [git.examples.md](git.examples.md) | Implementing or debugging git identity checks — provides GPG validation script and template escalation command |
