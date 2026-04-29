# Shell Script Instructions

[Back to Global Instructions Index](index.md)

## Shebang

- Always prefer `#!/bin/sh` over `#!/bin/bash` for new scripts and when maintaining existing ones.
- Only use `#!/bin/bash` if the script requires bash-specific functionality that cannot otherwise be achieved without shelling out to another executable — in that case the bash dependency is justified by the cost saved.
- Keeping scripts to `#!/bin/sh` keeps them portable and avoids implicit reliance on bashisms that may not be available in all environments.

## Linting

- All `#!/bin/sh` scripts must pass both `shellcheck` and `checkbashisms` before committing.

## Visual Indicators

> These conventions apply to **standalone shell scripts only**. GitHub Actions `run:` steps use a different set of emoji indicators — see [github-workflows.instructions.md](github-workflows.instructions.md#step-output-formatting).

Use consistent coloured prefixes for outcome messages so that success and failure are immediately visible in terminal output:

- **Failure** — prefix with a red `✗`:

```sh
die() {
    printf '\n\033[31m✗\033[0m %s\n' "$*"
    exit 1
}
```

- **Success** — prefix with a green `✓`:

```sh
success() {
    printf '\n\033[32m✓\033[0m %s\n' "$*"
}
```

- **Progress/info** — prefix with a green `→`:

```sh
info() {
    printf '\n\033[32m→\033[0m %s\n' "$*"
}
```

Use `printf` rather than `echo` for portable escape-sequence handling, and `"$*"` to pass the message as a single string (required for `shellcheck` and `checkbashisms` compliance).

## AI Agent Detection

Scripts that need to behave differently when invoked by an AI agent (e.g. providing plainer error messages or applying stricter guards) must use the standard helper below rather than inlining the detection:

```sh
# Returns true (0) when running inside a Claude Code Bash-tool session.
# Claude Code sets CLAUDECODE=1 in every shell it spawns via the Bash tool;
# that value is inherited by subprocesses (e.g. git hooks).
# Source: https://docs.anthropic.com/en/docs/claude-code/settings#environment-variables
is_ai_agent() {
    [ "${CLAUDECODE}" = "1" ]
}
```

Usage:

```sh
if is_ai_agent; then
    die "Prohibited — did you read the .ai-instructions?"
else
    die "Normal human-facing error message"
fi
```

- Define `is_ai_agent` alongside the other output helpers (`die`, `success`, `info`) near the top of the script, not inline at the point of use.
- Always keep the source URL comment so future maintainers know where `CLAUDECODE` comes from.

## No naked echo or printf

Never use bare `echo` or `printf` for user-facing output. Always route through one of the three output functions:

- `die` — fatal error, exits non-zero
- `success` — step or overall completion
- `info` — progress announcement before a step begins

Interpolate any variables directly into the string argument rather than using a format string with separate arguments:

```sh
info "Opening port ${PORT}/tcp..."   # correct
printf '→ Opening port %s/tcp...\n' "${PORT}"  # wrong — naked printf
```
