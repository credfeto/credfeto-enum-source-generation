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
- **Only convert back to ready for review once all work is complete** and Code Tester and Code Reviewer are both satisfied — this is done by PR Submitter at the end of the pipeline, not manually.

## Rules Compliance for In-Flight Work

Whenever any instruction file is added or updated — in this repo or in the global cs-template — all currently open branches and PRs must be re-evaluated against the new rules before being merged.

- After any rule change, review every open branch (`git diff origin/main...HEAD`) and every open PR to check whether the new or updated rules apply to the code already written.
- If any code or documentation on an in-flight branch does not comply, fix it on that branch before continuing other work on it.
- Treat rule compliance the same as a CI failure: work cannot be considered done until the code satisfies all current rules, not the rules that existed when the work started.

This applies to all rule types: coding conventions, test conventions, documentation structure, workflow rules, and AI instruction files themselves.

## Instruction File Source Routing

When an instruction file needs to be changed or a new rule needs to be added:

- If the file originates from `funfair/funfair-server-template` and the required change is not already present in that repository, raise an issue on `funfair/funfair-server-template` to get it added there — do not make the change only in the current repository.
- If the file originates from `credfeto/cs-template` and the required change is not already present in that repository, raise an issue on `credfeto/cs-template` to get it added there — do not make the change only in the current repository.
- If neither of the above applies, make the change directly in the current repository.

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

### General Agent Principles

#### Model selection

Agents that perform mechanical, well-defined tasks (running builds, committing, submitting PRs, monitoring CI, rebasing) must use a smaller/cheaper model. Agents that require judgement, creativity, or diagnosis (Code Writer, Code Reviewer, Code Fixer, CI Debugger, Orchestrator) use the full model.

| Use full model | Use lesser model |
| --- | --- |
| Orchestrator, Code Writer, Code Reviewer, Code Fixer, CI Debugger, Dependency Updater | Code Tester, Committer, Changelog, Rebase Agent, PR Submitter, CI Monitor |

#### Failure handling — no self-repair

Mechanical agents must not attempt to interpret or fix failures themselves. When a hardcoded check fails, the agent must:

1. Capture the full output of the failing step.
2. Stop immediately — do not proceed with subsequent steps.
3. Return the failure details verbatim to the calling agent so it can decide how to respond.

The calling agent is responsible for diagnosis and repair. For example:

- If a pre-commit hook fails during a commit, Committer sends the full hook output back to Code Writer and does not attempt to fix the code.
- If a build fails during Code Tester's run, Code Tester sends the full compiler output back to Code Writer and does not attempt to fix the code.
- If a rebase produces conflicts other than CHANGELOG conflicts, Rebase Agent reports the conflict details to the Orchestrator rather than attempting to resolve them (CHANGELOG conflicts have a deterministic rule — see Rebase Agent definition).

This keeps mechanical agents simple and predictable, and ensures all repair decisions are made by agents with the context and capability to make them correctly.

### Agent Roles

#### Orchestrator

- Checks for `CHANGES_REQUESTED` PRs first — these always take priority over new issues.
- Determines work type and routes to the correct agent.
- Never does implementation itself.

#### Code Writer

- Implements a GitHub issue: reads all instruction files, writes production code and the corresponding tests.
- Does NOT commit, push, or update the changelog — hands off to Code Tester once implementation is complete.

#### Code Tester

- Runs after Code Writer (or Code Fixer) has finished writing code and tests.
- Runs the project build.
- Runs all tests.
- Checks test coverage against all code added or changed in the current branch (`git diff origin/main...HEAD` to identify changed files).
- **If the build fails**: reports the full error output to Code Writer and waits for a fix — does not proceed.
- **If any test fails**: reports the test name, failure message, and full output to Code Writer and waits for a fix — does not proceed.
- **If new or changed code is not fully covered by tests**: reports the exact file paths and line ranges that lack coverage to Code Writer and waits for additional tests — does not proceed.
- Loops with Code Writer until all three conditions are satisfied: build passes, all tests pass, all new/changed code is covered.
- Does not modify code or tests itself — reports and verifies only.

#### Code Reviewer

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

#### Code Fixer (PR Review Responder)

- Addresses `CHANGES_REQUESTED` review comments on an existing PR.
- Converts the PR to draft before starting any work (see PR Draft State rules above).
- Each review comment gets its own separate commit.
- Hands off to Code Tester after each fix rather than running build/tests itself.

#### Rebase Agent

- Rebases a named branch onto `origin/main`.
- CHANGELOG conflicts are the one deterministic exception to the failure-punt rule: always resolve them by keeping entries from both sides (never discarding either) — no judgement required.
- Any other conflict must be reported verbatim to the Orchestrator — do not attempt to resolve it.
- Force-pushes with `--force-with-lease` only after all conflicts are resolved.

#### CI Debugger

- Invoked when CI fails and the cause is not obvious from the code change.
- Reads full workflow logs (`gh run view --log-failed`), identifies root cause.
- Fixes the cause if it is code-related; if environmental or infrastructure, escalates with a clear description.

#### Changelog

- Runs after both Code Tester and Code Reviewer are satisfied — never before.
- Reads `git diff origin/main...HEAD` to understand what changed.
- Adds appropriate changelog entries using the `dotnet changelog` tool (see [documentation.instructions.md](documentation.instructions.md) for the exact command syntax) — never edits `CHANGELOG.md` manually.
- Does NOT commit — that is Committer's responsibility.
- Does NOT run build or tests — that is Code Tester's responsibility.

