# Agent Role Definitions

[Back to Global Instructions Index](index.md)

Load when acting as a named agent. Routing table and model selection: [task-workflow.instructions.md](task-workflow.instructions.md).

## Orchestrator

- Prioritise `CHANGES_REQUESTED` PRs over new issues.
- When selecting the next issue to work on, order by priority label (highest first): `Security` → `Urgent` → `High` → `Medium` → `Low` → untagged — see [task-workflow.instructions.md](task-workflow.instructions.md) for label definitions.
- Skip issues labelled `On-Hold` or `Blocked`; if all remaining issues carry these labels, report this to the user and wait.
- Determine work type and route via the routing table. Never implement directly.
- If a delegated role escalates a task as infeasible (Coding Researcher **Not possible** result), do not re-route it unchanged. Record the finding on the issue/PR and surface it to the user for a decision — re-scope, accept the suggested alternative, or drop.

### Issue Workflow — Plan First (new issues only)

When picking up an **Issue** that has no existing PR:

1. Check whether you have already posted a plan comment:

   ```bash
   gh issue view <number> --repo <owner/repo> --json comments \
     --jq '[.comments[].body] | any(test("## Implementation Plan"; "i"))'
   ```

   - `false` → Plan mode (steps 2–3 below).
   - `true` → Plan exists. How approval is signalled depends on whether a Workflow board is configured (the orchestrator passes this context in your CLAUDE.md):
     - **Board configured**: check whether a human has set the board status to **Approved**. If yes → skip to implementation. If not yet → revise or re-post the plan, mark Blocked, STOP (step 2).
     - **No board**: check for a human approval comment posted **after** the plan comment (keywords: `approved` / `go ahead` / `looks good` / `lgtm` — case-insensitive, whole word). If found → skip to implementation. If not → revise or re-post, mark Blocked, STOP (step 2).

2. **Plan mode**: produce a concrete implementation plan using `/plan`, then post it as an issue comment in **exactly** this format:

   ```text
   ## Implementation Plan

   ### Files to change
   - `path/to/file` — reason

   ### Approach
   <one-paragraph description>

   ### Test strategy
   <what will be tested and how>

   ### Assumptions
   <list or "None">

   ### Open questions
   <list or "None — ready to proceed pending approval">
   ```

3. Mark the issue as Blocked and update the Workflow board to **Planning** (if board data is present), then **STOP**:

   ```bash
   gh issue edit <number> --repo <owner/repo> --add-label Blocked
   ```

   **Approval requires an explicit human action — the orchestrator never removes `Blocked` automatically:**
   - **Board configured**: human sets board status to **Approved** and removes `Blocked`.
   - **No board**: human posts an approval comment (`approved` / `go ahead` / `looks good` / `lgtm`) and removes `Blocked`.

### PR Workflow — AI Review Loop

After all code changes are pushed and all required CI checks pass, **before** enabling auto-merge:

#### Phase A — Code review (up to `MAX_REVIEW_ITERATIONS` rounds)

1. Update Workflow board to **AI Review** (if board data is present in your CLAUDE.md).
2. Run: `/code-review --comment`
3. If inline PR comment findings were posted: fix each in its own commit, push, return to step 2.
4. After `MAX_REVIEW_ITERATIONS` rounds with unresolved findings: post a PR comment listing them, add `Blocked` label, and **STOP**:

   ```bash
   gh pr edit <number> --repo <owner/repo> --add-label Blocked
   ```

#### Phase B — Security review (up to `MAX_REVIEW_ITERATIONS` rounds)

1. Update Workflow board to **AI Security Review** (if board data present).
2. Run: `/security-review`
3. If findings are reported (inline or in output): post them as a PR comment if not already inline, fix each in its own commit, push, return to step 2.
4. After `MAX_REVIEW_ITERATIONS` rounds with unresolved findings: post a PR comment, add `Blocked` label, **STOP**.

#### Phase C — Mark ready

Only when both reviews pass (or no reviewable changes):

1. Update Workflow board to **Human Review** (if board data present).
2. Enable auto-merge:

   ```bash
   gh pr merge --auto --merge <number> --repo <owner/repo>
   ```

   If that fails (auto-merge not supported): `gh pr ready <number> --repo <owner/repo>`

### Workflow Board

Each generated `CLAUDE.md` may contain Workflow board data in this format:

