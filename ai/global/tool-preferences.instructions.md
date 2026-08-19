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
