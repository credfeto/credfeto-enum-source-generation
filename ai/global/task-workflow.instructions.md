# Task Workflow Instructions

> Always loaded.

[Back to Global Instructions Index](index.md)

## Numbering and Cross-Reference Conventions (MANDATORY)

Applies everywhere a list of this kind is produced: in instruction files, and in an issue/PR comment or live chat (e.g. an `## Implementation Plan` comment, per [Ad-Hoc Prompt Intake](#ad-hoc-prompt-intake-mandatory) below).

- **Assumptions**: a lower-case alpha sequence — `a.`, `b.`, `c.`, ...
- **Open questions**: a `Q`-prefixed numbered sequence — `Q1.`, `Q2.`, `Q3.`, ...
- **Plan/procedure steps**: a `P`-prefixed sequence — `P1.`, `P2.`, `P3.`, ... **Never `P0`.** If a list would otherwise need a zero-indexed step, renumber the whole list to start at `P1` and update every reference to the shifted numbers.
- **Encoding in committed Markdown files**: write `P`/`Q`/alpha steps as bullets with a bold label, not as literal ordered-list markers — `- **P1.** text`, not `1. text` or `P1. text`. A literal `1.`/`P1.` marker is parsed as a new list item by CommonMark, which detaches any nested bullets, fenced code blocks, or continuation paragraphs that were children of the previous item; the bullet form keeps them nested (indent nested content 2 spaces under a `-` marker, not 3). This does not apply to prose in live chat or an issue/PR comment, where plain `P1.`/`Q1.`/`a.` text is fine.

### Named Anchors for Cross-Referenced Steps

Never reference a step by its number from **another list or file** — a plain "step 2" or "item 4" breaks silently the next time that list is renumbered, and the reference lives far enough from its target that an editor renumbering one won't think to check the other. Instead, give the target step a named, invisible HTML anchor and link to it:

```markdown
- **P4.** <a id="phase-b-convergence"></a>Otherwise, judge convergence yourself from the PR's history of prior code-review comments...
```

Place the `<a id="...">` tag inline at the very start of the item's own text, never on its own line — a bare HTML block between list items terminates the list under CommonMark. Name the anchor after the step's content (`phase-b-convergence`), not its position (`phase-b-p4`), so the link survives future renumbering. Reference it from elsewhere as a normal Markdown link: `[Phase B's P4](agent-roles.instructions.md#phase-b-convergence)`. This repo's `.markdownlint.json` allows `<a>` via an MD033 override for exactly this purpose.

A step referring to a **sibling step within its own list** (e.g. "return to P2", "once P4 is clean, go to P5") does not need an anchor: renumbering that list is a single, self-contained edit, and its own internal references get fixed as part of the same edit — there's no separate file or list left stale. Only add an anchor once the reference crosses to different content that could be edited independently.

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

When selecting the next issue to work on, prefer issues with higher-priority labels. Skip any issue labelled `On Hold` or `Blocked`.

### Status Labels

| Label | Meaning |
| --- | --- |
| `On Hold` | Needs further thought or cannot be implemented yet; do not start work |
| `Blocked` | Needs human input before work can continue; see the Orchestrator section in [agent-roles.instructions.md](agent-roles.instructions.md) |

## Workflow Project Board (MANDATORY)

Every issue raised, in any repository and via any flow (deliverable issues, ad-hoc intake tracking issues, AI-initiated issues, sub-issues), must be added to the "Workflow" GitHub project linked to that repository, immediately after creation.

Add the issue with the `cfwf` command in [github-cli.instructions.md](github-cli.instructions.md#adding-an-issue-to-the-workflow-project).

## GitHub Issue Creation (MANDATORY)

When asked to create or update a GitHub issue (i.e. the issue itself is the requested deliverable):

- **P1.** Enter Plan Mode.
- **P2.** Work out at a high level what code change the issue would represent: scope, affected files, approach.
- **P3.** Exit Plan Mode and return to auto.
- **P4.** Create the issue, or update the existing issue, using the plan output to write a meaningful description.

This is distinct from [Ad-Hoc Prompt Intake](#ad-hoc-prompt-intake-mandatory) below; that section covers being asked to _do_ something, where the issue is a tracking side-effect rather than the deliverable itself.

## Ad-Hoc Prompt Intake (MANDATORY)

Applies whenever a human asks you to _do_ something in the context of a repo (a task, not a request to raise an issue, which is covered above), and no existing issue or PR has already been specified as the thing to work on. No exception for triviality of the request, and no exception for the `credfeto/cs-template` repo itself.

- **P1.** Before taking any other action (including answering a read-only question), create a GitHub issue in the current repo:
  - Title: a concise summary of the prompt.
  - Body: the prompt, verbatim, as the starting point.
  - Labels: `AI-Work` and `Blocked` (minimum), always, regardless of who initiated the underlying task; add other relevant labels (e.g. priority) as appropriate.
- **P2.** Use Plan Mode to work out scope, affected files, and approach, and post it as an `## Implementation Plan` issue comment per the format in [agent-roles.instructions.md](agent-roles.instructions.md#issue-workflow-plan-first-new-issues-only).
- **P3.** As open questions are identified, add each as an issue comment as soon as it's identified; do not batch them all until the end.
- **P4.** Do not proceed until an explicit human approval comment exists (`approved` / `lgtm`) and `Blocked` is removed; if approval came via live chat, mirror it as a GitHub comment first (see [Blocked Label](agent-roles.instructions.md#blocked-label)). In an interactive session, see [Waiting for Approval in an Interactive Session](agent-roles.instructions.md#waiting-for-approval-in-an-interactive-session), which applies to these tracking issues too.
- **P5.** Once approved and `Blocked` is removed:
  - If the request needs a code change, proceed via the routing table below and open a PR referencing the issue when ready.
  - If the request is read-only/informational (no code change), post the answer as an issue comment and close the issue.
- **P6.** See [Prompt Traceability](#prompt-traceability-mandatory) below for further prompts once an issue or PR already exists for the work.

## Prompt Traceability (MANDATORY)

Once a request is already tracked by an issue or PR (including one just created under [Ad-Hoc Prompt Intake](#ad-hoc-prompt-intake-mandatory) above), every subsequent prompt from the human that changes, redirects, or adds detail to that work must be recorded on that issue/PR:

- Comment with the prompt (verbatim, or a faithful summary for long prompts) and how it was resolved: a code change, an answered question, a scope adjustment, etc.
- Post this before or immediately after acting on the prompt; do not let several prompts accumulate unrecorded.
- This applies whether the prompt arrived as a live chat message or as a GitHub comment (GitHub comments are already covered by [Comment Replies](agent-roles.instructions.md#comment-replies-mandatory)).

## Correcting a Prior Claim (MANDATORY)

If a factual claim or finding you previously posted in an issue/PR body or comment turns out to be wrong (e.g. a root-cause statement, an evidence point, a "this is a deviation from process" assertion), post a new comment stating the correction and briefly why, quoting or referencing the original claim being corrected. Editing the body to also fix it is fine, but the comment is the mandatory part: a silent in-place body edit is not sufficient on its own, because GitHub only surfaces it as a small "edited" marker that a human reviewer can easily miss, unlike a comment which appears in the normal timeline.

This is distinct from [PR Title, Body, and Label Sync](#pr-title-body-and-label-sync-mandatory) below, which requires routine in-place edits to keep a PR's title/body/labels synced with its linked issues; that is not a correction and needs no comment. This rule is about retracting or fixing something substantive that was previously asserted as true.

## Pre-Closure Decision Check (MANDATORY)

Before closing any issue or PR, check whether its Implementation Plan (Approach/Files-to-change text or an Open Question) or a later comment on it flagged a specific decision as required or pending (e.g. "needs policy sign-off", "pending a decision on X", an unresolved `Qn.`). If so, do not close until that specific item has a visible resolution of its own — a comment recording the decision, a link to the resolving issue/PR, or an explicit retraction — not just implicitly overtaken by whichever branch of the plan got implemented.

This is stricter than an unresolved `Qn.` alone: an Open Question already blocks via the Blocked-label approval gate ([Issue Workflow: Plan First](agent-roles.instructions.md#issue-workflow-plan-first-new-issues-only)), and removal of that label is not itself sufficient evidence this check is satisfied — the check here is that the resolution was actually posted, not merely that the item is otherwise ready to close. Applies equally to issues and PRs, including the closing points in [PR Workflow: AI Review Loop Phase E](agent-roles.instructions.md#phase-e-mark-ready) and [Large Multi-Handler / Multi-App Tasks P5](#large-multi-handler--multi-app-tasks).

## PR Lifecycle

- Only one active branch or open PR **per user** per repository at a time; do not create another until the current one is merged and closed.
- **Before blocking new work** because of an existing PR: always verify its current state with `gh pr view <number> --repo <owner/repo> --json state,mergedAt`; never rely on conversation memory. A PR that was open earlier in the session may have since been merged.
- When adding work to an open PR (review comments, missing coverage, CI fixes), convert to draft first: `gh pr ready <number> --undo`. Keep it in draft until Code Tester and Code Reviewer are both satisfied; only PR Submitter converts it back.

## Bot-Created PRs (MANDATORY, treat as your own)

github is configured to automatically create PRs from pushed branches. These PRs appear authored by `app/github-actions` but the commits are authored by you. **They are your work; treat them identically to PRs you created yourself.**

**Before starting any work in a repository:**

- **P1.** Run `gh pr list --state open --repo <owner/repo> --json number,title,author,headRefName,url`, no `--author @me` filter.
- **P2.** For any PR authored by `app/github-actions`, check the commit authors: `gh pr view <n> --repo <owner/repo> --json commits --jq '.commits[].authors[].login'`.
- **P3.** If **all commits** are from your account (you are the sole committer), **take ownership**: update the PR title and body to match the proper format (summary, `Closes #<n>`, test plan), add yourself as assignee, and treat it as your active PR for that repo.
- **P4.** If commits are from multiple authors (e.g. you plus a human or Copilot), do **not** take over; leave the PR as-is and do not claim it as yours.
- **P5.** Do **not** create a new branch or PR for the same issue; that would be duplicate work.

**When you find a duplicate pair** (a bot-created PR and one you authored yourself, for the same issue or branch):

- Keep whichever has the more complete body and later review activity.
- Close the other with a comment explaining which PR supersedes it.

**Checking for existing work before branching (MANDATORY):**

- Check branch names in all open PRs, not just PR authors. If any open PR's `headRefName` contains the issue number, that is your work from a prior session; resume it instead of creating a new branch.
- This only catches work that already has a PR open. A branch pushed but never turned into a PR (session died first) needs the separate check in [git.instructions.md's Branching section](git.instructions.md#branching).

## PR Title, Body, and Label Sync (MANDATORY)

On every agent run, for every PR being interacted with:

- **P1.** Ensure the **title** accurately reflects all changes in the PR; update it if the scope has changed.
- **P2.** Ensure the **body** summarises all changes and includes `Closes #<n>` for each linked issue, if any.
- **P3.** Sync labels from all linked closing issues to the PR:

  ```bash
  cfwf closing-issue-labels --repo <owner/repo> --pr <pr>
  gh pr edit <pr> --repo <owner/repo> --add-label "<label-1>,<label-2>"
  ```

  `cfwf closing-issue-labels` prints the labels to sync, one per line, already leaving out `Blocked` and `On Hold` (workflow-control labels are never synced from an issue to its PR). Pass them to one `gh pr edit --add-label` as a comma-separated list. A non-zero exit is a failure to report, not "no labels"; if it exits 0 and prints nothing, there is nothing to add. If the `gh pr edit` call fails because a label does not exist in the PR's repo, repeat it without that label.

- **P4.** Never remove any label from a PR or issue; GitHub workflows add labels automatically and they must not be removed. Sole exception: `Blocked` on live-chat plan approval, see [Waiting for Approval in an Interactive Session](agent-roles.instructions.md#waiting-for-approval-in-an-interactive-session) P5.

## Label Management (MANDATORY)

- Always use `--add-label` when adding labels; **never** `--label`, which replaces all existing labels and destroys automatically-applied classification labels. See [github-cli.instructions.md](github-cli.instructions.md#labels) for command syntax.
- Never remove labels from issues or PRs. GitHub workflows add classification labels automatically; removing them breaks automation. Sole exception: `Blocked` on live-chat plan approval, see [Waiting for Approval in an Interactive Session](agent-roles.instructions.md#waiting-for-approval-in-an-interactive-session) P5.

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

- **P1.** Create a top-level GitHub issue (if none specified); assign it; include the full original prompt as the body.
- **P2.** Comment findings on the issue before starting (handlers found, current state, etc.).
- **P3.** For each handler/app/component, create a sub-issue referencing the top-level issue; use the sub-issue number in branch names and commit messages.
- **P4.** Work on one handler/component at a time; commit and push before starting the next.
- **P5.** Close the sub-issue as soon as the relevant commits are pushed.

## Issue Tracking

- Only update issues if `gh` is installed and authenticated (`gh auth status`); otherwise read code and git log for state.
- Each sub-issue must list files with status: `❌ Not started` / `🔄 In progress` / `✅ Done`; update after each commit+push.
- The top-level issue tracks only handler/app-level status (sub-issues open/closed, branches merged).
- Update the sub-issue after each significant commit+push; update the top-level issue when overall status changes.
- When resuming, update the issue with current state before continuing.

## Commit, Push, and Issue Update Cadence

- One logical change per commit; do not batch unrelated changes.

Per-file cadence for coverage tasks:

- **P1.** Write tests until the file reaches target coverage.
- **P2.** Commit the test file; push immediately.
- **P3.** Update the sub-issue to mark the file done.
- **P4.** Move to the next file.

For complex files, commit+push+update after each round; do not wait until fully complete.

> Pre-commit hooks may make commits slow; wait for them to complete before assuming failure.

## Background Tasks and Monitor Tool (MANDATORY)

This section covers polling a command that was **accepted** and is running; if a command was
instead **denied** by a `PreToolUse` hook, it never started at all and there is nothing to poll
for, see [claude-hooks.instructions.md](claude-hooks.instructions.md) for that case.

When using the Monitor tool to watch a background Bash task, the poll condition in the `until` loop **must** be provably satisfiable; a condition that can never be met loops forever and blocks the entire session.

### Rules for poll conditions

- **P1.** **Never poll for `"exit code"`**; that string is not reliably written to background task output files. Poll for a specific string the command itself writes (see table below).

- **P2.** **Do not pipe after `grep -q` in a negation check.** `! grep -q "pattern" file | tail -1` does NOT detect absence; the pipe applies to grep's (empty) stdout, so `tail -1` exits 0 regardless, and `!` inverts that to always-false. Write `! grep -q "pattern" file` with no trailing pipe.

- **P3.** **Verify the poll string exists in real output before writing the loop.** If you cannot confirm what string the command writes, run the command in the foreground first and read its output.

- **P4.** **Prefer foreground for quick, bounded commands** (`git status`, a single `grep`, `ls`, and similar). **Always background project build/test/commit tooling instead** — `git commit`/`pre-commit`/`pre-commit-check`, `dotnet build`, `dotnet test`, `npm test`, `bun test` — regardless of how fast a specific run is expected to be; see [Never Truncate Test/Commit Commands](#never-truncate-testcommit-commands-mandatory) below for why and how. Use `run_in_background: true` for any other command that genuinely takes many minutes (e.g. a full integration-test run) and you have independent work to do while waiting.

- **P5.** **Time-box every poll loop: die after 30 minutes.** Always include a deadline so the session cannot hang forever:

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

This is a distinct concern from the poll-loop timeouts above: those govern how long you wait for something _else_ to finish; this governs the timeout on the command _actually doing the work_ (a call rejected outright for missing `run_in_background: true` never ran; see [claude-hooks.instructions.md](claude-hooks.instructions.md) for that case, not here).

`git commit`/`pre-commit`/`pre-commit-check`, `dotnet build`, `dotnet test`, `npm test`, and `bun test` have no bounded, predictable duration: `pre-commit` (and `pre-commit-check`, this template's wrapper that runs it against the existing checked-out repo) can run a heavy hook chain (`dotnet buildcheck` across every project, `trivy`, `hadolint`), `dotnet build` runs through a large analyzer stack (Roslynator, SonarAnalyzer, Meziantou, Threading, Security Code Scan, and more) plus NuGet restore, and test runs scale with what changed. A commit has already been killed mid-run on a foreground timeout in a live session. There is no timeout value that is both practical and safe to pick for any of these commands, so do not try to pick one.

- **Always run these commands via `run_in_background`; never in the foreground, regardless of how fast the specific run is expected to be.** This is unconditional, not a per-invocation judgement call.
- **Never wrap any of these in a shell `timeout` command as a substitute or a belt-and-braces addition** (e.g. `timeout 590 dotnet test ...`), whether or not `run_in_background: true` is also set. `timeout` is on `reject-obfuscated-commands`' categorical blocklist (see [claude-hooks.instructions.md](claude-hooks.instructions.md#reference-installed-hook-set)) and is rejected outright, independently of the backgrounding rule above — plain `run_in_background: true` on the unwrapped command is already unbounded and needs no additional wrapper. A missing `run_in_background` and a banned `timeout` wrapper are denied by different hooks and are unrelated; read which hook actually fired rather than assuming a single general conflict.
- Poll with the Monitor tool using the strings in the [Reliable poll strings by command](#reliable-poll-strings-by-command) table above, subject to the same 30-minute deadline required by [Background Tasks and Monitor Tool](#background-tasks-and-monitor-tool-mandatory). Backgrounding these commands is not exempt from that deadline; it trades a short, unsafe foreground guess for a long, controlled one.
- **A long stretch with no new output is normal and is not a hang.** Do not interpret silence as a failure and manually cancel or kill the command on that basis. The only valid reasons to stop waiting are: the tool itself reports its timeout was hit, or the poll-loop deadline actually fires.
- A killed run does not just fail; it skips the target process's own cleanup (a bash `EXIT` trap, .NET's `IDisposable` teardown, etc.), leaving orphaned temp directories, lock files, or half-applied state behind. Confirmed in practice: a killed `bats` run left thousands of orphaned fixture directories under a shared runtime directory, which went on to break an unrelated tool (`firejail`) that walked the same path; a `git commit` has separately been killed mid-run on a foreground timeout.
- Other `dotnet` commands (`dotnet restore`, a standalone `dotnet buildcheck`, `dotnet format`, etc.) are not covered by this section; they may run in the foreground, but **always with an explicit maximum timeout set on the tool call**, never the tool's built-in default (e.g. Claude Code's Bash tool defaults to 2 minutes when no `timeout` is given; use the maximum available, e.g. 600000ms/10 minutes, explicitly). If even that maximum is not enough, use `run_in_background` and the Monitor tool instead of accepting a truncated run.

### Sandbox-Caused False Timeouts in Benchmark/Perf Tests (MANDATORY)

If a `dotnet test`/`dotnet build` run that includes a benchmark or performance-test project fails with a timeout-shaped error (e.g. "configured timeout ... reached", "command took longer than the timeout", "Failed to set up high priority (Permission denied)"), do not conclude this is a genuine pre-existing/environmental limitation in the codebase before ruling out your own execution sandbox as the cause:

- **P1.** Re-run the identical command with sandboxing disabled if your tool supports it (e.g. a `dangerouslyDisableSandbox`-style flag).
- **P2.** Reproducing the same failure on a clean `main`/base branch does **not** rule out the sandbox; if you're still running inside the same sandboxed shell, that reproduction is confounded and proves nothing about the codebase itself.
- **P3.** If the failure disappears or measurably improves with sandboxing disabled, the sandbox was throttling CPU/resources; report this plainly; do not describe the benchmark suite as broken or flaky.
- **P4.** If still uncertain after disabling sandboxing, say so explicitly and ask the user to run the identical command in their own terminal before asserting any diagnosis; never present a sandbox artifact as a confirmed pre-existing bug.

## Multi-Agent Implementation and Review Pattern

### Model Selection

| Use full model | Use lesser model |
| --- | --- |
| Orchestrator, Code Writer, Code Reviewer, Code Fixer, Coding Researcher, CI Debugger, Dependency Updater | Code Tester, Committer, Changelog, Rebase Agent, PR Submitter, CI Monitor |

### Failure Handling: No Self-Repair

Mechanical agents must not interpret or fix failures. When a check fails: capture the full output, stop immediately, and return failure details verbatim to the calling agent.

### Routing Rules

Standard loop pattern: Code Writer/Fixer loops ≤5 with Code Tester; Code Reviewer loops ≤5 re-running both each round. Code Writer, Code Fixer, Code Reviewer, and CI Debugger may invoke Coding Researcher on demand at any point when the knowledge to implement or fix is lacking; this does not count toward the standard loop limits, but each calling role may invoke Coding Researcher at most 3 times per work item. Before invoking, the calling role checks the work item's issue/PR for an existing `### Coding Researcher` comment answering the same question and reuses it if found; reused findings do not count toward the cap. After Coding Researcher returns, the calling role records the question and outcome as a `### Coding Researcher` comment on the issue/PR so it can be reused. On reaching the cap, or if Coding Researcher returns **Not possible**, the calling role stops and escalates to Orchestrator rather than continuing the loop or guessing.

Every sequence below starts with the [Pre-Work Baseline Check](git.instructions.md#pre-work-baseline-check-mandatory-before-starting-any-work); it is an implicit first step of every sequence, not merely a standalone rule, and must actually run before the first agent in the row is invoked.

| Work type | Agent sequence |
| --- | --- |
| New feature / bug fix / refactor | Pre-Work Baseline Check → Changelog (placeholder) → Committer → PR Submitter → Code Writer → Code Tester → Code Reviewer → Changelog (correction) → Committer → PR Submitter → CI Monitor |
| `CHANGES_REQUESTED` on existing PR, or verbal/chat request for changes on an open PR | Pre-Work Baseline Check → Code Fixer (respond to every comment) → Code Tester → Code Reviewer → Changelog (correction) → Committer → PR Submitter → CI Monitor |
| Coverage-only task | Pre-Work Baseline Check → Changelog (placeholder) → Committer → PR Submitter → Code Writer (tests only) → Code Tester → Code Reviewer → Changelog (correction) → Committer → PR Submitter → CI Monitor |
| Documentation-only | Pre-Work Baseline Check → Changelog (placeholder) → Committer → PR Submitter → Code Writer (docs only) → Changelog (correction) → Committer → PR Submitter |
| Rebase requested | Pre-Work Baseline Check → Rebase Agent → PR Submitter |
| CI failure (unknown cause) | Pre-Work Baseline Check → CI Debugger |
| Dependabot / dependency update | Pre-Work Baseline Check → Dependency Updater |

Rows starting with `Changelog (placeholder)` assume the work item takes a changelog entry at all. If it hits the skip condition in [changelog.instructions.md](changelog.instructions.md#when-to-skip) (template repo), the row runs unchanged — see [agent-roles.instructions.md](agent-roles.instructions.md#changelog) for what the Changelog agent commits instead.

For detailed agent role definitions, see [agent-roles.instructions.md](agent-roles.instructions.md).

## Resuming Interrupted Work

- Check the status of existing issues and branches; skip merged branches.
- For unmerged branches, decide whether to continue or delete and recreate.
- Update the top-level issue with current status and next steps before resuming.
