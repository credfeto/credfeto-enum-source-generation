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
| `Security` | Security fix, highest possible priority |
| `Urgent` | Get this done ASAP; security fixes take precedence |
| `High` | Addressed after `Urgent` work |
| `Medium` | Addressed after `High` work |
| `Low` | Addressed after `Medium` work |
| _(untagged)_ | No priority set, tracked but timing does not matter |

When selecting the next issue to work on, prefer issues with higher-priority labels. Skip any issue labelled `On-Hold` or `Blocked`.

### Status Labels

| Label | Meaning |
| --- | --- |
| `On-Hold` | Needs further thought or cannot be implemented yet; do not start work |
| `Blocked` | Needs human input before work can continue; see the Orchestrator section in [agent-roles.instructions.md](agent-roles.instructions.md) |

## GitHub Issue Creation (MANDATORY)

When asked to create or update a GitHub issue (i.e. the issue itself is the requested deliverable):

1. Enter Plan Mode.
2. Work out at a high level what code change the issue would represent: scope, affected files, approach.
3. Exit Plan Mode and return to auto.
4. Create the issue, or update the existing issue, using the plan output to write a meaningful description.

This is distinct from [Ad-Hoc Prompt Intake](#ad-hoc-prompt-intake-mandatory) below; that section covers being asked to _do_ something, where the issue is a tracking side-effect rather than the deliverable itself.

## Ad-Hoc Prompt Intake (MANDATORY)

Applies whenever a human asks you to _do_ something in the context of a repo (a task, not a request to raise an issue, which is covered above), and no existing issue or PR has already been specified as the thing to work on. No exception for triviality of the request, and no exception for the `credfeto/cs-template` repo itself.

1. Before taking any other action (including answering a read-only question), create a GitHub issue in the current repo:
   - Title: a concise summary of the prompt.
   - Body: the prompt, verbatim, as the starting point.
   - Labels: `AI-Work` and `Blocked` (minimum), always, regardless of who initiated the underlying task; add other relevant labels (e.g. priority) as appropriate.
2. Use Plan Mode to work out scope, affected files, and approach, and post it as an `## Implementation Plan` issue comment per the format in [agent-roles.instructions.md](agent-roles.instructions.md#issue-workflow-plan-first-new-issues-only).
3. As open questions are identified, add each as an issue comment as soon as it's identified; do not batch them all until the end.
4. Do not proceed until an explicit human approval comment exists (`approved` / `go ahead` / `looks good` / `lgtm`) and `Blocked` is removed; if approval came via live chat, mirror it as a GitHub comment first (see [Blocked Label](agent-roles.instructions.md#blocked-label)).
5. Once approved and `Blocked` is removed:
   - If the request needs a code change, proceed via the routing table below and open a PR referencing the issue when ready.
   - If the request is read-only/informational (no code change), post the answer as an issue comment and close the issue.
6. See [Prompt Traceability](#prompt-traceability-mandatory) below for further prompts once an issue or PR already exists for the work.

## Prompt Traceability (MANDATORY)

Once a request is already tracked by an issue or PR (including one just created under [Ad-Hoc Prompt Intake](#ad-hoc-prompt-intake-mandatory) above), every subsequent prompt from the human that changes, redirects, or adds detail to that work must be recorded on that issue/PR:

- Comment with the prompt (verbatim, or a faithful summary for long prompts) and how it was resolved: a code change, an answered question, a scope adjustment, etc.
- Post this before or immediately after acting on the prompt; do not let several prompts accumulate unrecorded.
- This applies whether the prompt arrived as a live chat message or as a GitHub comment (GitHub comments are already covered by [Comment Replies](agent-roles.instructions.md#comment-replies-mandatory)).

## PR Lifecycle

- Only one active branch or open PR per repository at a time; do not create another until the current one is merged and closed.
- **Before blocking new work** because of an existing PR: always verify its current state with `gh pr view <number> --repo <owner/repo> --json state,mergedAt`; never rely on conversation memory. A PR that was open earlier in the session may have since been merged.
- When adding work to an open PR (review comments, missing coverage, CI fixes), convert to draft first: `gh pr ready <number> --undo`. Keep it in draft until Code Tester and Code Reviewer are both satisfied; only PR Submitter converts it back.

## Bot-Created PRs (MANDATORY, treat as your own)

github is configured to automatically create PRs from pushed branches. These PRs appear authored by `app/github-actions` but the commits are authored by you. **They are your work; treat them identically to PRs you created yourself.**

**Before starting any work in a repository:**

1. Run `gh pr list --state open --repo <owner/repo> --json number,title,author,headRefName,url`, no `--author @me` filter.
2. For any PR authored by `app/github-actions`, check the commit authors: `gh pr view <n> --repo <owner/repo> --json commits --jq '.commits[].authors[].login'`.
3. If **all commits** are from your account (you are the sole committer), **take ownership**: update the PR title and body to match the proper format (summary, `Closes #<n>`, test plan), add yourself as assignee, and treat it as your active PR for that repo.
4. If commits are from multiple authors (e.g. you plus a human or Copilot), do **not** take over; leave the PR as-is and do not claim it as yours.
5. Do **not** create a new branch or PR for the same issue; that would be duplicate work.

**When you find a duplicate pair** (a bot-created PR and one you authored yourself, for the same issue or branch):

- Keep whichever has the more complete body and later review activity.
- Close the other with a comment explaining which PR supersedes it.

**Checking for existing work before branching (MANDATORY):**

- Check branch names in all open PRs, not just PR authors. If any open PR's `headRefName` contains the issue number, that is your work from a prior session; resume it instead of creating a new branch.

## PR Title, Body, and Label Sync (MANDATORY)

On every agent run, for every PR being interacted with:

1. Ensure the **title** accurately reflects all changes in the PR; update it if the scope has changed.
2. Ensure the **body** summarises all changes and includes `Closes #<n>` for each linked issue, if any.
3. Sync labels from all linked closing issues to the PR:

   ```bash
   gh pr view <pr> --repo <owner/repo> --json closingIssuesReferences \
     --jq '.closingIssuesReferences[].number' \
   | while IFS= read -r n; do
       gh issue view "$n" --repo <owner/repo> --json labels --jq '.labels[].name' \
         || echo "Warning: could not fetch labels for issue $n" >&2
     done \
   | sort -u \
   | grep -vE '^(Blocked|On-Hold)$' \
   | while IFS= read -r label; do
       gh pr edit <pr> --repo <owner/repo> --add-label "$label" \
         || echo "Warning: could not add label '$label' to PR" >&2
     done
   ```

   The `Blocked` and `On-Hold` labels are explicitly excluded; workflow-control labels must never be synced from an issue to its PR.

4. Never remove any label from a PR or issue; GitHub workflows add labels automatically and they must not be removed.

## Label Management (MANDATORY)

- Always use `--add-label` when adding labels; **never** `--label`, which replaces all existing labels and destroys automatically-applied classification labels. See [github-cli.instructions.md](github-cli.instructions.md#labels) for command syntax.
- Never remove labels from issues or PRs. GitHub workflows add classification labels automatically; removing them breaks automation.

## Missing CLI Tools (MANDATORY)

If a required CLI tool is not found, **stop immediately and ask the user to install it**. Never:

- Search for the binary in alternative locations
- Manipulate PATH to try to find it
- Attempt to install it without being asked

**Exception: pre-commit hook tools:** Do not assume a tool is missing because `command -v` returns nothing in the current shell. Instead, follow the verification steps in [git.instructions.md](git.instructions.md); stage your changes and run the hook directly. Only block if it actually fails.

## Rules Compliance for In-Flight Work

Whenever an instruction file is added or updated, re-evaluate all open branches and PRs against the new rules. Fix any non-compliance before continuing; treat it the same as a CI failure.

## Instruction File Source Routing

- For changes to shared global instruction files (`ai/global/**`), raise an issue in `credfeto/cs-template`; it is the canonical source for those files.
- For changes specific to FunFair server projects, raise an issue in `funfair-tech/funfair-server-template` instead.
- Otherwise, make the change directly in the current repository.

## Large Multi-Handler / Multi-App Tasks

1. Create a top-level GitHub issue (if none specified); assign it; include the full original prompt as the body.
2. Comment findings on the issue before starting (handlers found, current state, etc.).
3. For each handler/app/component, create a sub-issue referencing the top-level issue; use the sub-issue number in branch names and commit messages.
4. Work on one handler/component at a time; commit and push before starting the next.
5. Close the sub-issue as soon as the relevant commits are pushed.

## Issue Tracking

- Only update issues if `gh` is installed and authenticated (`gh auth status`); otherwise read code and git log for state.
- Each sub-issue must list files with status: `❌ Not started` / `🔄 In progress` / `✅ Done`; update after each commit+push.
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

For complex files, commit+push+update after each round; do not wait until fully complete.

> Pre-commit hooks may make commits slow; wait for them to complete before assuming failure.

## Background Tasks and Monitor Tool (MANDATORY)

When using the Monitor tool to watch a background Bash task, the poll condition in the `until` loop **must** be provably satisfiable; a condition that can never be met loops forever and blocks the entire session.

### Rules for poll conditions

1. **Never poll for `"exit code"`**; that string is not reliably written to background task output files. Poll for a specific string the command itself writes (see table below).

2. **Do not pipe after `grep -q` in a negation check.** `! grep -q "pattern" file | tail -1` does NOT detect absence; the pipe applies to grep's (empty) stdout, so `tail -1` exits 0 regardless, and `!` inverts that to always-false. Write `! grep -q "pattern" file` with no trailing pipe.

3. **Verify the poll string exists in real output before writing the loop.** If you cannot confirm what string the command writes, run the command in the foreground first and read its output.

4. **Prefer foreground over background for bounded work.** Use `run_in_background: true` only when a command genuinely takes many minutes (e.g. a full integration-test run) and you have independent work to do while waiting. A `dotnet build` or `git commit` should always run in the foreground.

5. **Time-box every poll loop: die after 30 minutes.** Always include a deadline so the session cannot hang forever:

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

   Use `gh pr edit` / `gh pr comment` instead if the work item is a PR. Then exit; do not continue work.

### Reliable poll strings by command

| Command / scenario | String to poll for |
| --- | --- |
| `dotnet build` succeeded | `Build succeeded.` |
| `dotnet test` all passed | `Passed!` |
| pre-commit hooks passed | `→ All checks passed.` |
| pre-commit hooks failed | `→` followed by `Failed` (check for both to distinguish pass/fail) |
| `git push` completed | `branch` (branch tracking line in push output) |
| `gh pr create` / `gh pr ready` | poll not needed: these exit immediately |

## Never Truncate Test/Commit Commands (MANDATORY)

This is a distinct concern from the poll-loop timeouts above: those govern how long you wait for something _else_ to finish; this governs the timeout on the command _actually doing the work_.

Never pass a tool-level timeout that could truncate `pre-commit`, a `git commit` in a repo with pre-commit enabled (globally via `core.hooksPath` or locally via `.pre-commit-config.yaml`), `dotnet test`, `npm test`, or `bun test`; these must run to completion.

- Pass the maximum available timeout rather than a short one (e.g. Claude Code's Bash tool supports up to 600000ms/10 minutes; use it, don't default to a shorter value).
- If even the maximum available timeout is not enough, use `run_in_background` and the Monitor tool instead of accepting a truncated run.
- A killed run does not just fail; it skips the target process's own cleanup (a bash `EXIT` trap, .NET's `IDisposable` teardown, etc.), leaving orphaned temp directories, lock files, or half-applied state behind. Confirmed in practice: a killed `bats` run left thousands of orphaned fixture directories under a shared runtime directory, which went on to break an unrelated tool (`firejail`) that walked the same path.

### Sandbox-Caused False Timeouts in Benchmark/Perf Tests (MANDATORY)

If a `dotnet test`/`dotnet build` run that includes a benchmark or performance-test project fails with a timeout-shaped error (e.g. "configured timeout ... reached", "command took longer than the timeout", "Failed to set up high priority (Permission denied)"), do not conclude this is a genuine pre-existing/environmental limitation in the codebase before ruling out your own execution sandbox as the cause:

1. Re-run the identical command with sandboxing disabled if your tool supports it (e.g. a `dangerouslyDisableSandbox`-style flag).
2. Reproducing the same failure on a clean `main`/base branch does **not** rule out the sandbox; if you're still running inside the same sandboxed shell, that reproduction is confounded and proves nothing about the codebase itself.
3. If the failure disappears or measurably improves with sandboxing disabled, the sandbox was throttling CPU/resources; report this plainly; do not describe the benchmark suite as broken or flaky.
4. If still uncertain after disabling sandboxing, say so explicitly and ask the user to run the identical command in their own terminal before asserting any diagnosis; never present a sandbox artifact as a confirmed pre-existing bug.

## Multi-Agent Implementation and Review Pattern

### Model Selection

| Use full model | Use lesser model |
| --- | --- |
| Orchestrator, Code Writer, Code Reviewer, Code Fixer, Coding Researcher, CI Debugger, Dependency Updater | Code Tester, Committer, Changelog, Rebase Agent, PR Submitter, CI Monitor |

### Failure Handling: No Self-Repair

Mechanical agents must not interpret or fix failures. When a check fails: capture the full output, stop immediately, and return failure details verbatim to the calling agent.

### Routing Rules

Standard loop pattern: Code Writer/Fixer loops ≤5 with Code Tester; Code Reviewer loops ≤5 re-running both each round. Code Writer, Code Fixer, Code Reviewer, and CI Debugger may invoke Coding Researcher on demand at any point when the knowledge to implement or fix is lacking; this does not count toward the standard loop limits, but each calling role may invoke Coding Researcher at most 3 times per work item. Before invoking, the calling role checks the work item's issue/PR for an existing `### Coding Researcher` comment answering the same question and reuses it if found; reused findings do not count toward the cap. After Coding Researcher returns, the calling role records the question and outcome as a `### Coding Researcher` comment on the issue/PR so it can be reused. On reaching the cap, or if Coding Researcher returns **Not possible**, the calling role stops and escalates to Orchestrator rather than continuing the loop or guessing.

Every sequence below starts with the [Pre-Work Baseline Check](git.instructions.md#pre-work-baseline-check-mandatory-before-starting-any-work); it is step 0, not merely a standalone rule, and must actually run before the first agent in the row is invoked.

| Work type | Agent sequence |
| --- | --- |
| New feature / bug fix / refactor | Pre-Work Baseline Check → Code Writer → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| `CHANGES_REQUESTED` on existing PR, or verbal/chat request for changes on an open PR | Pre-Work Baseline Check → Code Fixer (respond to every comment) → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| Coverage-only task | Pre-Work Baseline Check → Code Writer (tests only) → Code Tester → Code Reviewer → Changelog → Committer → PR Submitter → CI Monitor |
| Documentation-only | Pre-Work Baseline Check → Code Writer (docs only) → PR Submitter |
| Rebase requested | Pre-Work Baseline Check → Rebase Agent → PR Submitter |
| CI failure (unknown cause) | Pre-Work Baseline Check → CI Debugger |
| Dependabot / dependency update | Pre-Work Baseline Check → Dependency Updater |

For detailed agent role definitions, see [agent-roles.instructions.md](agent-roles.instructions.md).

## Resuming Interrupted Work

- Check the status of existing issues and branches; skip merged branches.
- For unmerged branches, decide whether to continue or delete and recreate.
- Update the top-level issue with current status and next steps before resuming.
