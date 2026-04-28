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

Use `printf` rather than `echo` for portable escape-sequence handling, and `"$*"` to pass the message as a single string (required for `shellcheck` and `checkbashisms` compliance).
