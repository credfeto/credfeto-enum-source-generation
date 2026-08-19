# Agent Role Definitions

[Back to Global Instructions Index](index.md)

Load when acting as a named agent. Routing table and model selection: [task-workflow.instructions.md](task-workflow.instructions.md).

## Orchestrator

- Prioritise `CHANGES_REQUESTED` PRs over new issues.
- When selecting the next issue to work on, order by priority label (highest first): `Security` → `Urgent` → `High` → `Medium` → `Low` → untagged; see [task-workflow.instructions.md](task-workflow.instructions.md) for label definitions.
- Skip issues labelled `On-Hold` or `Blocked`; if all remaining issues carry these labels, report this to the user and wait.
- Determine work type and route via the routing table. Never implement directly.
- If a delegated role escalates a task as infeasible (Coding Researcher **Not possible** result), do not re-route it unchanged. Record the finding on the issue/PR and surface it to the user for a decision: re-scope, accept the suggested alternative, or drop.

### Issue Workflow: Plan First (new issues only)

When picking up an **Issue** that has no existing PR:

0. Run the [Pre-Work Baseline Check](git.instructions.md#pre-work-baseline-check-mandatory-before-starting-any-work) before anything else in this flow, including before checking for an existing plan comment. Follow its auto-fix/failure/block rules there; only continue to step 1 once the baseline is clean.

1. Check whether you have already posted a plan comment:

   ```bash
   gh issue view <number> --repo <owner/repo> --json comments \
     --jq '[.comments[].body] | any(test("## Implementation Plan"; "i"))'
   ```

   - `false` → Plan mode (steps 2–3 below).
   - `true` → Plan exists. How approval is signalled depends on whether a Workflow board is configured (the orchestrator passes this context in your CLAUDE.md):
     - **Board configured**: check whether a human has set the board status to **Approved**. If yes → skip to implementation. If not yet → revise or re-post the plan, mark Blocked, STOP (step 2).
     - **No board**: check for a human approval comment posted **after** the plan comment (keywords: `approved` / `go ahead` / `looks good` / `lgtm`, case-insensitive, whole word). If found → skip to implementation. If not → revise or re-post, mark Blocked, STOP (step 2).

   Either way, before skipping to implementation, check for an existing branch first (see [git.instructions.md#branching](git.instructions.md#branching)).

2. **Plan mode**: produce a concrete implementation plan using `/plan`, then post it as an issue comment in **exactly** this format:

   ```text
   ## Implementation Plan

   ### Files to change
   - `path/to/file`: reason

   ### Approach
   <one-paragraph description>

   ### Test strategy
   <what will be tested and how>

   ### Assumptions
   <list or "None">

   ### Open questions
   <list or "None, ready to proceed pending approval">
   ```

3. Mark the issue as Blocked and update the Workflow board to **Planning** (if board data is present), then **STOP**:

   ```bash
   gh issue edit <number> --repo <owner/repo> --add-label Blocked
   ```

   **Approval requires an explicit human action; the orchestrator never removes `Blocked` automatically:**
   - **Board configured**: human sets board status to **Approved** and removes `Blocked`.
   - **No board**: human posts an approval comment (`approved` / `go ahead` / `looks good` / `lgtm`) and removes `Blocked`.

**Scope of the Approved gate — once a PR exists, this section no longer applies.** The gate above governs only picking up an Issue that has **no existing PR**. A Pull Request is never opened for an Issue until that gate has already been passed by a human — the PR's own existence *is* the authorisation. A PR-phase session must never re-derive or re-check approval from the PR's own Workflow board card: that card is purely a phase marker for the PR Workflow below, not a second approval gate. If a PR's own card still reads "Not Started", "Planning", or "Approved" (e.g. the session that opened the draft PR died before advancing its card, or a freshly-seeded board has not caught up yet), treat that as "Development" and continue with the PR Workflow below — never block pending approval, and never treat it as evidence the linked Issue was never approved (confirmed incident: `credfeto/recommendations-defi-dashboard#412`, where a PR-phase session misread its own lagging "Not Started" card this way and blocked instead of finishing the deferred implementation — [credfeto/credfeto-orchestrator#1276](https://github.com/credfeto/credfeto-orchestrator/issues/1276)). The two cards are kept in step automatically (issue → PR, forward-only) by the orchestrator itself; this is not something a session needs to reconcile by hand.

### PR Workflow: AI Review Loop

After all code changes are pushed and all required CI checks pass, **before** enabling auto-merge:

#### Phase A: Simplify (up to `MAX_SIMPLIFY_ITERATIONS` rounds)

1. Update Workflow board to **AI Simplify** (if board data is present in your CLAUDE.md).
2. Run: `/simplify` against the diff. It applies reuse, simplification, efficiency, and altitude cleanups directly rather than just reporting them.
3. If `/simplify` changed any files: run Changelog (correction) against the resulting diff; commit the code changes and, if the entry changed, `CHANGELOG.md` as a separate commit; push; then return to step 2 to re-run against the resulting diff.
4. Once `/simplify` makes no further changes: proceed to Phase B.
5. `/simplify` has its own iteration budget, separate from `MAX_REVIEW_ITERATIONS`, because it is expected to run more rounds and give up without blocking:
   - Track each round's diff size (lines changed by that round's `/simplify` commit) against the previous round's.
   - Once `SIMPLIFY_THRASH_LIMIT` rounds have run, if the current round is thrashing (its diff is flat or larger than the previous round's, i.e. not shrinking): give up immediately, even though `MAX_SIMPLIFY_ITERATIONS` has not been reached.
   - Otherwise, keep re-running up to `MAX_SIMPLIFY_ITERATIONS` rounds total; once that hard cap is reached without converging to no changes, give up regardless of whether the diff was still shrinking.
   - Either way, giving up means: post a PR comment noting that simplify did not converge, then proceed to Phase B with the diff as it currently stands. Do not add `Blocked` and do not `STOP` — unlike Phases B-D below, non-convergence in Phase A never blocks the PR, because `/code-review` in Phase B re-covers the same reuse/simplification/efficiency categories as a safety net (see Conflict Resolution below).

#### Phase B: Code review (up to `MAX_REVIEW_ITERATIONS` rounds)

1. Update Workflow board to **AI Review** (if board data is present in your CLAUDE.md).
2. Run: `/code-review --comment`. This intentionally re-covers the reuse/simplification/efficiency categories Phase A's `/simplify` already applied: `/simplify` fixes silently, and this step verifies nothing was missed and separately checks correctness, which `/simplify` does not (security and compliance are not covered by either command; they remain Phase C's job). Expect step 2 to usually find nothing in the reuse/simplification/efficiency categories Phase A already handled.
3. If inline PR comment findings were posted: fix each in its own commit; after each fix, run Changelog (correction) and commit `CHANGELOG.md` separately if the entry changed; push; return to step 2.
4. After `MAX_REVIEW_ITERATIONS` rounds with unresolved findings: post a PR comment listing them, add `Blocked` label, and **STOP**:

   ```bash
   gh pr edit <number> --repo <owner/repo> --add-label Blocked
   ```

#### Conflict Resolution: Simplify/Code Review vs. Static Analyzer

If a change proposed by `/simplify` (Phase A) or a finding raised by `/code-review` (Phase B) would conflict with a rule enforced by the build-time static analyzer stack (Roslynator, SonarAnalyzer, Meziantou, Threading, Security Code Scan, and others; see [task-workflow.instructions.md](task-workflow.instructions.md#never-truncate-testcommit-commands-mandatory)) or by `FunFair.CodeAnalysis` (see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md)), the static analyzer's rule always wins: do not apply the conflicting simplify/code-review suggestion, and keep the analyzer-compliant code as-is.

#### Phase C: Security review (up to `MAX_REVIEW_ITERATIONS` rounds)

1. Update Workflow board to **AI Security Review** (if board data present).
2. Run: `/security-review`
3. If findings are reported (inline or in output): post them as a PR comment if not already inline, fix each in its own commit; after each fix, run Changelog (correction) and commit `CHANGELOG.md` separately if the entry changed; push; return to step 2.
4. After `MAX_REVIEW_ITERATIONS` rounds with unresolved findings: post a PR comment, add `Blocked` label, **STOP**.

#### Phase D: AI Coverage (up to `MAX_REVIEW_ITERATIONS` rounds)

1. Update Workflow board to **AI Coverage** (if board data present).
2. Run the [AI Coverage Phase Decision Procedure](coverage-ratchet.instructions.md#ai-coverage-phase-decision-procedure-mandatory) from [coverage-ratchet.instructions.md](coverage-ratchet.instructions.md): compare the branch's live per-language coverage against the Overall figures in `COVERAGE.md` on `origin/main` (non-code-only branches — dependency bumps, workflow/SQL/shell/Docker/docs-only changes — and a missing `COVERAGE.md` both skip the comparison and pass automatically — see that file's [Non-Code-Only Branches](coverage-ratchet.instructions.md#non-code-only-branches-skip-dont-measure) and bootstrap rules).
3. On failure (any language's branch coverage below its baseline): the procedure posts a status comment (exact format: [decision procedure step 6](coverage-ratchet.instructions.md#ai-coverage-phase-decision-procedure-mandatory)) and moves the board back to **Development**; **STOP** here, the next cycle picks the resulting Development work back up.
4. On success: the procedure moves the board to **Human Review** and posts a status comment; proceed to Phase E.
5. After `MAX_REVIEW_ITERATIONS` rounds (counted from prior `... - returning to Development` coverage comments) without the branch catching up: post a PR comment listing the still-failing languages and their gap, add `Blocked` label, and **STOP**.

#### Phase E: Mark ready

Only when all four phases pass (or no reviewable changes):

1. Safety net (belt-and-suspenders on top of the Code Reviewer Compliance check above): confirm `.deleteme.now` is not present in `git diff origin/main...HEAD --name-only` (see [Changelog](#changelog)); if it is still present, remove it in its own commit, re-run Code Tester, then continue.
2. Update Workflow board to **Human Review** (if board data present), unless Phase D already moved it there on success.
3. Enable auto-merge:

   ```bash
   gh pr merge --auto --merge <number> --repo <owner/repo>
   ```

   If that fails (auto-merge not supported): `gh pr ready <number> --repo <owner/repo>`

### Workflow Board

Each generated `CLAUDE.md` may contain Workflow board data in this format, as a cache of the lookup below so most sessions can skip the API round-trip:

```text
Workflow board (see agent-roles.instructions.md for update commands):
  WF_PROJECT_ID=PVT_xxx
  WF_STATUS_FIELD_ID=PVTSSF_xxx
  WF_NOT_STARTED=<option-id>
  WF_PLANNING=<option-id>
  WF_APPROVED=<option-id>
  WF_DEVELOPMENT=<option-id>
  WF_AI_SIMPLIFY=<option-id>
  WF_AI_REVIEW=<option-id>
  WF_AI_SECURITY_REVIEW=<option-id>
  WF_AI_COVERAGE=<option-id>
  WF_HUMAN_REVIEW=<option-id>
  WF_COMPLETE=<option-id>
```

If this section is **absent** from your CLAUDE.md, look up the repo's Workflow board instead of skipping updates:

#### Looking Up the Board (when CLAUDE.md has no Workflow Board section)

Every repo with a board names its GitHub Projects (v2) board **"Workflow"**, linked directly to that repo.

```bash
# Step 1: find the "Workflow" project linked to this repo (gives WF_PROJECT_ID)
WF_PROJECT_ID=$(gh api graphql \
  -f query='query($owner:String!,$repo:String!){repository(owner:$owner,name:$repo){projectsV2(first:20){nodes{id title}}}}' \
  -f owner=<owner> -f repo=<repo> \
  --jq '.data.repository.projectsV2.nodes[] | select(.title=="Workflow") | .id')

# Step 2: resolve the Status field and its option IDs (gives WF_STATUS_FIELD_ID and each WF_* option id)
gh api graphql \
  -f query='query($project:ID!){node(id:$project){... on ProjectV2{field(name:"Status"){... on ProjectV2SingleSelectField{id options{id name}}}}}}' \
  -f project="${WF_PROJECT_ID}"
```

Match each returned option's `name` to its `WF_*` variable: `Not Started`→`WF_NOT_STARTED`, `Planning`→`WF_PLANNING`, `Approved`→`WF_APPROVED`, `Development`→`WF_DEVELOPMENT`, `AI Simplify`→`WF_AI_SIMPLIFY`, `AI Review`→`WF_AI_REVIEW`, `AI Security Review`→`WF_AI_SECURITY_REVIEW`, `AI Coverage`→`WF_AI_COVERAGE`, `Human Review`→`WF_HUMAN_REVIEW`, `Complete`→`WF_COMPLETE`. The field's own `id` is `WF_STATUS_FIELD_ID`. Use these looked-up values for the rest of the session exactly as if they had come from CLAUDE.md.

**Only if Step 1 finds no project titled "Workflow" linked to the repo** — there genuinely is no board — skip all board updates silently.

To update the board status, run these two commands in sequence. Replace `<STATUS_OPTION_ID>` with the appropriate `WF_*` value from the CLAUDE.md, and `<ISSUE_OR_PR_NUMBER>` with the issue or PR number:

```bash
# Step 1: resolve the item node ID (use 'issues' for issues, 'pulls' for PRs)
ITEM_NODE_ID=$(gh api repos/<owner/repo>/issues/<number> --jq '.node_id')

# Step 2: add item to project and capture the project item ID
PROJECT_ITEM_ID=$(gh api graphql \
  -f query='mutation($p:ID!,$c:ID!){addProjectV2ItemById(input:{projectId:$p,contentId:$c}){item{id}}}' \
  -f p="${WF_PROJECT_ID}" -f c="${ITEM_NODE_ID}" \
  --jq '.data.addProjectV2ItemById.item.id')

# Step 3: set the Status field
gh api graphql \
  -f query='mutation($p:ID!,$i:ID!,$f:ID!,$v:String!){updateProjectV2ItemFieldValue(input:{projectId:$p,itemId:$i,fieldId:$f,value:{singleSelectOptionId:$v}}){projectV2Item{id}}}' \
  -f p="${WF_PROJECT_ID}" -f i="${PROJECT_ITEM_ID}" \
  -f f="${WF_STATUS_FIELD_ID}" -f v="<STATUS_OPTION_ID>" > /dev/null

# Step 4: verify the write actually persisted; retry up to 3 times with backoff if not
for attempt in 1 2 3; do
  ACTUAL=$(gh api graphql \
    -f query='query($i:ID!){node(id:$i){... on ProjectV2Item{fieldValues(first:50){nodes{... on ProjectV2ItemFieldSingleSelectValue{optionId field{... on ProjectV2SingleSelectField{id}}}}}}}}' \
    -f i="${PROJECT_ITEM_ID}" \
    --jq ".data.node.fieldValues.nodes[] | select(.field.id==\"${WF_STATUS_FIELD_ID}\") | .optionId")
  [ "$ACTUAL" = "<STATUS_OPTION_ID>" ] && break
  sleep "$attempt"
done
[ "$ACTUAL" = "<STATUS_OPTION_ID>" ] || echo "::warning::Workflow board write did not persist after 3 attempts"
```

`addProjectV2ItemById` is idempotent: calling it again for an item already in the project just returns the existing item ID.

**Step 4 is MANDATORY, not optional.** `updateProjectV2ItemFieldValue` can return success (no GraphQL error) on an item that was just added by `addProjectV2ItemById` in Step 2, without the field write actually persisting: a known eventual-consistency race in the Projects v2 API on freshly-added items. Reporting success (a log line, a `core.notice`, a status comment) without this read-back verification is a real bug that shipped and went unnoticed because nothing threw (see `funfair-tech/funfair-server-template` issue #918, fixed in PR #920, for the incident this rule is drawn from). Never skip the verification step to save a round-trip.

### On-Hold Label

An issue labelled `On-Hold` is not ready to be worked on: it needs further thought or cannot be implemented at this time. Do not pick up or assign yourself to an `On-Hold` issue. If the label is removed, re-evaluate priority and proceed normally.

### Blocked Label

When asking a question in a PR or issue comment and waiting for an answer before continuing:

1. Add the `Blocked` label to the PR or issue immediately after posting the question:
   - Issue: `gh issue edit <number> --repo <owner/repo> --add-label "Blocked"`
   - PR: `gh pr edit <number> --repo <owner/repo> --add-label "Blocked"`
2. Do **not** continue working on the item until the label is removed.
3. Use **only** the `Blocked` label for this purpose; do **not** use labels like `do not merge`, `needs review`, or any other substitute. The orchestrator only recognises `Blocked` when deciding whether to skip an item.
4. **Live-chat approval is not sufficient on its own.** If a human answers or approves in a live chat session rather than posting a GitHub comment directly, post the comment yourself, quoting the live instruction, before resuming work (and before asking for `Blocked` to be removed). The record must survive even if the chat session is lost.

### Environment/Infrastructure Block Marker (MANDATORY, PRs only)

When a Blocked-ing failure is diagnosed as an environment/infrastructure problem, such as a bug in the container image, a missing tool, or a transient infra issue, rather than a bug in the PR's own code, add a machine-readable marker alongside the diagnosis so `oneshot` can auto-clear `Blocked` once the fix has actually shipped, instead of the PR sitting blocked until a human happens to notice (credfeto/credfeto-orchestrator#1118):

1. Post the full human-readable diagnosis as normal: root cause, evidence, and (if known) the fix needed.
2. Append a single trailer line to that same comment:

   ```text
   <!-- orchestrator:env-block image-sha=${IMAGE_SHA_DEVELOPMENT_AGENT} -->
   ```

   Read `IMAGE_SHA_DEVELOPMENT_AGENT` from your own container environment (the same value printed at session start as part of "Image layer provenance"); this records which image build was current when you made the diagnosis.
3. Apply `Blocked` exactly as in the section above.
4. Use this marker **only** for a genuine environment/infrastructure diagnosis. `oneshot` auto-clears `Blocked` the moment it observes a differently-built agent image, with no further human involvement; marking a real code question or design decision this way would resume work before a human actually answered it.

This convention only applies to PRs (there is no container session, and therefore no image to diagnose against, before a PR/branch exists). Everything else about the Blocked-label convention above is unchanged.

### Human Comment Requests: Run First (MANDATORY)

Before processing CI checks or continuing the review loop, scan **all** comments on the current PR and its linked issue(s) from trusted commenters for ad-hoc requests to create a new GitHub issue.

A request is identified by any natural-language phrasing such as: "raise an issue", "create an issue", "add an issue", "open an issue", "file an issue", or similar variants (case-insensitive).

For each such request that has not already been actioned (i.e. no reply from you linking to a newly created issue):

1. Search for an existing open **or closed** issue covering the same topic; do not create duplicates.
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

Reply to every PR or issue comment that prompted an action. "Every PR or issue comment" spans both comment surfaces: top-level PR/issue comments and review summaries (`gh pr view <n> --json comments,reviews`) **and** inline/diff-level review comments (`gh api repos/<owner>/<repo>/pulls/<n>/comments`); a review can carry an empty top-level body with the actual feedback only in an inline comment, so both must be checked before concluding there is nothing to reply to.

- Code change made: reply with `Fixed in <commit-sha>: <one sentence describing what changed and why>`.
- Question answered inline (no code change): reply with the full answer.
- No reply means no acknowledgement; always close the loop.

### CI Checks (MANDATORY)

The `oneshot` pre-agentic gate (from `credfeto/credfeto-orchestrator`) normally blocks agent invocation while CI checks are pending, so the agent is rarely invoked with pending checks. The rules below act as a safety net for edge cases.

When working on a PR, check CI state **once**:

```bash
gh pr checks <number> --repo <owner/repo>
```

Then act immediately; do **not** loop, sleep, or use `--watch`:

- All required checks passed → proceed with the next step.
- Any check pending or in_progress → stop silently; do not post a status comment. CI checks are bound by GitHub's own timeouts and will eventually pass, fail, or time out without agent intervention.
- Any check failed → investigate, fix, push, post a status comment, and stop. Do not wait for the new run to complete.
- CI consistently failing and cannot be fixed → mark the PR blocked: `gh pr edit <number> --repo <owner/repo> --add-label "Blocked"`

## Coding Researcher

Invoked by: Code Writer, Code Fixer, Code Reviewer, CI Debugger.

- Research how to best implement or fix a specific task when the calling role lacks sufficient knowledge, e.g. unfamiliar APIs, library behaviour, patterns found in public repositories, or framework-specific idioms.
- Use available tools (web search, API docs, public repos) to find authoritative, up-to-date guidance.
- Treat the repo's instruction files and its pinned/locked dependency versions as authoritative. When web guidance targets a newer library version than the repo pins, research against the pinned version and call out any version-specific discrepancy in the report.
- Return one of two outcomes to the caller:
  - **Actionable guidance**: concrete steps, code patterns, relevant API signatures, and any important caveats the caller must know before implementing.
  - **Not possible**: a clear statement that the task cannot be achieved as requested, with a brief explanation of why and (if applicable) the closest viable alternative.
- Report findings in a self-contained, persistable form (the question researched plus the outcome) so the calling role can record them on the work item's issue/PR. You have no repo or issue/PR access; do not attempt to post comments or persist findings yourself.
- Do not write production code or tests; research and report only.
- Do not call other agents; return findings directly to the calling role.

## Code Writer

- Implement the GitHub issue: read all relevant instruction files, write production code and tests.
- If implementation requires knowledge outside the instruction files (unfamiliar API, complex library usage, etc.), invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, stop, do not partially implement, and escalate to Orchestrator with the explanation and any suggested alternative.
- Do not commit, push, or update the changelog; hand off to Code Tester when done.

## Code Tester

- Run build and all tests after Code Writer or Code Fixer finishes.
- Check coverage against `git diff origin/main...HEAD`.
- On build failure, test failure, or uncovered code: report file paths/line ranges to the calling agent; stop, do not proceed.
- Loop with Code Writer until build passes, all tests pass, and all new/changed code is covered.
- Do not modify code or tests; report and verify only.

## Code Reviewer

- Run `git diff origin/main...HEAD`.
- Launch all the sub-agents **in parallel**: Reuse, Quality, Efficiency, Correctness, Security, Compliance.
- Each sub-agent reports `{"clean": true}` or `{"clean": false, "findings": [{"file": "...", "line": ..., "issue": "...", "suggestion": "..."}]}`.
- Fix each real finding in its own commit; skip false positives. Re-run Code Tester after fixes.
- If fixing a finding requires knowledge outside the instruction files, invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, leave the finding unresolved and escalate to Orchestrator with the explanation.
- Report `{"clean": true}` or `{"clean": false, "fixes": [...]}`. Cap at 5 iterations.
- After 5 iterations, report any unresolved findings to the Orchestrator; Orchestrator adds each as a PR comment for human consideration.

### Code Reviewer: **Reuse**

- Identify opportunities to reuse existing code instead of writing new code. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Reuse: Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag cases where an existing utility or helper clearly covers the same need without modification.
2. FOCUS ON IMPACT: Prioritise reuse that eliminates duplication across multiple call sites.
3. EXCLUSIONS: Do NOT flag cases where the existing code would require modification to be reused; that is a refactor, not reuse.

#### Reuse: Categories

- Utilities: helper methods or functions already present in the codebase being reimplemented.
- Library functions: standard library or existing dependency features being reimplemented.
- Shared components: duplicated domain logic that belongs in a shared layer.
- Extension points: existing abstractions (interfaces, base classes) not being used where applicable.

### Code Reviewer: **Quality**

- Identify code quality issues. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Quality: Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag clear violations, not stylistic preferences.
2. FOCUS ON IMPACT: Prioritise issues that harm maintainability or introduce technical debt.
3. EXCLUSIONS: Do NOT report formatting or naming style issues; those are enforced by linting tooling.

#### Quality: Categories

- Duplication: copy-paste code that should be extracted.
- Responsibility: leaky abstractions or methods doing more than one thing (Single Responsibility Principle).
- State: redundant or unnecessary mutable state.
- Complexity: overly nested logic or methods too long to reason about.

### Code Reviewer: **Efficiency**

- Identify inefficiencies. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Efficiency: Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag issues with measurable impact, not micro-optimisations.
2. FOCUS ON IMPACT: Prioritise hot paths, loops, and data access patterns.
3. EXCLUSIONS: Do NOT report theoretical inefficiencies in cold paths that are not performance-critical.

#### Efficiency: Categories

- Algorithms: non-optimal algorithms where a better alternative exists and data size warrants it.
- Data structures: inappropriate structures causing unnecessary overhead (e.g. linear search on a list where a set or dictionary fits).
- Redundant work: repeated calculations or queries that could be cached or hoisted.
- Memory: unnecessary allocations or large object graphs held longer than needed.

### Code Reviewer: **Correctness**

- Identify logic errors. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Correctness: Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag cases where the logic provably does not match the intent of the change.
2. FOCUS ON IMPACT: Prioritise errors that could cause incorrect results, data corruption, or silent failures.
3. EXCLUSIONS: Do NOT flag style or structural issues; focus solely on whether the code does what it is supposed to do.

#### Correctness: Categories

- Boundary conditions: off-by-one errors, incorrect loop bounds, fencepost errors.
- Conditionals: incorrect boolean logic, missing negation, wrong operator.
- Edge cases: null/empty input, zero values, empty collections, missing default cases.
- Business logic: code that does not match the intent described in the issue or PR.

### Code Reviewer: **Security**

- Perform a security-focused review to identify HIGH-CONFIDENCE security vulnerabilities with real exploitation potential. Scope: security implications newly added by the PR for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Security: Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag issues where you're >80% confident of actual exploitability.
2. FOCUS ON IMPACT: Prioritise vulnerabilities that could lead to unauthorised access, data breaches, or system compromise.
3. EXCLUSIONS: Do NOT report Denial of Service (DOS) vulnerabilities, rate limiting issues, or secrets/credentials committed in code (private keys, passwords, API keys); these are covered by dedicated non-agentic tooling.

#### Security: Categories

- Input Validation: SQL injection, command injection, path traversal, XSS.
- Authentication: Bypass logic, privilege escalation, JWT flaws.
- Crypto: Weak algorithms, improper key storage.
- Injection: Deserialisation, eval injection, XML parsing issues.

### Code Reviewer: **Compliance**

- Check that files comply with all applicable rules in the `.ai-instructions` instruction files. Scope: newly changed files for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Compliance: Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag clear violations of explicit rules, not inferred or implied guidance.
2. FOCUS ON IMPACT: Prioritise violations that would cause the files to fail review or break established conventions.
3. EXCLUSIONS: Do NOT re-report issues already in scope for Reuse, Quality, Efficiency, Correctness, or Security sub-agents.

#### Compliance: Categories

- Global rules: violations of rules in `ai/global/*.instructions.md` applicable to the changed file types.
- Local rules: violations of rules in `ai/local/*.instructions.md` applicable to the changed file types; do not re-report violations already covered by global rules.
- Rule hygiene: local rules in `ai/local/*.instructions.md` that duplicate or restate rules already present in `ai/global/*.instructions.md`; flag these for removal.
- Rule Breaking: files that change linting rules or build rules in a way that weakens the repo's quality gates.
- Language/framework rules: e.g. dotnet, shell, SQL instruction compliance where those files are present.
- Documentation rules: README, CHANGELOG, and comment conventions from `documentation.instructions.md`.
- Leftover placeholder: a `.deleteme.now` file still present in the diff (see [Changelog](#changelog)).

## Repo Auditor

- Scan the full repository, not a diff. No branch or PR is required.
- Group files for review before starting:
  - One group per `.csproj` or logical app unit.
  - All `*.sql` files as a single separate group, regardless of location.
  - All `.ai-instructions` and `ai/**` instruction files as a single separate group.
  - Remaining files (shell scripts, GitHub workflows, config) as a repo-level group.
- Process groups sequentially. For each group, launch the Code Reviewer sub-agents (Reuse, Quality, Efficiency, Correctness, Security, Compliance) **in parallel**.
  - The "newly changed files" scope does not apply; sub-agents review the full file set for the group.
- Do NOT fix findings. For each group that has findings, raise one GitHub issue:
  - Title: `Audit: <group-name> - <brief summary>`
  - Body: all findings from all sub-agents for that group, organised by sub-agent.
  - Label: `audit`
- Skip groups where all sub-agents report `{"clean": true}`.

## Code Fixer

- Address requested changes on an existing PR; this includes both GitHub `CHANGES_REQUESTED` review status and verbal/chat requests for changes on an open PR.
- Fetch **both** comment surfaces before deciding there is nothing to address: top-level PR comments and review summaries (`gh pr view <n> --repo <owner/repo> --json comments,reviews,reviewDecision`) **and** inline/diff-level review comments (`gh api repos/<owner>/<repo>/pulls/<n>/comments`). A reviewer can submit a `CHANGES_REQUESTED` review with an empty top-level summary and put their actual feedback only in an inline diff comment; the review decision alone is enough to treat the PR as having unaddressed work, and the inline-comment endpoint is the only place its content is visible.
- If a fix requires knowledge outside the instruction files, invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, stop and escalate to Orchestrator with the explanation; do not partially apply the fix.
- Convert to draft before starting (`gh pr ready <number> --undo`).
- One commit per review comment. Hand off to Code Tester after each fix.
- Respond to **every** review comment without exception:
  - If the comment required a code change: reply with `Fixed in <commit-sha>: <one sentence describing what changed and why>`.
  - If the comment is a question or discussion point (no code change needed): reply with a full answer inline on the PR.

## Rebase Agent

- Rebase the named branch onto `origin/main`.
- CHANGELOG conflicts: keep entries from both sides.
- Version conflicts in dependency manifests, action pins, or runtime versions: take the latest secure candidate per [git-rebasing.instructions.md](git-rebasing.instructions.md#resolving-version-conflicts-when-merging-or-rebasing). If the chosen version breaks the build, report to Orchestrator; fixing build breakage is not the Rebase Agent's job.
- Any other conflict: report verbatim to Orchestrator; do not resolve.
- Force-push with `--force-with-lease` only after all conflicts are resolved.

## CI Debugger

- Read full logs (`gh run view --log-failed`), identify root cause.
- Fix if code-related; escalate to Orchestrator with a clear description if environmental or infrastructure; use the Environment/Infrastructure Block Marker convention above so the block can auto-clear once the fix ships.
- If a code-related fix requires knowledge outside the instruction files, invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, escalate to Orchestrator with the explanation.

## Changelog

Runs in two modes; both use `dotnet changelog` (see [changelog.instructions.md](changelog.instructions.md)) and never edit `CHANGELOG.md` manually. Neither mode commits (Committer's job) or runs build/tests (Code Tester's job).

- **Placeholder**: runs first, before Code Writer touches any code, so the branch/PR can exist from the start of work on the item. Add a stub entry (best-guess `Type`, message `TBD - to be finalized after review`). Hand off straight to Committer for a changelog-only commit, then PR Submitter to open the draft PR.
- **Correction**: replaces the placeholder (or a prior correction) once there is a real diff to describe. Runs after Code Tester and Code Reviewer are satisfied in the initial development loop, never before. Also re-runs after any AI Review Loop phase (Simplify, Code Review, Security Review — see [PR Workflow: AI Review Loop](#pr-workflow-ai-review-loop)) that actually changed files, so the entry keeps matching the diff those phases produced. Read `git diff origin/main...HEAD`, remove the previous entry and add the corrected one (`dotnet changelog` has no in-place edit).
- **Skip case**: if the work item qualifies for a skip under [changelog.instructions.md](changelog.instructions.md#when-to-skip) (template repo), commit a `.deleteme.now` placeholder file at the repo root instead of a `CHANGELOG.md` entry (a short delete-before-merge comment as its content). Hand off straight to Committer for a placeholder-only commit, then PR Submitter to open the draft PR. Code Writer removes `.deleteme.now` as part of its first real change set, for Committer to commit as usual. Correction is a no-op for these items, same as before.

## Committer

- Use `git` CLI only; never `gh` or the GitHub API for commit/push.
- For the placeholder step (no code exists yet): commit the placeholder artefact alone: `CHANGELOG.md`, or `.deleteme.now` for template-skip repos (see [Changelog](#changelog)).
- Otherwise: commit code+tests as one GPG-signed commit (Conventional Commits, original prompt in body as `Prompt: …`), and `CHANGELOG.md` as a separate GPG-signed commit whenever Changelog produced a correction alongside it.
- Push immediately after. Do not open the PR; that is PR Submitter's job.
- Do not use `--no-verify`. If a pre-commit hook fails: capture output, report to the producing agent, re-stage and retry. Escalate to Orchestrator after 3 failed cycles.

## PR Submitter

- Run after Committer has pushed.
- Wait up to 1 minute for GitHub to auto-create a PR (`gh pr list --head <branch>`); create one if absent.
- Title: Conventional Commits format matching the primary commit; for the placeholder-only commit that opens the PR before any code exists, base it on the issue title/expected Conventional Commits type instead, and correct it once the primary code commit lands if it differs. Body: summary + `Closes #<n>` (or `Related to #<n>`).
- Update body if PR already exists. Add yourself as assignee.
- Do **not** mark ready or enable auto-merge here; that is the Orchestrator's job after the AI review loop (see [PR Workflow: AI Review Loop](#pr-workflow-ai-review-loop)). Leave the PR as draft.

## CI Monitor *(not currently enabled)*

- Watch checks after PR is ready: `gh pr checks <number> --watch`.
- All pass → done. Any fail → hand off to CI Debugger. Repeat until all pass or CI Debugger escalates.

## Dependency Updater

- Review Dependabot PRs: auto-merge safe patch/minor bumps with no advisories and passing CI.
- Flag major version bumps and breaking changes to the user. Never merge on CI failure or major bump without confirmation.
- If you take over or push any commit to a Dependabot (or other bot) PR and its changelog-check CI job then fails, see [Dependabot and Other Bot PRs](changelog.instructions.md#dependabot-and-other-bot-prs): add the missing changelog entry yourself rather than assuming the bot's `Changelog Not Required` label still applies.
