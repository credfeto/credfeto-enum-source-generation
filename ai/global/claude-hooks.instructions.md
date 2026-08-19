# Claude Code Hook Interaction Instructions

[Back to Global Instructions Index](index.md)

This template's development containers (and any interactive session with the hooks installed via
`install-claude-hooks`) run a fixed set of Claude Code `PreToolUse` hooks against every Bash tool
call. This file covers how to interpret a hook **denial** correctly; for how to background and
poll long-running commands once a call has actually been accepted, see
[Background Tasks and Monitor Tool](task-workflow.instructions.md#background-tasks-and-monitor-tool-mandatory)
and [Never Truncate Test/Commit Commands](task-workflow.instructions.md#never-truncate-testcommit-commands-mandatory).

## A Denial Means the Command Never Ran (MANDATORY)

A `PreToolUse` hook denial (a tool result explaining the call was blocked) means the command
**never started**. There is nothing in flight and nothing will ever notify you about it later.

- Fix the specific thing the denial names and retry **immediately, in the same turn**.
- Never end a turn saying you are "waiting for it to finish" or "waiting for a completion
  notification" for a denied command. This matters most acutely in a single-shot session: there
  is no later turn for that notification to land in, so uncommitted work is silently abandoned,
  but the same misread is just as wrong in an interactive session with turns to spare.

## Read the Denial's Stated Reason Literally (MANDATORY)

What "the specific thing" above means in practice:

- `git commands must use "git -C <dir>" format` means add `-C <dir>` to that git invocation, not a
  general git problem.
- `git commit must run with run_in_background: true` means add that tool parameter, not switch to
  a different commit approach.

Fixing one hook's violation at a time and retrying can trigger a second hook's denial on the same
call, so satisfy every applicable rule in the one call that is retried rather than discovering them
one by one. The most common case is a command that must satisfy both a git-invocation-shape hook
and a must-be-backgrounded hook at once: `git -C <dir> commit -m "..."` invoked with
`run_in_background: true` set on that same tool call.

## Prefer the Tool's Own Backgrounding Parameter (MANDATORY)

Use the tool's own `run_in_background: true` parameter, never shell-level backgrounding (`&`,
`nohup ... &`, `disown`). Shell-level backgrounding is blocked outright by
`enforce-background-for-long-running-commands` (see below) and produces the same
denial-misread-as-in-flight failure described above.

## Reference: Installed Hook Set

The exact hook set installed at `$HOME/.claude/hooks` (from `install-claude-hooks`) at the time
this file was written:

| Hook | Blocks | Why |
| --- | --- | --- |
| `reject-obfuscated-commands` | Any Bash command not built from plain, obviously-spelled command words (indirect execution, sub-shells, wrapper-flag smuggling) | Text/regex scanning for banned patterns is an arms race that never converges against a determined bypass attempt; this hook parses with a real shell parser and applies policy to the resulting AST instead. Reads `command-allowlist`, `command-blocklist`, and `env-var-blocklist` as its data tables. |
| `command-allowlist` (data file, not a hook) | N/A | Known-good command names for `reject-obfuscated-commands`; a command not on this list (and not on `command-blocklist`, which wins) is rejected outright. |
| `command-blocklist` (data file, not a hook) | N/A | Known-bad command names for `reject-obfuscated-commands` (e.g. `eval`, `source`, `bash`) that are rejected even though they are plain bare words, because each one hides or re-enters execution in a way this check cannot see through. |
| `env-var-blocklist` (data file, not a hook) | N/A | Environment variables (`PATH`, `IFS`, `LD_PRELOAD`, `GIT_*`, and similar) that `reject-obfuscated-commands` refuses to let a command assign, because they change how *other* commands are located, parsed, or attributed. |
| `enforce-git-dash-c` | Any git subcommand not written as `git -C <dir> <command>` | See [Running Git Commands in a Specific Directory](git.instructions.md#running-git-commands-in-a-specific-directory). |
| `enforce-git-identity` | Git subcommands that create or rewrite commits (or precede one, like `fetch`) unless git identity and GPG signing are correctly configured | Prevents an unsigned or misattributed commit from being created at all, rather than relying on review to catch it afterwards. |
| `enforce-background-for-long-running-commands` | `git commit`, `pre-commit` (direct invocation), `dotnet build`, `dotnet test`, `npm test`, and `bun test` unless the call sets `run_in_background: true` | See [Never Truncate Test/Commit Commands](task-workflow.instructions.md#never-truncate-testcommit-commands-mandatory) for why these five have no safe foreground timeout. |
| `block-git-worktree` | `git worktree add` | Worktrees split repo state across multiple linked checkouts sharing one object store; this template's tooling assumes a single checkout per repo directory, and an errant `worktree add` has previously left the primary checkout bare with no work tree of its own. See [Avoid `git worktree`](git.instructions.md#avoid-git-worktree). |
| `block-dotnet-tool-install` | `dotnet tool install` (local or global) and `dotnet new tool-manifest` | This container's .NET global tools are pinned and baked into the image at build time; installing an unpinned tool at runtime would bypass the dependency-selection review the pinned set went through. |

If a command is blocked by a hook not listed here, or this table no longer matches
`$HOME/.claude/hooks` on a given container, treat the table as stale rather than the denial as
wrong: read the hook's own header comment (each one documents its rationale) before assuming it is
a bug.
