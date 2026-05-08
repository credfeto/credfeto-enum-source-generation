# Shell Script Instructions

> Load when: any `.sh` file is present or shell script work is needed.

[Back to Global Instructions Index](index.md)

## Shebang

- Prefer `#!/bin/sh`; only use `#!/bin/bash` if bash-specific functionality is genuinely required.
- All `#!/bin/sh` scripts must pass `shellcheck` and `checkbashisms` before committing.

## Output Helpers

> Applies to **standalone shell scripts only**. GitHub Actions `run:` steps use emoji indicators — see [github-workflows.instructions.md](github-workflows.instructions.md#step-output-formatting).

Use `die`, `success`, and `info` for all user-facing output — never bare `echo` or `printf`. See [shell-scripts.examples.md](shell-scripts.examples.md) for implementations and usage examples.

- `die` — fatal error, red `✗` to stderr, exits non-zero
- `success` — completion, green `✓`
- `info` — progress/step announcement, green `→`

## AI Agent Detection

Scripts that behave differently when invoked by an AI agent must use the standard `is_ai_agent` helper — see [shell-scripts.examples.md](shell-scripts.examples.md).
