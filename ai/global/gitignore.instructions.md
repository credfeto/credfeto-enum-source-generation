# .gitignore Instructions

[Back to Global Instructions Index](index.md)

## IDE Files

Never commit IDE-specific files or folders, including:

- `.idea/` — JetBrains IDEs
- `.vscode/` — Visual Studio Code
- `.vs/` — Visual Studio

These are covered by the root `.gitignore` in `credfeto/cs-template`; if any are missing, update there.

## Root `.gitignore`

The root `.gitignore` is the global baseline — only edit it in `credfeto/cs-template`. Updates are distributed to derived repositories via the standard template update mechanism.

## Additional `.gitignore` Files

Derived repositories may add `.gitignore` files for repo-specific concerns (language build output, generated content, local tooling artefacts), placed at the appropriate directory level.

## Consistency

When creating or modifying any `.gitignore`, check it against the root to ensure no duplication or conflicts.