#### Committer

- Runs after the Changelog agent has written the changelog entry.
- **Uses the `git` CLI exclusively** — never `gh`, GitHub REST API, or GitHub GraphQL API for any commit or push operation.
- Verifies git identity and GPG signing are correctly configured (runs the checks from [git.instructions.md](git.instructions.md#git-identity-check-mandatory-before-any-commit)).
- If either check fails: stops and reports the misconfiguration — does not proceed.
- All commits **must be GPG signed** (`git commit -S`). If signing fails, stop and report.
- Commits all pending code and test changes as one commit (Conventional Commits format, original prompt in body prefixed with `Prompt:`, GPG signed).
- If a pre-commit hook fails: capture the full hook output, abort, and return it to Code Writer — do not attempt to fix the code or retry.
- Commits `CHANGELOG.md` changes as a separate subsequent commit (also GPG signed).
- Pushes all commits to `origin` immediately after using `git push`.
- Does not open the PR — that is PR Submitter's responsibility.

#### Pre-commit hook failures

- Pre-commit hooks run automatically when `git commit` is executed. This is expected and intentional — do not use `--no-verify` to bypass them.
- If a pre-commit hook fails:
  1. Do **not** retry the commit immediately.
  2. Capture the full hook output (which hooks failed, the exact error messages).
  3. Report the failure details back to the agent that produced the change (Code Writer, Code Fixer, etc.) and wait for them to fix the code.
  4. Once the fix is received, re-stage the corrected files and retry the commit.
  5. If the same hook fails again after 3 fix-and-retry cycles, stop and escalate to the user — do not loop indefinitely.

#### PR Submitter

- Runs after Committer has pushed all commits to `origin`.
- Wait up to 1 minute for GitHub to automatically create a PR (e.g. via a branch protection rule or auto-PR workflow). Check with `gh pr list --head <branch>`.
- If no PR exists after 1 minute, create one: `gh pr create --title "<title>" --body "<body>"`.
- The PR title must follow Conventional Commits format and match the primary commit title on the branch.
- The PR body must include:
  - A brief summary of what changed and why.
  - A `Closes #<n>` line for every GitHub issue being resolved by the branch. Find these by scanning commit messages and branch name for issue numbers.
  - If the branch partially addresses an issue (i.e. does not fully resolve it), use `Related to #<n>` instead of `Closes #<n>`.
- If a PR already exists (created automatically or from a previous run), update its body to reflect the current set of issues and the current state of the branch: `gh pr edit <number> --body "<updated body>"`.
- Add yourself as assignee: `gh pr edit <number> --add-assignee @me`.
- Mark the PR ready for review (`gh pr ready <number>`) **only if** Code Tester and Code Reviewer have both signed off on this round — i.e. the pipeline that reached PR Submitter passed through both agents without outstanding issues. Before doing so, rebase the branch onto `origin/main` and resolve any conflicts: `git fetch origin && git rebase origin/main`.
- If the pipeline did not include Code Tester and Code Reviewer (e.g. a rebase-only run), leave the PR in whatever draft state it is currently in — do not flip it to ready.

#### CI Monitor _(not currently enabled — implementation TBD)_

- Runs after PR Submitter, once the PR is open and marked ready for review.
- Watches all required status checks on the PR for CI failures: `gh pr checks <number> --watch`.
- If all required checks pass, CI Monitor completes with no action.
- If any required check fails, hand the failure off to CI Debugger: pass the PR number and the names of the failing checks so CI Debugger can read the full logs.
- After CI Debugger has applied a fix and pushed, re-check all required checks — repeat until all pass or CI Debugger escalates to the user.
- Does not attempt to fix failures itself — diagnosis and repair belong to CI Debugger.

#### Dependency Updater

- Reviews Dependabot PRs: checks if the update is a safe patch/minor bump with no security advisories and CI passing.
- Auto-merges safe updates; flags breaking changes or major version bumps to the user.
- Never merges a dependency update that has CI failures or is a major version bump without user confirmation.

### Routing Rules

| Work type | Agent sequence |
| --- | --- |
| New feature / bug fix / refactor | Code Writer → Code Tester (loop ≤5 with Code Writer) → Code Reviewer (loop ≤5, re-running Code Writer and Code Tester each round) → Changelog → Committer → PR Submitter → CI Monitor |
| `CHANGES_REQUESTED` on existing PR | Code Fixer → Code Tester (loop ≤5 with Code Fixer) → Code Reviewer (loop ≤5, re-running Code Fixer and Code Tester each round) → Changelog → Committer → PR Submitter → CI Monitor |
| Coverage-only task | Code Writer (tests only) → Code Tester (loop ≤5 with Code Writer) → Code Reviewer (loop ≤5, re-running Code Writer and Code Tester each round) → Changelog → Committer → PR Submitter → CI Monitor |
| Documentation-only | Code Writer (docs only) → PR Submitter |
| Rebase requested | Rebase Agent → PR Submitter |
| CI failure (unknown cause) | CI Debugger |
| Dependabot / dependency update | Dependency Updater |

## Resuming Interrupted Work

When asked to resume a large task:

- Check the status of existing issues and branches (open, closed, merged).
- If a branch has been merged into main, skip that— the work is done.
- If a branch exists but is unmerged, decide whether to continue it or delete and recreate based on its state.
- Determine current position from issue/branch status and continue from there.
- Update the top-level issue with the current status and next steps before resuming work.
- Update the sub-issue as work progresses.
