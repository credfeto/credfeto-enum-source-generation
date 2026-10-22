# NPM Package Version Instructions

> Load when: adding, updating, installing, or reviewing npm/JavaScript/TypeScript package dependencies.

[Back to Global Instructions Index](index.md)

- Adding any new npm package requires explicit human approval after a full security review; see [packages.instructions.md](packages.instructions.md#third-party-packages-require-human-approval-mandatory). Exception: a package a pre-commit component tool's own output (e.g. `npm audit`) demands as the specific fix for its failure; see [Conflict Resolution](packages.instructions.md#conflict-resolution-pre-commitcomponent-tool-mandated-package-changes-mandatory).

## Registry Lookup

- Before searching for, adding or updating a package, check which registry is actually configured: `npm config get registry`. Use that registry for lookups rather than assuming the public npm registry, since it may point at a private feed, proxy, or mirror.

## Running npm Commands (MANDATORY)

- Run every npm command as one plain command from the working directory. When `package-lock.json` is not at the repo root, add `--prefix <dir>`, where `<dir>` is the relative path inside the repo holding it (e.g. `src`).
- Run `npm ci` and `npm install` with the tool's `run_in_background`, then wait for them and read their output as [Background Tasks and Monitor Tool](task-workflow.instructions.md#background-tasks-and-monitor-tool-mandatory) describes.
- Never add a `cd`, a `--prefix` outside the allowed directories, a redirect to a file (`> $TMPDIR/npm.log`), a `;`/`&&` chain, or a pipe (such as `| tail`): a `cd`, chain, redirect or pipe makes the permission allow list reject the call, and a `--prefix` outside the allowed directories is blocked by the `enforce-allowed-dirs` hook, while the plain form is allowed.

## Installing Dependencies (MANDATORY)

- This covers installing from an existing lockfile; regenerating or updating `package-lock.json` follows [Fixed Package Versions](#fixed-package-versions).
- Install with exactly `npm ci --ignore-scripts`, plus `--prefix` where [Running npm Commands](#running-npm-commands-mandatory) requires it.
- Never install without `--ignore-scripts`, and never run `npm rebuild`: CI installs without scripts too, so builds must not depend on them.

## Fixed Package Versions

- Always use **exact (pinned) version numbers** in `package.json`, no `^` or `~` prefixes.
- New packages must be added with `npm install --save-exact --ignore-scripts` (or `npm install -E --ignore-scripts`) to avoid range specifiers.
- Dev dependencies must also use exact versions: `npm install --save-dev --save-exact --ignore-scripts`.
- Regenerate `package-lock.json` with `npm install --package-lock-only --ignore-scripts`.
- Do not use `*`, `latest`, or any semver range expressions.
- Updates to existing packages must be **explicit and intentional**: specify the target version directly rather than relying on range resolution.
- When updating a package, update only that package (and its required peer dependencies); do not allow transitive upgrades to pull in unreviewed version changes.
- When a merge or rebase produces conflicting versions of the same package, take the latest secure candidate and regenerate `package-lock.json` rather than hand-merging it; see [git-rebasing.instructions.md](git-rebasing.instructions.md#resolving-version-conflicts-when-merging-or-rebasing).
