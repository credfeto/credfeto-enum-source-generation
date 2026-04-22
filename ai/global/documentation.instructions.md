# Documentation Instructions

[Back to Global Instructions Index](index.md)

## README

- `README.md` must contain a clear description of what the repository is for and be kept up to date as things change.
- If the `README.md` still contains template placeholder content (e.g. describes itself as a template), it must be rewritten automatically to reflect the actual repository when work begins.
- `README.md` must include the following links and badges where applicable:
  - GitHub Actions workflow badges for **debug** and **release** builds/tests (as appropriate for the repository).
  - Link to the head of any project documentation (e.g. `docs/`).
  - Link to build instructions in the documentation.
  - Link to the contributing guide — use the standard filename (`CONTRIBUTING.md`).
  - Link to the security policy — use the standard filename (`SECURITY.md`).
  - Link to `CHANGELOG.md`.

## Configuration Documentation

- Whenever a configuration option is added, removed, or renamed (in `appsettings.json`, an options class, or environment variable mapping), the corresponding section in `README.md` must be updated in the same commit.
- The `README.md` configuration section must accurately reflect all current options, their types, default values, and a brief description of what each does.
- This applies to any project that has a `README.md` with a configuration or setup section.

## CHANGELOG

- Maintain `CHANGELOG.md` in [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format to track changes and updates to the project.
- Purely documentation changes should not be added to the changelog.
- The `Credfeto.Changelog.Cmd` dotnet tool is the **only** permitted way to modify `CHANGELOG.md`, regardless of the project's primary language — never edit the file manually, whether adding or removing entries.
- Changelog entries must be written so they are easily understandable by someone who has not read the code — describe what changed and why it matters, not how it was implemented.
- Each task should be updated in `CHANGELOG.md`, **unless**:
  - This is the `git@github.com:credfeto/cs-template.git` repository — the `CHANGELOG.md` there is expected to be kept blank, ready for new repositories using the template.
  - It is purely a documentation change and does not affect any production code.
  - It is an AI instructions change.

### Adding a changelog entry

Use the `-a` flag with one of the standard [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) change types:

```
dotnet changelog -f CHANGELOG.md -a Added    -m "Brief description of what was added"
dotnet changelog -f CHANGELOG.md -a Changed  -m "Brief description of what changed and why"
dotnet changelog -f CHANGELOG.md -a Deprecated -m "Brief description of what is now deprecated"
dotnet changelog -f CHANGELOG.md -a Removed  -m "Brief description of what was removed"
dotnet changelog -f CHANGELOG.md -a Fixed    -m "Brief description of what was fixed"
dotnet changelog -f CHANGELOG.md -a Security -m "Brief description of the security fix"
```

### Removing a changelog entry

Use the `-r` flag with the change type and the exact message to remove:

```
dotnet changelog -f CHANGELOG.md -r <ChangeType> -m "<exact message to remove>"
```

### All other operations

For any other changelog operations, consult:

```
dotnet changelog --help
```

## Additional Documentation

- Documentation should be generated and maintained for repositories other than `git@github.com:credfeto/cs-template.git`.
- The `git@github.com:credfeto/cs-template.git` repository must not contain a `docs` folder — documentation is explicitly for derived repositories only.
- Documentation and architecture diagrams other than `README.md` and `CHANGELOG.md` should be placed in the `docs` folder.
- Documentation should be kept up-to-date as changes are applied and be created when missing.
- Do not specify every detail (e.g. package version numbers, every single file) in architecture diagrams — folder structure is sufficient.
- Where diagrams are generated, prefer vector formats (e.g. SVG) over raster formats (e.g. PNG, JPEG).
