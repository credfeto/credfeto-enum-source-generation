# NPM Package Version Instructions

> Load when: adding, updating, or reviewing npm/JavaScript/TypeScript package dependencies.

[Back to Global Instructions Index](index.md)

## Fixed Package Versions

- Always use **exact (pinned) version numbers** in `package.json`, no `^` or `~` prefixes.
- New packages must be added with `npm install --save-exact` (or `npm install -E`) to avoid range specifiers.
- Dev dependencies must also use exact versions: `npm install --save-dev --save-exact`.
- Do not use `*`, `latest`, or any semver range expressions.
- Updates to existing packages must be **explicit and intentional**: specify the target version directly rather than relying on range resolution.
- When updating a package, update only that package (and its required peer dependencies); do not allow transitive upgrades to pull in unreviewed version changes.
- When a merge or rebase produces conflicting versions of the same package, take the latest secure candidate and regenerate `package-lock.json` rather than hand-merging it; see [git-rebasing.instructions.md](git-rebasing.instructions.md#resolving-version-conflicts-when-merging-or-rebasing).
