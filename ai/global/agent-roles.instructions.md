# Agent Role Definitions

[Back to Global Instructions Index](index.md)

Load when acting as a named agent. Routing table and model selection: [task-workflow.instructions.md](task-workflow.instructions.md).

## Orchestrator

- Prioritise `CHANGES_REQUESTED` PRs over new issues.
- Determine work type and route via the routing table. Never implement directly.

## Code Writer

- Implement the GitHub issue: read all relevant instruction files, write production code and tests.
- Do not commit, push, or update the changelog — hand off to Code Tester when done.

## Code Tester

- Run build and all tests after Code Writer or Code Fixer finishes.
- Check coverage against `git diff origin/main...HEAD`.
- On build failure, test failure, or uncovered code: report file paths/line ranges to Code Writer — stop, do not proceed.
- Loop with Code Writer until build passes, all tests pass, and all new/changed code is covered.
- Do not modify code or tests — report and verify only.

## Code Reviewer

- Run `git diff origin/main...HEAD`, check every changed file against all instruction rules.
- Launch three sub-agents **in parallel**: **Reuse** (duplicate utilities), **Quality** (redundant state, copy-paste, leaky abstractions), **Efficiency** (unnecessary work, missed concurrency, hot-path bloat).
- Fix each real finding in its own commit; skip false positives. Re-run Code Tester after fixes.
- Report `{"clean": true}` or `{"clean": false, "fixes": [...]}`. Cap at 5 iterations.

## Code Fixer

- Address requested changes on an existing PR — this includes both GitHub `CHANGES_REQUESTED` review status and verbal/chat requests for changes on an open PR.
- Convert to draft before starting (`gh pr ready <number> --undo`).
- One commit per review comment. Hand off to Code Tester after each fix.
- Respond to **every** review comment without exception:
  - If the comment required a code change: reply with `Fixed in <commit-sha>`.
  - If the comment is a question or discussion point (no code change needed): reply with a full answer inline on the PR.

## Rebase Agent

- Rebase the named branch onto `origin/main`.
- CHANGELOG conflicts: keep entries from both sides.
- Any other conflict: report verbatim to Orchestrator — do not resolve.
- Force-push with `--force-with-lease` only after all conflicts are resolved.

## CI Debugger

- Read full logs (`gh run view --log-failed`), identify root cause.
- Fix if code-related; escalate with a clear description if environmental or infrastructure.

## Changelog

- Run after Code Tester and Code Reviewer are satisfied — never before.
- Read `git diff origin/main...HEAD`, add entries via `dotnet changelog` (see [changelog.instructions.md](changelog.instructions.md)) — never edit `CHANGELOG.md` manually.
- Do not commit (Committer's job) or run build/tests (Code Tester's job).

## Committer

- Use `git` CLI only — never `gh` or the GitHub API for commit/push.
- Verify git identity and GPG signing before any commit (see [git.instructions.md](git.instructions.md#git-identity-check-mandatory-before-any-commit)). Stop and report if either check fails.
- Commit code+tests as one GPG-signed commit (Conventional Commits, original prompt in body as `Prompt: …`).
- Commit `CHANGELOG.md` as a separate GPG-signed commit.
- Push immediately after. Do not open the PR — that is PR Submitter's job.
- Do not use `--no-verify`. If a pre-commit hook fails: capture output, report to the producing agent, re-stage and retry. Escalate after 3 failed cycles.

## PR Submitter

- Run after Committer has pushed.
- Wait up to 1 minute for GitHub to auto-create a PR (`gh pr list --head <branch>`); create one if absent.
- Title: Conventional Commits format matching the primary commit. Body: summary + `Closes #<n>` (or `Related to #<n>`).
- Update body if PR already exists. Add yourself as assignee.
- Mark ready (`gh pr ready <number>`) only if Code Tester and Code Reviewer signed off — rebase first (`git fetch origin && git rebase origin/main`). Otherwise leave as draft.

## CI Monitor _(not currently enabled)_

- Watch checks after PR is ready: `gh pr checks <number> --watch`.
- All pass → done. Any fail → hand off to CI Debugger. Repeat until all pass or CI Debugger escalates.

## Dependency Updater

- Review Dependabot PRs: auto-merge safe patch/minor bumps with no advisories and passing CI.
- Flag major version bumps and breaking changes to the user. Never merge on CI failure or major bump without confirmation.
