# Shell Script Instructions

> Load when: any `.sh` file is present or shell script work is needed.

[Back to Global Instructions Index](index.md)

## Shebang

- Prefer `#!/bin/sh`; only use `#!/bin/bash` if bash-specific functionality is genuinely required.
- All `#!/bin/sh` scripts must pass `shellcheck` and `checkbashisms` before committing.

## Output Helpers

> Applies to **standalone shell scripts only**. GitHub Actions `run:` steps use emoji indicators; see [github-workflows.instructions.md](github-workflows.instructions.md#step-output-formatting).

Use `die`, `success`, and `info` for all user-facing output; never bare `echo` or `printf`. See [shell-scripts.examples.md](shell-scripts.examples.md) for implementations and usage examples.

- `die`: fatal error, red `✗` to stderr, exits non-zero
- `success`: completion, green `✓`
- `info`: progress/step announcement, green `→`

## AI Agent Detection

Scripts that behave differently when invoked by an AI agent must use the standard `is_ai_agent` helper; see [shell-scripts.examples.md](shell-scripts.examples.md).

## Argument Size Limits

Never pass a value of unbounded or externally-sourced size (an API response, accumulated log/comment data, file contents, etc.) as a single command-line argument to an external command. Use stdin (piping), or a temp file with a flag designed for it (e.g. `jq --slurpfile`/`--rawfile` instead of `--argjson`/`--arg`), instead.

- This applies even when the total combined argument list looks well under `ARG_MAX`: a single argv string is separately capped at `MAX_ARG_STRLEN` (128KiB on Linux), and that per-string ceiling is the one that actually gets hit in practice with growing data.
- Values that are inherently small and bounded (flags, IDs, short fixed strings, scalars) are fine as regular arguments.
