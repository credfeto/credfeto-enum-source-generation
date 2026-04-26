# Git Instructions

[Back to Global Instructions Index](index.md)

## Language/Runtime Prerequisites (MANDATORY before any work)

- Before starting any work, verify that all languages and runtimes required by the project are installed and available.
- If a required language or runtime is not installed, **stop immediately**. Do not attempt to work around the missing tooling, scaffold code, or make partial changes. Ask the user to install it first.

## Build and Test Verification (MANDATORY before any commit or push)

- Before committing or pushing any change, the project must build and all tests must pass.
- If the build fails or any test fails, **do not commit or push**. Fix the issues first.
- If build or test errors cannot be resolved, stop and ask the user for guidance.

## Git Identity Check (MANDATORY before any commit)

Before making any git commit, verify the configured identity is correct and GPG signing is enabled:

```bash
CURRENT_EMAIL=$(git config user.email)
if [ "$CURRENT_EMAIL" = "andy@nanoclaw.ai" ] || [ -z "$CURRENT_EMAIL" ]; then
  echo "ERROR: Git is configured with the wrong identity ($CURRENT_EMAIL). Aborting."
  exit 1
fi
if [ "$(git config commit.gpgsign)" != "true" ]; then
  echo "ERROR: GPG signing is not enabled. Aborting."
  exit 1
fi
```

**If either check fails: stop all work immediately, do not commit, and report the misconfiguration.**

## Pre-Commit Branch Check

- Always verify the current branch before staging or committing — run `git branch --show-current` and confirm it is the expected working branch.
- Never commit if the current branch is `main`.
- If the current branch has switched back to `main` and the upstream branch no longer exists (i.e. it has been merged and deleted), this is a strong signal that a new branch is needed before continuing work — create one before making any further changes.

## GitHub Issues

- If the GitHub CLI (`gh`) is available, it must be used to manage issues as part of every piece of work.
- Before starting work, either:
  - Find an existing issue that is a **100% match** for the task — confirm with the user before linking to it, or
  - Create a new issue that includes the original prompt and a clear description of what needs to be done.
- For complex or multi-component tasks, see [task-workflow.instructions.md](task-workflow.instructions.md) for how to break work into sub-issues and track progress.
- Issue numbers must be referenced in commit messages and branch names where applicable (see branch naming rules above).
- If work on an issue is abandoned for any reason (e.g. benchmarks show no gain, investigation reveals the change is not worthwhile), comment on the issue with the findings before closing or leaving it — do not abandon an issue silently.

## GitHub CLI (`gh`) Proxy Behavior

When the `GH_HOST` environment variable is set to a value other than `github.com`, the `gh` CLI is routing through a proxy (e.g. `credfeto/github-api-proxy`).

- **If a `gh` command fails** while `GH_HOST` is overridden, raise an issue on `credfeto/github-api-proxy` with:
  - The exact `gh` subcommand and flags used.
  - The API method (HTTP verb + path) that failed, if visible in the error output.
  - The full error message returned.
- **Commit and push operations will always be rejected** when routed through the proxy — the proxy does not support write operations on repository content. Do not attempt to work around this; use `git` CLI directly for all commit and push operations (see Committer rules in [task-workflow.instructions.md](task-workflow.instructions.md)).

## Branching

- All new work must be done in a branch. Never commit directly to `main` (also enforced by the Pre-Commit Branch Check above).
- Before making any changes, ensure the current branch is `main` and is up-to-date with `origin`.
- Until there is an explicit change in task, continue working in the same branch.
- Before starting any new piece of work on an existing branch, check whether `origin/main` has been updated — if it has, rebase the branch onto `origin/main` before continuing, resolving any conflicts in a way that retains the intent of the current branch's work.

## Branch Naming

- Branch names must follow the format `<type>/<name>`, mirroring Conventional Commits types, e.g.:
  - `feature/add-user-auth`
  - `fix/null-pointer-on-login`
  - `chore/update-dependencies`
  - `refactor/simplify-payment-flow`
- If the branch relates to a GitHub issue, include the issue number in the name, e.g. `fix/123-null-pointer-on-login`.
- If working on a branch that fixes multiple GitHub issues (e.g. a feature with several related bug fixes), each individual commit that fixes an issue should reference that issue number in the commit message body.

## Commits

- Changes should be worked on in small increments and committed one by one.
- Commits should be as small as possible — one logical change per commit:
  - When achieving 100% coverage, commit and push each file as soon as it reaches 100% coverage individually; do not batch multiple files into one commit.
  - Dead/unreachable code removal must be a separate commit from test changes, made only after running tests on the entire handler or app to confirm no code path accesses it; removing several methods or functions should be done one per commit.
  - For shared code removal, first verify the entire codebase does not access it — this can only be done once all apps and handlers have 100% code coverage; each shared removal is its own commit.
  - Never amend an existing commit; always create a new one.
- After every commit, push the changes to `origin` immediately.

## Commit Message Format

- Use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) format for all commit messages.
- Always include the user's original prompt verbatim in the commit description (body), prefixed with `Prompt: `. Do not include it in the commit title.

## Dependabot Vulnerability Warnings

- After any push, if the remote prints a message like:
  ```
  remote: GitHub found N vulnerabilities on <repo>'s default branch (X critical, Y high, ...).
  remote:      https://github.com/<repo>/security/dependabot
  ```
  check whether there are already open Dependabot PRs covering those vulnerabilities (`gh pr list --label dependencies`).
- If there are no open Dependabot PRs (or they do not cover all flagged advisories), visit `https://github.com/<repo>/security/dependabot`, review each unfixed advisory, and for any where a manual fix is possible create a GitHub Issue that:
  - Names the vulnerable package and the severity.
  - Describes the steps to fix it manually.
  - Is labelled `Security` and `AI-Work`.

## Template Rule Escalation (Non-Template Repos Only)

This section applies **only when working in a non-template repository** (i.e., any repo other than `credfeto/cs-template`).

If, during work in a non-template repo, a gap or needed change is identified in the global template rules or instructions:

1. **Do not** apply the rule change locally — template rules are managed centrally in `credfeto/cs-template`.
2. Create a new issue in `credfeto/cs-template` using the GitHub CLI:
   ```bash
   gh issue create --repo credfeto/cs-template \
     --title "<short description of the rule change>" \
     --label "AI-Work" \
     --body "..."
   ```
3. The issue body **must** include all of the following:
   - **Source repository**: The repo where the need was discovered.
   - **Current behaviour / gap**: What is missing or inconsistent in the existing rules.
   - **Proposed rule text**: The concrete rule update or new instruction text being requested.
   - **Reason for template propagation**: Why this change should apply across all repos, not just locally.
4. After creating the issue, note its URL in any relevant commit or PR description so the context is not lost.
5. Continue work in the current repo without waiting for the template issue to be resolved.