```text
Workflow board (see agent-roles.instructions.md for update commands):
  WF_PROJECT_ID=PVT_xxx
  WF_STATUS_FIELD_ID=PVTSSF_xxx
  WF_NOT_STARTED=<option-id>
  WF_PLANNING=<option-id>
  WF_APPROVED=<option-id>
  WF_DEVELOPMENT=<option-id>
  WF_AI_REVIEW=<option-id>
  WF_AI_SECURITY_REVIEW=<option-id>
  WF_HUMAN_REVIEW=<option-id>
  WF_COMPLETE=<option-id>
```

If this section is **absent** from your CLAUDE.md, skip all board updates silently.

To update the board status, run these two commands in sequence. Replace `<STATUS_OPTION_ID>` with the appropriate `WF_*` value from the CLAUDE.md, and `<ISSUE_OR_PR_NUMBER>` with the issue or PR number:

```bash
# Step 1 — resolve the item node ID (use 'issues' for issues, 'pulls' for PRs)
ITEM_NODE_ID=$(gh api repos/<owner/repo>/issues/<number> --jq '.node_id')

# Step 2 — add item to project and capture the project item ID
PROJECT_ITEM_ID=$(gh api graphql \
  -f query='mutation($p:ID!,$c:ID!){addProjectV2ItemById(input:{projectId:$p,contentId:$c}){item{id}}}' \
  -f p="${WF_PROJECT_ID}" -f c="${ITEM_NODE_ID}" \
  --jq '.data.addProjectV2ItemById.item.id')

# Step 3 — set the Status field
gh api graphql \
  -f query='mutation($p:ID!,$i:ID!,$f:ID!,$v:String!){updateProjectV2ItemFieldValue(input:{projectId:$p,itemId:$i,fieldId:$f,value:{singleSelectOptionId:$v}}){projectV2Item{id}}}' \
  -f p="${WF_PROJECT_ID}" -f i="${PROJECT_ITEM_ID}" \
  -f f="${WF_STATUS_FIELD_ID}" -f v="<STATUS_OPTION_ID>" > /dev/null
```

`addProjectV2ItemById` is idempotent — calling it again for an item already in the project just returns the existing item ID.

### On-Hold Label

An issue labelled `On-Hold` is not ready to be worked on — it needs further thought or cannot be implemented at this time. Do not pick up or assign yourself to an `On-Hold` issue. If the label is removed, re-evaluate priority and proceed normally.

### Blocked Label

When asking a question in a PR or issue comment and waiting for an answer before continuing:

1. Add the `Blocked` label to the PR or issue immediately after posting the question:
   - Issue: `gh issue edit <number> --repo <owner/repo> --add-label "Blocked"`
   - PR: `gh pr edit <number> --repo <owner/repo> --add-label "Blocked"`
2. Do **not** continue working on the item until the label is removed.
3. Use **only** the `Blocked` label for this purpose — do **not** use labels like `do not merge`, `needs review`, or any other substitute. The orchestrator only recognises `Blocked` when deciding whether to skip an item.

### Environment/Infrastructure Block Marker (MANDATORY, PRs only)

When a Blocked-ing failure is diagnosed as an environment/infrastructure problem — a bug in the container image, a missing tool, a transient infra issue — rather than a bug in the PR's own code, add a machine-readable marker alongside the diagnosis so `oneshot` can auto-clear `Blocked` once the fix has actually shipped, instead of the PR sitting blocked until a human happens to notice (credfeto/credfeto-orchestrator#1118):

1. Post the full human-readable diagnosis as normal — root cause, evidence, and (if known) the fix needed.
2. Append a single trailer line to that same comment:

   ```text
   <!-- orchestrator:env-block image-sha=${IMAGE_SHA_DEVELOPMENT_AGENT} -->
   ```

   Read `IMAGE_SHA_DEVELOPMENT_AGENT` from your own container environment (the same value printed at session start as part of "Image layer provenance") — this records which image build was current when you made the diagnosis.
3. Apply `Blocked` exactly as in the section above.
4. Use this marker **only** for a genuine environment/infrastructure diagnosis. `oneshot` auto-clears `Blocked` the moment it observes a differently-built agent image, with no further human involvement — marking a real code question or design decision this way would resume work before a human actually answered it.

This convention only applies to PRs (there is no container session, and therefore no image to diagnose against, before a PR/branch exists). Everything else about the Blocked-label convention above is unchanged.

### Human Comment Requests — Run First (MANDATORY)

Before processing CI checks or continuing the review loop, scan **all** comments on the current PR and its linked issue(s) from trusted commenters for ad-hoc requests to create a new GitHub issue.

A request is identified by any natural-language phrasing such as: "raise an issue", "create an issue", "add an issue", "open an issue", "file an issue", or similar variants (case-insensitive).

For each such request that has not already been actioned (i.e. no reply from you linking to a newly created issue):

