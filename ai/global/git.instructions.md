# Git Instructions

[Back to Global Instructions Index](index.md)

## Language/Runtime Prerequisites (MANDATORY before any work)

Verify all required languages and runtimes are installed. If any are missing, stop — do not scaffold code or make partial changes; ask the user to install them first.

## Environment Health (MANDATORY)

If the environment is too broken to work in without first fixing infrastructure or tooling, **stop** and demand it be fixed. Do not work around broken tooling.

## Build and Test Verification (MANDATORY before any commit or push)

Build must pass and all tests must pass before committing or pushing. If they fail and cannot be resolved, stop and ask.

## Git Identity Check (MANDATORY before any commit)

Before any commit, verify identity and GPG signing are correct — see [git.examples.md](git.examples.md) for the check script.

**If either check fails: stop all work, do not commit, and report the misconfiguration.**

## Pre-Commit Branch Check

- Run `git branch --show-current` and confirm it is the expected working branch before staging or committing.
- Never commit if the current branch is `main`.
- If the branch has switched to `main` and the upstream no longer exists (merged and deleted), create a new branch before continuing.

## GitHub Issues

- If `gh` is available, use it to manage issues for every piece of work.
- Before starting, either find a **100% matching** existing issue (confirm with user before linking) or create a new one with the original prompt and a clear description.
- For complex or multi-component tasks, see [task-workflow.instructions.md](task-workflow.instructions.md).
- Reference issue numbers in commit messages and branch names.
- If work on an issue is abandoned, comment with findings before closing — do not abandon silently.

## GitHub CLI (`gh`) Proxy Behavior

When `GH_HOST` is set to a value other than `github.com`, `gh` routes through a proxy:

- If a `gh` command fails, raise an issue on `credfeto/github-api-proxy` with the exact subcommand and flags, the API method (if visible), and the full error message.
- Commit and push operations are always rejected by the proxy — use `git` CLI directly for all commit and push operations.

## Branching

- All new work must be in a branch — never commit directly to `main`.
- Ensure `main` is up-to-date with `origin` before starting.
- Continue in the same branch until the task changes.
- Before continuing work on an existing branch, check if `origin/main` has advanced — if so, rebase first.

## Branch Naming

Format: `<type>/<name>` (mirroring Conventional Commits types):

- `feature/add-user-auth`
- `fix/null-pointer-on-login`
- `chore/update-dependencies`
- `refactor/simplify-payment-flow`

Include the issue number when applicable: `fix/123-null-pointer-on-login`.

For branches fixing multiple issues, reference each issue number in the individual commit message bodies.

## Commits

See [git-commits.instructions.md](git-commits.instructions.md) — load when about to commit.

## Dependabot Vulnerability Warnings

After any push, if the remote reports vulnerabilities:

- Check for open Dependabot PRs covering them (`gh pr list --label dependencies`).
- If none exist, visit the repo's Dependabot page and for any manually fixable advisory create a GitHub issue labelled `Security` and `AI-Work`, naming the package, severity, and fix steps.

## Pre-Commit Hook Known Incompatibilities

- **dotenv-linter**: use `entry: dotenv-linter check` — v3.x requires the `check` subcommand before the filename.

## Template Rule Escalation (Non-Template Repos Only)

When working outside `credfeto/cs-template` and a gap in the global template rules is found:

1. Do not apply the change locally.
2. Create an issue in `credfeto/cs-template` — see [git.examples.md](git.examples.md) for the command template.
3. The issue must include: source repository, current behaviour/gap, proposed rule text, reason for template propagation.
4. Note the issue URL in any relevant commit or PR description.
5. Continue work without waiting for the template issue to be resolved.
