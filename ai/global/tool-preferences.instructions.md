# Tool Preference Instructions

[Back to Global Instructions Index](index.md)

This file covers which tool to reach for when more than one could technically do the job - a
built-in Claude Code tool over an equivalent Bash command, or one Bash command over another. This
is distinct from [claude-hooks.instructions.md](claude-hooks.instructions.md), which covers how to
interpret a hook *denial* once a tool call has already been made; the rules here are about the
choice made before that call happens.

## Prefer Glob Over find for Simple File Listing (MANDATORY)

Use the `Glob` tool, not `find`, to list files matching a name/path pattern inside the workspace - it
is read-only by construction, always available, and does not go through the Bash-command hook chain
at all. Reach for `find` only when the need is something `Glob` cannot express: ownership/permission
predicates (`-user`, `-group`, `-perm`), time predicates (`-mtime`, `-newer`), or `-exec`.

## Exclude Secret-Bearing Files From Repo Searches (MANDATORY)

`.env`, `.env.*`, `*.env`, `.database`, any `.claude/` directory, and any file local instructions mark
as secret-bearing hold credentials or private configuration. Never let a repository-wide search read
them, whether via Bash or a built-in tool:

- `grep`/`rg`: add `--exclude='.env' --exclude='.env.*' --exclude='*.env' --exclude='.database' --exclude-dir='.claude'`
  (rg: `-g '!.env*' -g '!*.env' -g '!.database' -g '!.claude/'`).
- `find`: add `-not -path '*/.claude/*' -not -name '.env*' -not -name '*.env' -not -name '.database'`.
- `Glob`/`Grep` tools: use a pattern that cannot match them, or filter the result before reading any file.
- Never `cat`, `Read`, or print one of these files; if a value from it is genuinely needed, ask the user
  to run the command themselves.

The pre-execution deny hooks are the backstop for this rule, not the first line of defence: a denied
search means the command was written wrongly, not that the hook is over-strict.