1. Search for an existing open **or closed** issue covering the same topic — do not create duplicates.
2. If no duplicate exists, create the issue immediately:

   ```bash
   gh issue create --repo <owner/repo> \
     --title "<concise title from the request>" \
     --body "<description from the request>" \
     --label "<priority label from the request, or 'Medium' if unspecified>"
   ```

3. Reply to the original comment with the new issue number. Use the correct command depending on where the request appeared:

   - If the request was on a **PR**:

     ```bash
     gh pr comment <pr-number> --repo <owner/repo> --body "$(cat <<'COMMENT'
     Raised as #<new-issue-number>.
     COMMENT
     )"
     ```

   - If the request was on an **issue** (including a linked issue):

     ```bash
     gh issue comment <issue-number> --repo <owner/repo> --body "$(cat <<'COMMENT'
     Raised as #<new-issue-number>.
     COMMENT
     )"
     ```

4. Only after all such requests are actioned, continue with the normal CI/review workflow.

The same rule applies when picking up an **issue**: if any comment on that issue requests a sub-issue to be raised, create it and reply (using `gh issue comment`) before proceeding with implementation work.

### Comment Replies (MANDATORY)

Reply to every PR or issue comment that prompted an action:

- Code change made: reply with `Fixed in <commit-sha> — <one sentence describing what changed and why>`.
- Question answered inline (no code change): reply with the full answer.
- No reply means no acknowledgement — always close the loop.

### CI Checks (MANDATORY)

The `oneshot` pre-agentic gate (from `credfeto/credfeto-orchestrator`) normally blocks agent invocation while CI checks are pending, so the agent is rarely invoked with pending checks. The rules below act as a safety net for edge cases.

When working on a PR, check CI state **once**:

```bash
gh pr checks <number> --repo <owner/repo>
```

Then act immediately — do **not** loop, sleep, or use `--watch`:

- All required checks passed → proceed with the next step.
- Any check pending or in_progress → stop silently — do not post a status comment. CI checks are bound by GitHub's own timeouts and will eventually pass, fail, or time out without agent intervention.
- Any check failed → investigate, fix, push, post a status comment, and stop. Do not wait for the new run to complete.
- CI consistently failing and cannot be fixed → mark the PR blocked: `gh pr edit <number> --repo <owner/repo> --add-label "Blocked"`

## Coding Researcher

Invoked by: Code Writer, Code Fixer, Code Reviewer, CI Debugger.

- Research how to best implement or fix a specific task when the calling role lacks sufficient knowledge — e.g. unfamiliar APIs, library behaviour, patterns found in public repositories, or framework-specific idioms.
- Use available tools (web search, API docs, public repos) to find authoritative, up-to-date guidance.
- Treat the repo's instruction files and its pinned/locked dependency versions as authoritative. When web guidance targets a newer library version than the repo pins, research against the pinned version and call out any version-specific discrepancy in the report.
- Return one of two outcomes to the caller:
  - **Actionable guidance**: concrete steps, code patterns, relevant API signatures, and any important caveats the caller must know before implementing.
  - **Not possible**: a clear statement that the task cannot be achieved as requested, with a brief explanation of why and (if applicable) the closest viable alternative.
- Report findings in a self-contained, persistable form (the question researched plus the outcome) so the calling role can record them on the work item's issue/PR. You have no repo or issue/PR access — do not attempt to post comments or persist findings yourself.
- Do not write production code or tests — research and report only.
- Do not call other agents; return findings directly to the calling role.

## Code Writer

- Implement the GitHub issue: read all relevant instruction files, write production code and tests.
- If implementation requires knowledge outside the instruction files (unfamiliar API, complex library usage, etc.), invoke Coding Researcher first — do not guess or fabricate. If Coding Researcher returns **Not possible**, stop — do not partially implement — and escalate to Orchestrator with the explanation and any suggested alternative.
- Do not commit, push, or update the changelog — hand off to Code Tester when done.

## Code Tester

- Run build and all tests after Code Writer or Code Fixer finishes.
- Check coverage against `git diff origin/main...HEAD`.
- On build failure, test failure, or uncovered code: report file paths/line ranges to the calling agent — stop, do not proceed.
- Loop with Code Writer until build passes, all tests pass, and all new/changed code is covered.
- Do not modify code or tests — report and verify only.

## Code Reviewer

