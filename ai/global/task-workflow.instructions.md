# Task Workflow Instructions

[Back to Global Instructions Index](index.md)

## Issue Assignment

- **When picking up an issue to work on**, assign it to yourself (`gh issue edit <number> --add-assignee @me`) before starting any work.
- **Do not pick up issues already assigned to someone else.** If an issue is assigned, skip it and move to the next available one.
- Only work on issues that are unassigned, or already assigned to you.

## PR Assignment

- **When creating or updating a PR**, always add yourself as an assignee (`gh pr edit <number> --add-assignee @me`).
- Do this at the time of PR creation, or immediately after if the PR already exists.

## PR and Branch Concurrency

- Only one active branch or open PR per repository at a time.
- Do not start new work (create a new branch or open a new PR) until the current branch has been merged and the PR closed.
- If asked to start new work while a PR is still open, stop and inform the user — do not proceed until either the open PR is merged or the user explicitly instructs you to work in parallel.

## PR Draft State

- **When additional work needs to be added to an open PR** (e.g. addressing review comments, adding missing coverage, fixing CI failures), convert it to a draft immediately before starting: `gh pr ready <number> --undo`.
- Keep the PR in draft for the entire duration of that work — do not flip it back early.
- **Only convert back to ready for review once all work is complete** and Code Tester and Code Reviewer are both satisfied.
- Before marking ready, rebase the branch onto `origin/main` to eliminate any merge conflicts: `git fetch origin && git rebase origin/main`. Resolve any conflicts before proceeding.
- Once rebased and clean, mark ready: `gh pr ready <number>`.

## Large Multi-Handler / Multi-App Tasks

When given a task that spans multiple handlers, apps, or components (e.g. "ensure 100% coverage for all handlers", "migrate all projects to a new package"):

1. **Create a top-level GitHub issue** if none is specified. Assign it to whoever asked. Include the full original prompt as the issue body.
2. **Comment findings** on the issue before starting any work (e.g. list handlers found, current state of each).
3. **For each handler/app/component**, create a sub-issue referencing the top-level issue. Use the sub-issue number in the branch name and commit messages.
4. **Work on one handler/component at a time** — do not start the next until the current one is committed and pushed.
5. **Push each branch** when the handler's work is complete.
6. **Close the sub-issue** as soon as the relevant commits have been pushed to the working branch — do not leave sub-issues open after the work is done.

## Sub-issue File Status Tracking

Each sub-issue (not the top-level issue) should contain the list of files being worked on and their status. Keep this updated as work progresses:

- Use a table or checklist with one row per file.
- Mark each file as: `❌ Not started`, `🔄 In progress`, or `✅ Done`.
- Update the sub-issue **immediately after each commit+push** that completes a file.
- For complex files where multiple rounds of changes are needed, update the sub-issue after each commit+push.

The top-level issue should only track handler/app-level status (which sub-issues are open/closed, which branches are merged).

## GitHub Issue Updates

- **Only update issues if the GitHub CLI (`gh`) is installed and properly authenticated.** To check: run `gh auth status`. If it fails or shows unauthenticated, determine current state by reading the code and git log instead — do not attempt issue updates.
- **Update the sub-issue** after each significant piece of work (each commit+push to the branch).
- **Update the top-level issue** when the overall status changes (e.g. a sub-issue is closed, a PR is raised).
- When resuming work, update the issue with current state before continuing.

## Commit, Push, and Issue Update Cadence

For coverage tasks, the cadence per file is:

1. Write tests until the file reaches target coverage.
2. `git commit` the test file (one file per commit).
3. `git push` the branch immediately — do not batch pushes.
4. Update the sub-issue to mark that file as done.
5. Move to the next file.

For complex files where it takes multiple rounds of changes:

1. After each round: commit, push, update the sub-issue.
2. Do not wait until the file is fully complete before the first push.

> **Note:** Commits may take longer than expected due to pre-commit hooks running linters. Do not assume failure if the commit is slow — wait for the hooks to complete before retrying.

## Multi-Agent Implementation and Review Pattern

### Agent Roles

**Orchestrator**

- Checks for `CHANGES_REQUESTED` PRs first — these always take priority over new issues.
- Determines work type and routes to the correct agent.
- Never does implementation itself.

**Code Writer**

- Implements a GitHub issue: reads all instruction files, writes production code and the corresponding tests.
- Does NOT commit, push, or update the changelog — hands off to Code Tester once implementation is complete.

**Code Tester**

- Runs after Code Writer (or Code Fixer) has finished writing code and tests.
- Runs the project build.
- Runs all tests.
- Checks test coverage against all code added or changed in the current branch (`git diff origin/main...HEAD` to identify changed files).
- **If the build fails**: reports the full error output to Code Writer and waits for a fix — does not proceed.
- **If any test fails**: reports the test name, failure message, and full output to Code Writer and waits for a fix — does not proceed.
- **If new or changed code is not fully covered by tests**: reports the exact file paths and line ranges that lack coverage to Code Writer and waits for additional tests — does not proceed.
- Loops with Code Writer until all three conditions are satisfied: build passes, all tests pass, all new/changed code is covered.
- Does not modify code or tests itself — reports and verifies only.

