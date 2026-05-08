# Documentation Instructions

> Load when: writing code, documentation, comments, or commit messages.

[Back to Global Instructions Index](index.md)

## README

- Keep `README.md` accurate and up to date; rewrite template placeholder content before starting work.
- Include where applicable: CI badges (debug + release), links to `docs/`, build instructions, `CONTRIBUTING.md`, `SECURITY.md`, `CHANGELOG.md`.
- Update the configuration section in the same commit whenever a config option (`appsettings.json`, options class, or env var) is added, removed, or renamed — include type, default, and description.
- For changelog format and tooling see [changelog.instructions.md](changelog.instructions.md).

## Additional Documentation

- All repos except `credfeto/cs-template` must have a `docs/` folder; that repo must not.
- Place all docs and architecture diagrams (except `README.md`/`CHANGELOG.md`) in `docs/`; keep them current.
- Architecture diagrams: show folder structure only — omit individual files and package versions; prefer SVG over raster formats.