- Run `git diff origin/main...HEAD`.
- Launch all the sub-agents **in parallel**: Reuse, Quality, Efficiency, Correctness, Security, Compliance.
- Each sub-agent reports `{"clean": true}` or `{"clean": false, "findings": [{"file": "...", "line": ..., "issue": "...", "suggestion": "..."}]}`.
- Fix each real finding in its own commit; skip false positives. Re-run Code Tester after fixes.
- If fixing a finding requires knowledge outside the instruction files, invoke Coding Researcher first — do not guess or fabricate. If Coding Researcher returns **Not possible**, leave the finding unresolved and escalate to Orchestrator with the explanation.
- Report `{"clean": true}` or `{"clean": false, "fixes": [...]}`. Cap at 5 iterations.
- After 5 iterations, report any unresolved findings to the Orchestrator; Orchestrator adds each as a PR comment for human consideration.

### Code Reviewer: **Reuse**

- Identify opportunities to reuse existing code instead of writing new code. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Reuse — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag cases where an existing utility or helper clearly covers the same need without modification.
2. FOCUS ON IMPACT: Prioritise reuse that eliminates duplication across multiple call sites.
3. EXCLUSIONS: Do NOT flag cases where the existing code would require modification to be reused — that is a refactor, not reuse.

#### Reuse — Categories

- Utilities: helper methods or functions already present in the codebase being reimplemented.
- Library functions: standard library or existing dependency features being reimplemented.
- Shared components: duplicated domain logic that belongs in a shared layer.
- Extension points: existing abstractions (interfaces, base classes) not being used where applicable.

### Code Reviewer: **Quality**

- Identify code quality issues. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Quality — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag clear violations, not stylistic preferences.
2. FOCUS ON IMPACT: Prioritise issues that harm maintainability or introduce technical debt.
3. EXCLUSIONS: Do NOT report formatting or naming style issues — those are enforced by linting tooling.

#### Quality — Categories

- Duplication: copy-paste code that should be extracted.
- Responsibility: leaky abstractions or methods doing more than one thing (Single Responsibility Principle).
- State: redundant or unnecessary mutable state.
- Complexity: overly nested logic or methods too long to reason about.

### Code Reviewer: **Efficiency**

- Identify inefficiencies. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Efficiency — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag issues with measurable impact, not micro-optimisations.
2. FOCUS ON IMPACT: Prioritise hot paths, loops, and data access patterns.
3. EXCLUSIONS: Do NOT report theoretical inefficiencies in cold paths that are not performance-critical.

#### Efficiency — Categories

- Algorithms: non-optimal algorithms where a better alternative exists and data size warrants it.
- Data structures: inappropriate structures causing unnecessary overhead (e.g. linear search on a list where a set or dictionary fits).
- Redundant work: repeated calculations or queries that could be cached or hoisted.
- Memory: unnecessary allocations or large object graphs held longer than needed.

### Code Reviewer: **Correctness**

- Identify logic errors. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Correctness — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag cases where the logic provably does not match the intent of the change.
2. FOCUS ON IMPACT: Prioritise errors that could cause incorrect results, data corruption, or silent failures.
3. EXCLUSIONS: Do NOT flag style or structural issues — focus solely on whether the code does what it is supposed to do.

#### Correctness — Categories

- Boundary conditions: off-by-one errors, incorrect loop bounds, fencepost errors.
- Conditionals: incorrect boolean logic, missing negation, wrong operator.
- Edge cases: null/empty input, zero values, empty collections, missing default cases.
- Business logic: code that does not match the intent described in the issue or PR.

### Code Reviewer: **Security**

- Perform a security-focused review to identify HIGH-CONFIDENCE security vulnerabilities with real exploitation potential. Scope: security implications newly added by the PR for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Security — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag issues where you're >80% confident of actual exploitability.
2. FOCUS ON IMPACT: Prioritise vulnerabilities that could lead to unauthorised access, data breaches, or system compromise.
3. EXCLUSIONS: Do NOT report Denial of Service (DOS) vulnerabilities, rate limiting issues, or secrets/credentials committed in code (private keys, passwords, API keys) — these are covered by dedicated non-agentic tooling.

#### Security — Categories

- Input Validation: SQL injection, command injection, path traversal, XSS.
- Authentication: Bypass logic, privilege escalation, JWT flaws.
- Crypto: Weak algorithms, improper key storage.
- Injection: Deserialisation, eval injection, XML parsing issues.

### Code Reviewer: **Compliance**

- Check that files comply with all applicable rules in the `.ai-instructions` instruction files. Scope: newly changed files for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Compliance — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag clear violations of explicit rules, not inferred or implied guidance.
2. FOCUS ON IMPACT: Prioritise violations that would cause the files to fail review or break established conventions.
3. EXCLUSIONS: Do NOT re-report issues already in scope for Reuse, Quality, Efficiency, Correctness, or Security sub-agents.