**Code Reviewer**

- Reviews a branch against every rule in the instruction files.
- Runs `git diff origin/main...HEAD`, checks every changed file.
- Launches three sub-agents **in parallel** against the full diff:
  1. **Reuse agent** — searches codebase for existing utilities/helpers that duplicate newly written code.
  2. **Quality agent** — checks for redundant state, copy-paste variation, leaky abstractions, unnecessary comments.
  3. **Efficiency agent** — checks for unnecessary work, missed concurrency, hot-path bloat, repeated lookups.
- Aggregates findings, fixes each real issue in its own commit, skips false positives.
- After applying any fixes, Code Tester must re-run and pass before this round is considered complete.
- Reports `{"clean": true}` or `{"clean": false, "fixes": [...]}`.
- Loops with Code Writer (via Code Tester) until clean, capped at 5 iterations.

**Code Fixer (PR Review Responder)**

- Addresses `CHANGES_REQUESTED` review comments on an existing PR.
- Converts the PR to draft before starting any work (see PR Draft State rules above).
- Each review comment gets its own separate commit.
- Hands off to Code Tester after each fix rather than running build/tests itself.

**Rebase Agent**

- Rebases a named branch onto `origin/main`.
- Resolves CHANGELOG conflicts by keeping entries from both sides (never discarding either).
- Force-pushes with `--force-with-lease`.

**CI Debugger**

- Invoked when CI fails and the cause is not obvious from the code change.
- Reads full workflow logs (`gh run view --log-failed`), identifies root cause.
- Fixes the cause if it is code-related; if environmental or infrastructure, escalates with a clear description.

**Changelog**

- Runs after both Code Tester and Code Reviewer are satisfied — never before.
- Reads `git diff origin/main...HEAD` to understand what changed.
- Adds appropriate changelog entries using the `dotnet changelog` tool (see [documentation.instructions.md](documentation.instructions.md) for the exact command syntax) — never edits `CHANGELOG.md` manually.
- Does NOT commit — that is Committer's responsibility.
- Does NOT run build or tests — that is Code Tester's responsibility.

**Committer**

- Runs after the Changelog agent has written the changelog entry.
- Verifies git identity and GPG signing are correctly configured (runs the checks from [git.instructions.md](git.instructions.md#git-identity-check-mandatory-before-any-commit)).
- If either check fails: stops and reports the misconfiguration — does not proceed.
- Commits all pending code and test changes as one commit (Conventional Commits format, original prompt in body prefixed with `Prompt:`, GPG signed).
- Commits `CHANGELOG.md` changes as a separate subsequent commit (also GPG signed).
- Pushes all commits to `origin` immediately after.
- Does not open the PR — that is handled downstream.

**Dependency Updater**

- Reviews Dependabot PRs: checks if the update is a safe patch/minor bump with no security advisories and CI passing.
- Auto-merges safe updates; flags breaking changes or major version bumps to the user.
- Never merges a dependency update that has CI failures or is a major version bump without user confirmation.

### Routing Rules

| Work type | Agent sequence                                                                                                                                                                       |
|---|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| New feature / bug fix / refactor | Code Writer → Code Tester (loop ≤5 with Code Writer) → Code Reviewer (loop ≤5, re-running Code Write and Code Tester each round) → Changelog → Committer → CI Monitor → PR Submitter |
| `CHANGES_REQUESTED` on existing PR | Code Fixer → Code Tester (loop ≤5 with Code Fixer) → Code Reviewer (loop ≤5, re-running Code Write and Code Tester each round) → Changelog → Committer → CI Monitor → PR Submitter                  |
| Coverage-only task | Code Writer (tests only) → Code Tester (loop ≤5 with Code Writer) → Code Reviewer (loop ≤5, re-running Code Write and Code Tester each round) → Changelog → Committer → CI Monitor → PR Submitter   |
| Documentation-only | Code Writer (docs only) → PR Submitter                                                                                                                                               |
| Rebase requested | Rebase Agent → PR Submitter                                                                                                                                                          |
| CI failure (unknown cause) | CI Debugger                                                                                                                                                                          |
| Dependabot / dependency update | Dependency Updater                                                                                                                                                                   |

## Resuming Interrupted Work

When asked to resume a large task:

- Check the status of existing issues and branches (open, closed, merged).
- If a branch has been merged into main, skip that— the work is done.
- If a branch exists but is unmerged, decide whether to continue it or delete and recreate based on its state.
- Determine current position from issue/branch status and continue from there.
- Update the top-level issue with the current status and next steps before resuming work.
- Update the sub-issue as work progresses.
