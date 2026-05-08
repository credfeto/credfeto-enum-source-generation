# Shell Script Examples

[Back to Global Instructions Index](index.md)

Reference implementations for the helper functions described in [shell-scripts.instructions.md](shell-scripts.instructions.md). Load this file when actively writing or modifying shell scripts.

## Output Helpers

```sh
die() {
    printf '\n\033[31m✗\033[0m %s\n' "$*" >&2
    exit 1
}

success() {
    printf '\n\033[32m✓\033[0m %s\n' "$*"
}

info() {
    printf '\n\033[32m→\033[0m %s\n' "$*"
}
```

Always direct `die()` to stderr (`>&2`) so error messages are not captured by stdout pipelines. Use `"$*"` to pass the message as a single string (required for `shellcheck` and `checkbashisms` compliance).

### Usage Example

```sh
info "Opening port ${PORT}/tcp..."   # correct — uses helper
printf '→ Opening port %s/tcp...\n' "${PORT}"  # wrong — naked printf
```

## AI Agent Detection

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

Define `is_ai_agent` alongside the other output helpers near the top of the script, not inline at the point of use. Always keep the source URL comment.