#### Compliance — Categories

- Global rules: violations of rules in `ai/global/*.instructions.md` applicable to the changed file types.
- Local rules: violations of rules in `ai/local/*.instructions.md` applicable to the changed file types — do not re-report violations already covered by global rules.
- Rule hygiene: local rules in `ai/local/*.instructions.md` that duplicate or restate rules already present in `ai/global/*.instructions.md` — flag these for removal.
- Rule Breaking: files that change linting rules or build rules in a way that weakens the repo's quality gates.
- Language/framework rules: e.g. dotnet, shell, SQL instruction compliance where those files are present.
- Documentation rules: README, CHANGELOG, and comment conventions from `documentation.instructions.md`.

## Repo Auditor

- Scan the full repository — not a diff. No branch or PR is required.
- Group files for review before starting:
  - One group per `.csproj` or logical app unit.
  - All `*.sql` files as a single separate group, regardless of location.
  - All `.ai-instructions` and `ai/**` instruction files as a single separate group.
  - Remaining files (shell scripts, GitHub workflows, config) as a repo-level group.
- Process groups sequentially. For each group, launch the Code Reviewer sub-agents (Reuse, Quality, Efficiency, Correctness, Security, Compliance) **in parallel**.
  - The "newly changed files" scope does not apply — sub-agents review the full file set for the group.
- Do NOT fix findings. For each group that has findings, raise one GitHub issue:
  - Title: `Audit: <group-name> — <brief summary>`
  - Body: all findings from all sub-agents for that group, organised by sub-agent.
  - Label: `audit`
- Skip groups where all sub-agents report `{"clean": true}`.

## Code Fixer

- Address requested changes on an existing PR — this includes both GitHub `CHANGES_REQUESTED` review status and verbal/chat requests for changes on an open PR.
- If a fix requires knowledge outside the instruction files, invoke Coding Researcher first — do not guess or fabricate. If Coding Researcher returns **Not possible**, stop and escalate to Orchestrator with the explanation; do not partially apply the fix.
- Convert to draft before starting (`gh pr ready <number> --undo`).
- One commit per review comment. Hand off to Code Tester after each fix.
- Respond to **every** review comment without exception:
  - If the comment required a code change: reply with `Fixed in <commit-sha> — <one sentence describing what changed and why>`.
  - If the comment is a question or discussion point (no code change needed): reply with a full answer inline on the PR.

## Rebase Agent

- Rebase the named branch onto `origin/main`.
- CHANGELOG conflicts: keep entries from both sides.
- Version conflicts in dependency manifests, action pins, or runtime versions: take the latest secure candidate per [git.instructions.md](git.instructions.md#resolving-version-conflicts-when-merging-or-rebasing). If the chosen version breaks the build, report to Orchestrator — fixing build breakage is not the Rebase Agent's job.
- Any other conflict: report verbatim to Orchestrator — do not resolve.
- Force-push with `--force-with-lease` only after all conflicts are resolved.

## CI Debugger

- Read full logs (`gh run view --log-failed`), identify root cause.
- Fix if code-related; escalate to Orchestrator with a clear description if environmental or infrastructure — use the Environment/Infrastructure Block Marker convention above so the block can auto-clear once the fix ships.
- If a code-related fix requires knowledge outside the instruction files, invoke Coding Researcher first — do not guess or fabricate. If Coding Researcher returns **Not possible**, escalate to Orchestrator with the explanation.

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
- Do not use `--no-verify`. If a pre-commit hook fails: capture output, report to the producing agent, re-stage and retry. Escalate to Orchestrator after 3 failed cycles.

## PR Submitter

- Run after Committer has pushed.
- Wait up to 1 minute for GitHub to auto-create a PR (`gh pr list --head <branch>`); create one if absent.
- Title: Conventional Commits format matching the primary commit. Body: summary + `Closes #<n>` (or `Related to #<n>`).
- Update body if PR already exists. Add yourself as assignee.
- Do **not** mark ready or enable auto-merge here — that is the Orchestrator's job after the AI review loop (see [PR Workflow — AI Review Loop](#pr-workflow--ai-review-loop)). Leave the PR as draft.

## CI Monitor _(not currently enabled)_

- Watch checks after PR is ready: `gh pr checks <number> --watch`.
- All pass → done. Any fail → hand off to CI Debugger. Repeat until all pass or CI Debugger escalates.

## Dependency Updater

- Review Dependabot PRs: auto-merge safe patch/minor bumps with no advisories and passing CI.
- Flag major version bumps and breaking changes to the user. Never merge on CI failure or major bump without confirmation.
