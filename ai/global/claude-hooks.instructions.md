# Claude Code Hook Interaction Instructions

[Back to Global Instructions Index](index.md)

This template's development containers (and any interactive session with the hooks installed via
`install-claude-hooks`) run a fixed set of Claude Code `PreToolUse` hooks. Most match every Bash
tool call; a couple also match a specific non-Bash tool call (see the reference table below). This
file covers how to interpret a hook **denial** correctly, and how to
tell one apart from a denial coming from Claude Code's separate permission system; for how to
background and poll long-running commands once a call has actually been accepted, see
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

Different denials on similar-looking commands usually come from **different** hooks with
**different** fixes; do not average them into one general theory (e.g. "backgrounding is broken").
Read the exact hook name and message each time. For example, a plain `dotnet test` without
`run_in_background: true` is blocked by `enforce-background-for-long-running-commands`, while the same
command with both `run_in_background: true` *and* a `timeout N` shell wrapper is blocked by
`reject-obfuscated-commands` instead (`timeout` is categorically blocklisted, see below): a
wrapper-command rejection, unrelated to backgrounding. These are two independent, correctly-working
checks, not one contradiction.

## A Permission Denial Is Not a Hook Denial (MANDATORY)

Claude Code has a second way to refuse a command: the permission system itself, sitting above the
hook chain. Under `permissions.defaultMode: "dontAsk"`, a command that would normally prompt for
approval is auto-denied instead. This is not a `PreToolUse` hook running; no hook name appears
anywhere in the message.

Tell the two apart by the message shape, not by guessing at a cause:

- A **hook** denial names the hook and states a reason and a fix, in the shared form every hook
  uses: `Blocked (command did not run - fix and retry, do not wait for it): <reason>`.
- A **permission** denial names no hook, states no rule, and gives no fix, typically just
  `Permission to use Bash has been denied because Claude Code is running in don't ask mode.`

