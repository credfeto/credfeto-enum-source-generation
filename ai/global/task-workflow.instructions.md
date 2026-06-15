# Task Workflow Instructions

> Always loaded.

[Back to Global Instructions Index](index.md)

## Assignment

- Assign yourself to the issue before starting: `gh issue edit <number> --add-assignee @me`.
- Only work on unassigned issues or issues already assigned to you.
- Assign yourself to PRs when creating or updating: `gh pr edit <number> --add-assignee @me`.

## GitHub Issue and PR Labels

### Priority Labels (highest to lowest)

| Label | Meaning |
| --- | --- |
| `Security` | Security fix — highest possible priority |
| `Urgent` | Get this done ASAP; security fixes take precedence |
| `High` | Addressed after `Urgent` work |
| `Medium` | Addressed after `High` work |
| `Low` | Addressed after `Medium` work |
| _(untagged)_ | No priority set — tracked but timing does not matter |

When selecting the next issue to work on, prefer issues with higher-priority labels. Skip any issue labelled `On-Hold` or `Blocked`.

### Status Labels

| Label | Meaning |
| --- | --- |
| `On-Hold` | Needs further thought or cannot be implemented yet — do not start work |
| `Blocked` | Needs human input before work can continue — see the Orchestrator section in [agent-roles.instructions.md](agent-roles.instructions.md) |

## PR Lifecycle

- Only one active branch or open PR per repository at a time; do not create another until the current one is merged and closed.
- **Before blocking new work** because of an existing PR: always verify its current state with `gh pr view <number> --repo <owner/repo> --json state,mergedAt` — never rely on conversation memory. A PR that was open earlier in the session may have since been merged.
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
4. Never remove any label from a PR or issue — GitHub workflows add labels automatically and they must not be removed.

Repeat after every push or PR update.

## Label Management (MANDATORY)

- Always use `--add-label` when adding labels — **never** `--label`, which replaces all existing labels and destroys automatically-applied classification labels.
- Never remove labels from issues or PRs. GitHub workflows add classification labels automatically; removing them breaks automation.

## Missing CLI Tools (MANDATORY)

If a required CLI tool is not found, **stop immediately and ask the user to install it**. Never:

- Search for the binary in alternative locations
- Manipulate PATH to try to find it
- Attempt to install it without being asked

**Exception — pre-commit hook tools:** Do not assume a tool is missing because `command -v` returns nothing in the current shell. Instead, follow the verification steps in [git.instructions.md](git.instructions.md) — stage your changes and run the hook directly. Only block if it actually fails.

## Rules Compliance for In-Flight Work

Whenever an instruction file is added or updated, re-evaluate all open branches and PRs against the new rules. Fix any non-compliance before continuing — treat it the same as a CI failure.

## Instruction File Source Routing

- For changes to shared global instruction files (`ai/global/**`), raise an issue in `credfeto/cs-template` — it is the canonical source for those files.
- For changes specific to FunFair server projects, raise an issue in `funfair-tech/funfair-server-template` instead.
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

## Background Tasks and Monitor Tool (MANDATORY)

When using the Monitor tool to watch a background Bash task, the poll condition in the `until` loop **must** be provably satisfiable — a condition that can never be met loops forever and blocks the entire session.

### Rules for poll conditions

1. **Never poll for `"exit code"`** — that string is not reliably written to background task output files. Poll for a specific string the command itself writes (see table below).

2. **Do not pipe after `grep -q` in a negation check.** `! grep -q "pattern" file | tail -1` does NOT detect absence — the pipe applies to grep's (empty) stdout, so `tail -1` exits 0 regardless, and `!` inverts that to always-false. Write `! grep -q "pattern" file` with no trailing pipe.

3. **Verify the poll string exists in real output before writing the loop.** If you cannot confirm what string the command writes, run the command in the foreground first and read its output.

4. **Prefer foreground over background for bounded work.** Use `run_in_background: true` only when a command genuinely takes many minutes (e.g. a full integration-test run) and you have independent work to do while waiting. A `dotnet build` or `git commit` should always run in the foreground.

5. **Time-box every poll loop — die after 30 minutes.** Always include a deadline so the session cannot hang forever:

   ```bash
   deadline=$(( $(date +%s) + 1800 ))
   until grep -q "Build succeeded." "${output_file}" 2>/dev/null; do
       sleep 15
       if [ "$(date +%s)" -ge "${deadline}" ]; then
           echo "ERROR: timed out after 30 minutes waiting for build" >&2
           exit 1
       fi
   done
   ```

   If the deadline fires, mark the work item Blocked and stop:

   ```bash
   gh issue edit <number> --repo <owner/repo> --add-label "Blocked"
   gh issue comment <number> --repo <owner/repo> \
       --body "Blocked: timed out after 30 minutes waiting for <what>. Last output: $(tail -5 "${output_file}" 2>/dev/null)"
   ```

   Use `gh pr edit` / `gh pr comment` instead if the work item is a PR. Then exit — do not continue work.

### Reliable poll strings by command

| Command / scenario | String to poll for |
| --- | --- |
| `dotnet build` succeeded | `Build succeeded.` |
| `dotnet test` all passed | `Passed!` |
| pre-commit hooks passed | `→ All checks passed.` |
| pre-commit hooks failed | `→` followed by `Failed` (check for both to distinguish pass/fail) |
| `git push` completed | `branch` (branch tracking line in push output) |
| `gh pr create` / `gh pr ready` | poll not needed — these exit immediately |

## Multi-Agent Implementation and Review Pattern

### Model Selection

| Use full model | Use lesser model |
| --- | --- |
| Orchestrator, Code Writer, Code Reviewer, Code Fixer, Coding Researcher, CI Debugger, Dependency Updater | Code Tester, Committer, Changelog, Rebase Agent, PR Submitter, CI Monitor |

### Failure Handling — No Self-Repair

Mechanical agents must not interpret or fix failures. When a check fails: capture the full output, stop immediately, and return failure details verbatim to the calling agent.

### Routing Rules

Standard loop pattern: Code Writer/Fixer loops ≤5 with Code Tester; Code Reviewer loops ≤5 re-running both each round. Code Writer, Code Fixer, Code Reviewer, and CI Debugger may invoke Coding Researcher on demand at any point when the knowledge to implement or fix is lacking — this does not count toward the standard loop limits, but each calling role may invoke Coding Researcher at most 3 times per work item. Before invoking, the calling role checks the work item's issue/PR for an existing `### Coding Researcher` comment answering the same question and reuses it if found — reused findings do not count toward the cap. After Coding Researcher returns, the calling role records the question and outcome as a `### Coding Researcher` comment on the issue/PR so it can be reused. On reaching the cap, or if Coding Researcher returns **Not possible**, the calling role stops and escalates to Orchestrator rather than continuing the loop or guessing.

| Work type | Agent sequence |
| --- | --- |
| New feature / bug fix / refactor | Code Writer → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| `CHANGES_REQUESTED` on existing PR, or verbal/chat request for changes on an open PR | Code Fixer (respond to every comment) → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
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
