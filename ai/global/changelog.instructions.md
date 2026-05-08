# Changelog Instructions

[Back to Global Instructions Index](index.md)

Load this file when adding changelog entries or acting as the Changelog agent.

## Rules

- Use `Credfeto.Changelog.Cmd` — never edit `CHANGELOG.md` manually.
- `CHANGELOG.md` must be listed in `.markdownlintignore` at the repo root (create the file if absent).
- Entries must describe what changed and why it matters — not how it was implemented.

## When to Skip

Do **not** add an entry if:

- The repo is `credfeto/cs-template` (kept blank for template consumers).
- The change is documentation-only with no effect on production code.
- The change is to AI instruction files.

## Commands

```bash
# Add
dotnet changelog -f CHANGELOG.md -a <Type> -m "<message>"

# Remove
dotnet changelog -f CHANGELOG.md -r <Type> -m "<exact message to remove>"

# Help
dotnet changelog --help
```

Valid types: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`, `Deployment Changes`