The one thing both share: the command **never ran**. Everything in
[A Denial Means the Command Never Ran](#a-denial-means-the-command-never-ran-mandatory) above
applies identically to a permission denial; do not wait for it to finish.

The most common cause of a permission denial is a search command that omits its mandated
exclusions. A Bash command naming a directory (`find <dir>`, `grep -r ... <dir>`, `cd <dir> && ...`)
is modelled as a read of everything under it. Without the exclusions required by
[Exclude Secret-Bearing Files From Repo Searches](tool-preferences.instructions.md#exclude-secret-bearing-files-from-repo-searches-mandatory),
it cannot be proven that a `.env`/`.database`/`.claude` path will not be read, so the call
escalates, and under `dontAsk` an escalation comes back as a denial rather than a prompt.

Confirmed in practice: a `find` over a work tree containing a `.claude` directory, run without the
mandated exclusions, was denied this way, message-for-message; the identical command scoped to a
subtree with no secret-bearing file ran clean. This is also a live, more general risk: an agent
working an unrelated issue in another repository hit both denial shapes on the same underlying
command (`pre-commit-check`), a genuine hook denial in the foreground (named hook, stated fix) and a
permission denial in the background (naming neither), read the two as one broken session rather
than two different denial shapes, and escalated to a human on the first occurrence. This differs
from averaging two hook denials into one theory: here, one denial was a hook and the other wasn't.
Identify which part of the command is being modelled as a broad read, narrow or exclude it, and
retry before escalating to a human.

## Prefer the Tool's Own Backgrounding Parameter (MANDATORY)

Use the tool's own `run_in_background: true` parameter, never shell-level backgrounding (`&`,
`nohup ... &`, `disown`). Shell-level backgrounding is blocked outright by
`enforce-background-for-long-running-commands` (see below) and produces the same
denial-misread-as-in-flight failure described above.

## Reference: Installed Hook Set

The exact hook set installed at `$HOME/.claude/hooks` (from `install-claude-hooks`) at the time
this file was written, in the order they run (the three `reject-obfuscated-commands` data files
are listed directly after it rather than at their own position, since they have none):

| Hook | Blocks | Why |
| --- | --- | --- |
| `reject-obfuscated-commands` | Any Bash command not built from plain, obviously-spelled command words (indirect execution, sub-shells, wrapper-flag smuggling) | Text/regex scanning for banned patterns is an arms race that never converges against a determined bypass attempt; this hook parses with a real shell parser and applies policy to the resulting AST instead. Reads `command-allowlist`, `command-blocklist`, and `env-var-blocklist` as its data tables. |
| `command-allowlist` (data file, not a hook) | N/A | Known-good command names for `reject-obfuscated-commands`; a command not on this list (and not on `command-blocklist`, which wins) is rejected outright. |
| `command-blocklist` (data file, not a hook) | N/A | Known-bad command names for `reject-obfuscated-commands` (e.g. `eval`, `source`, `bash`, and wrapper commands including `timeout` and `xargs`) that are rejected even though they are plain bare words, because each one hides or re-enters execution in a way this check cannot see through, or (for the wrapper commands) can smuggle another command past name-based checks. This is why a shell `timeout` wrapper around `dotnet test`/`dotnet build`/`git commit` is always rejected, `run_in_background: true` or not — see [Never Truncate Test/Commit Commands](task-workflow.instructions.md#never-truncate-testcommit-commands-mandatory). |
| `env-var-blocklist` (data file, not a hook) | N/A | Environment variables (`PATH`, `IFS`, `LD_PRELOAD`, `GIT_*`, and similar) that `reject-obfuscated-commands` refuses to let a command assign, because they change how *other* commands are located, parsed, or attributed. |
| `enforce-allowed-dirs` | `cd`/`pushd`, `git -C`, `npm --prefix`, `find` starting points, and `rm`/`mv`/`cp` operands outside a configured allowlist of directory roots, plus flags on those same commands that turn a path argument into code execution (`git --exec-path`/`--git-dir`/`--work-tree`, most `git -c` keys, `npm --script-shell`, `find -exec`/`-delete`, `rm --no-preserve-root`) | The permission-rule syntax has no typed placeholder for "a directory goes here", so a wildcarded directory position also matches any option injected there; this hook does the positional check statically instead. Configured via `allowed-dirs` (or a host-local `allowed-dirs.local` override); a directory outside every configured root blocks. |
| `block-no-verify` | `--no-verify`/`-n` on any git command that would skip commit hooks, and the equivalent on `mcp__github__.*` tool calls | Enforces "never bypass hooks or formatters": a failing pre-commit hook must be fixed and retried, not skipped. Installed globally on `PATH` rather than shipped under `claude-hooks/`, which is why its `claude-settings.json` entries (registered for both the `Bash` matcher and the `mcp__github__.*` matcher) omit the `$HOME/.claude/hooks/` prefix every other hook uses. |
| `enforce-git-identity` | Git subcommands that create or rewrite commits (or precede one, like `fetch`) unless git identity and GPG signing are correctly configured | Prevents an unsigned or misattributed commit from being created at all, rather than relying on review to catch it afterwards. |
| `enforce-git-dash-c` | Any git subcommand not written as `git -C <dir> <command>` | See [Running Git Commands in a Specific Directory](git.instructions.md#running-git-commands-in-a-specific-directory). |
| `block-git-worktree` | `git worktree add`, and the equivalent native `EnterWorktree` tool call | Worktrees split repo state across multiple linked checkouts sharing one object store; this template's tooling assumes a single checkout per repo directory, and an errant `worktree add` has previously left the primary checkout bare with no work tree of its own. See [Avoid `git worktree`](git.instructions.md#avoid-git-worktree). |
| `block-dotnet-tool-install` | `dotnet tool install` (local or global) and `dotnet new tool-manifest` | This container's .NET global tools are pinned and baked into the image at build time; installing an unpinned tool at runtime would bypass the dependency-selection review the pinned set went through. |
| `enforce-ssh-host-and-key` | Any `ssh` call other than exactly `ssh user@host command...` with no flags, and one when no usable key is loaded in the forwarded ssh-agent | `ssh` has a blanket allow entry; the danger is in the destination, not the verb, which a permission-rule prefix pattern cannot scope. Restricts the host to a private-network suffix and requires the agent to hold a working key first, since this container never mounts raw private key files. |
| `enforce-background-for-long-running-commands` | `git commit`, `pre-commit` (direct invocation), `pre-commit-check` (this template's wrapper around it), `dotnet build`, `dotnet test`, `npm test`, and `bun test` unless the call sets `run_in_background: true` | See [Never Truncate Test/Commit Commands](task-workflow.instructions.md#never-truncate-testcommit-commands-mandatory) for why none of these have a safe foreground timeout. |
| `cache-gh-lookups` | Nothing; it never blocks | Rewrites a bare `gh api user --jq '.login'` call to read a cached copy instead of hitting the API every time, falling through unchanged on any parse failure. |

If a command is blocked by a hook not listed here, or this table no longer matches
`$HOME/.claude/hooks` on a given container, treat the table as stale rather than the denial as
wrong: read the hook's own header comment (each one documents its rationale) before assuming it is
a bug.
