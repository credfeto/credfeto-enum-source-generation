# Changelog Instructions

[Back to Global Instructions Index](index.md)

Load this file when adding changelog entries or acting as the Changelog agent.

## Rules

- Use `Credfeto.Changelog.Cmd`; never edit `CHANGELOG.md` manually. `Credfeto.Changelog.Cmd` is the dotnet tool package that provides the `dotnet changelog` command; no separate install step is required if the repo's dotnet tool manifest already includes it.
- `CHANGELOG.md` must be listed in `.markdownlintignore` at the repo root (create the file if absent).
- Entries must describe what changed and why it matters, not how it was implemented.

## When to Skip

Do **not** add a `CHANGELOG.md` entry if:

- The repository name contains `-template` (e.g. `credfeto/cs-template`), kept blank for template consumers. The Changelog agent's Placeholder step still commits a `.deleteme.now` file instead; see [agent-roles.instructions.md](agent-roles.instructions.md#changelog) for the convention.

## Dependabot and Other Bot PRs

- If you push any commit to a Dependabot (or other bot-authored) PR — taking ownership, rebasing, resolving conflicts, addressing review comments — later CI runs on that PR may no longer attribute `github.actor` to the bot. Some repos' changelog-check workflow (e.g. an `include-changelog-entry` job in `pr-lint.yml`) only skips the `CHANGELOG.md` diff check when `github.actor == 'dependabot[bot]'`, so the check can start failing even though the PR still carries a `Changelog Not Required` label from when the bot opened it.
- In that situation, if the repo's rules require a changelog entry, add one with `dotnet changelog` describing the change (e.g. the dependency bump) exactly as you would for any change you authored yourself. Do not rely on the pre-existing `Changelog Not Required` label once you have pushed to the PR; verify the CI check actually passes.
- This is a workaround for the actor-detection gap, not a substitute for fixing it. If you have write access to the workflow, also consider fixing the underlying check to key off `github.event.pull_request.user.login` instead of `github.actor` (see [github-workflows.instructions.md](github-workflows.instructions.md)) so future bot PRs are not affected.

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
