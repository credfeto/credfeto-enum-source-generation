# Task Workflow Instructions

> Always loaded.

[Back to Global Instructions Index](index.md)

## Assignment

- Assign yourself to the issue before starting: `gh issue edit <number> --add-assignee @me`.
- Only work on unassigned issues or issues already assigned to you.
- Assign yourself to PRs when creating or updating: `gh pr edit <number> --add-assignee @me`.

## PR Lifecycle

- Only one active branch or open PR per repository at a time; do not create another until the current one is merged and closed.
- When adding work to an open PR (review comments, missing coverage, CI fixes), convert to draft first: `gh pr ready <number> --undo`. Keep it in draft until Code Tester and Code Reviewer are both satisfied — only PR Submitter converts it back.

## Bot-Created PRs (MANDATORY — treat as your own)

github is configured to automatically create PRs from pushed branches. These PRs appear authored by `app/github-actions` but the commits are authored by you. **They are your work — treat them identically to PRs you created yourself.**

**Before starting any work in a repository:**

1. Run `gh pr list --state open --repo <owner/repo> --json number,title,author,headRefName,url` — no `--author @me` filter.
2. For any PR authored by `app/github-actions`, check the commit authors: `gh pr view <n> --repo <owner/repo> --json commits --jq '.commits[].authors[].login'`.
3. If **all commits** are from your account (you are the sole committer), **take ownership**: update the PR title and body to match the proper format (summary, `Closes #<n>`, test plan), add yourself as assignee, and treat it as your active PR for that repo.
4. If commits are from multiple authors (e.g. you plus a human or Copilot), do **not** take over — leave the PR as-is and do not claim it as yours.
5. Do **not** create a new branch or PR for the same issue — that would be duplicate work.

**When you find a duplicate pair** (a bot-created PR and one you authored yourself, for the same issue or branch):

- Keep whichever has the more complete body and later review activity.
- Close the other with a comment explaining which PR supersedes it.

**Checking for existing work before branching (MANDATORY):**

- Check branch names in all open PRs, not just PR authors. If any open PR's `headRefName` contains the issue number, that is your work from a prior session — resume it instead of creating a new branch.

## PR Title, Body, and Label Sync (MANDATORY)

When creating or updating a PR linked to one or more issues:

1. Ensure the **title** accurately reflects all changes in the PR — update it if the scope has changed.
2. Ensure the **body** summarises all changes and includes `Closes #<n>` for each linked issue.
3. Copy all issue labels: `gh issue view <n> --json labels --jq '.labels[].name'` → `gh pr edit <n> --add-label "<label>"`

Repeat after every push or PR update.

## Rules Compliance for In-Flight Work

Whenever an instruction file is added or updated, re-evaluate all open branches and PRs against the new rules. Fix any non-compliance before continuing — treat it the same as a CI failure.

## Instruction File Source Routing

- If the file originates from `funfair/funfair-server-template` or `credfeto/cs-template`, raise an issue there first.
- Otherwise, make the change directly in the current repository.

## Large Multi-Handler / Multi-App Tasks

1. Create a top-level GitHub issue (if none specified); assign it; include the full original prompt as the body.
2. Comment findings on the issue before starting (handlers found, current state, etc.).
3. For each handler/app/component, create a sub-issue referencing the top-level issue; use the sub-issue number in branch names and commit messages.
4. Work on one handler/component at a time — commit and push before starting the next.
5. Close the sub-issue as soon as the relevant commits are pushed.

## Issue Tracking

- Only update issues if `gh` is installed and authenticated (`gh auth status`); otherwise read code and git log for state.
- Each sub-issue must list files with status: `❌ Not started` / `🔄 In progress` / `✅ Done` — update after each commit+push.
- The top-level issue tracks only handler/app-level status (sub-issues open/closed, branches merged).
- Update the sub-issue after each significant commit+push; update the top-level issue when overall status changes.
- When resuming, update the issue with current state before continuing.

## Commit, Push, and Issue Update Cadence

- One logical change per commit; do not batch unrelated changes.

Per-file cadence for coverage tasks:

1. Write tests until the file reaches target coverage.
2. Commit the test file; push immediately.
3. Update the sub-issue to mark the file done.
4. Move to the next file.

For complex files, commit+push+update after each round — do not wait until fully complete.

> Pre-commit hooks may make commits slow — wait for them to complete before assuming failure.

## Multi-Agent Implementation and Review Pattern

### Model Selection

| Use full model | Use lesser model |
| --- | --- |
| Orchestrator, Code Writer, Code Reviewer, Code Fixer, CI Debugger, Dependency Updater | Code Tester, Committer, Changelog, Rebase Agent, PR Submitter, CI Monitor |

### Failure Handling — No Self-Repair

Mechanical agents must not interpret or fix failures. When a check fails: capture the full output, stop immediately, and return failure details verbatim to the calling agent.

### Routing Rules

Standard loop pattern: Code Writer/Fixer loops ≤5 with Code Tester; Code Reviewer loops ≤5 re-running both each round.

| Work type | Agent sequence |
| --- | --- |
| New feature / bug fix / refactor | Code Writer → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| `CHANGES_REQUESTED` on existing PR | Code Fixer → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| Coverage-only task | Code Writer (tests only) → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| Documentation-only | Code Writer (docs only) → PR Submitter |
| Rebase requested | Rebase Agent → PR Submitter |
| CI failure (unknown cause) | CI Debugger |
| Dependabot / dependency update | Dependency Updater |

For detailed agent role definitions, see [agent-roles.instructions.md](agent-roles.instructions.md).

## Resuming Interrupted Work

- Check the status of existing issues and branches; skip merged branches.
- For unmerged branches, decide whether to continue or delete and recreate.
- Update the top-level issue with current status and next steps before resuming.
