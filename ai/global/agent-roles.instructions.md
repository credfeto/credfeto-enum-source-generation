# Agent Role Definitions

[Back to Global Instructions Index](index.md)

Load when acting as a named agent. Routing table and model selection: [task-workflow.instructions.md](task-workflow.instructions.md).

## Orchestrator

- Prioritise `CHANGES_REQUESTED` PRs over new issues.
- When selecting the next issue to work on, order by priority label (highest first): `Security` → `Urgent` → `High` → `Medium` → `Low` → untagged; see [task-workflow.instructions.md](task-workflow.instructions.md) for label definitions.
- Skip issues labelled `On Hold` or `Blocked`; if all remaining issues carry these labels, report this to the user and wait.
- Determine work type and route via the routing table. Never implement directly.
- If a delegated role escalates a task as infeasible (Coding Researcher **Not possible** result), do not re-route it unchanged. Record the finding on the issue/PR and surface it to the user for a decision: re-scope, accept the suggested alternative, or drop.
- When a delegated role reports a pre-existing bug outside the current change's scope (in Code Reviewer's `preExistingBugs`, listed in a Code Writer or Code Fixer hand-off report, or in a CI Debugger report, including one that CI Monitor passes on), handle it as in [Pre-Existing Bugs Found During Work](code-quality.instructions.md#pre-existing-bugs-found-during-work-mandatory). If the human chooses to fix it, route the fix as in that section's [P3](code-quality.instructions.md#pre-existing-bug-fix-route) rather than fixing it yourself, because the Orchestrator never implements directly.

### Trusted Commenters

A trusted commenter is a human whose comment can approve a plan or ask for work. Decide it by the comment author's login, never by `authorAssociation` (`OWNER`, `MEMBER` or `COLLABORATOR`), because an account with collaborator access is not necessarily a human approver: the agent's own bot account is usually a collaborator.

- **P1.** Trust only the logins in the "Trusted commenters" list the orchestrator passes in your CLAUDE.md.
- **P2.** Never trust a comment whose author login is the bot login the orchestrator passes in your CLAUDE.md alongside the "Trusted commenters" list, even if that login is also in the list, because a comment from the agent's own bot account is never a human approval, and its live-chat mirror comment ([Blocked Label](#blocked-label) P4) quotes the approval keywords and would otherwise approve its own plan. Match that login by name, never by whether `gh` reports `viewerDidAuthor` as `true`, because the agent can run as a trusted human's account (the repository owner, for example) and excluding every comment by that account would drop that human's real approvals. If no bot login is provided, exclude no login.
- **P3.** If no list is provided (for example an interactive session started without the orchestrator), trust only the repository owner's login (the `<owner>` in `<owner/repo>`), because it is the most conservative choice. If the owner is an organisation, no comment matches, so ask the human instead.
- **P4.** Never write either approval keyword (`approved` or `lgtm`, in any case) in a comment you post unless that comment mirrors a real human approval ([Blocked Label](#blocked-label) P4, [Waiting for Approval in an Interactive Session](#waiting-for-approval-in-an-interactive-session) P5), or the keyword sits inside a verbatim quote of a human's own words formatted as a Markdown quote (`>`), as [Prompt Traceability](task-workflow.instructions.md#prompt-traceability-mandatory) and [Blocked Label](#blocked-label) P4 require, because those rules need the exact text and a quote is plainly the human's words. The ban covers every other comment, such as a re-block comment saying no approval was found, a status comment or a question; to refer to the words there, write "the two accepted approval keywords". This is what stops the agent's own comments being read as approval when it runs as a trusted account, because P2 can only exclude a separate bot login and cannot tell the agent's comments apart from that account's human ones.

### Issue Workflow: Plan First (new issues only)

When picking up an **Issue** that has no existing PR:

- **P1.** Run the [Pre-Work Baseline Check](git.instructions.md#pre-work-baseline-check-mandatory-before-starting-any-work) before anything else in this flow, including before checking for an existing plan comment. Follow its auto-fix/failure/block rules there; only continue to P2 once the baseline is clean.

- **P2.** Check whether you have already posted a plan comment. A comment is a plan comment only when one of its lines is exactly `## Implementation Plan` (case-sensitive, nothing else on that line, a trailing carriage return ignored), wherever that line sits in the comment, because a plan with a short preamble should still count and an exact whole-line, case-sensitive heading avoids false matches. The query splits on lines because jq's `^` and `$` anchor to the whole string, not to each line:

  ```bash
  gh issue view <number> --repo <owner/repo> --json comments \
    --jq 'any(.comments[].body; split("\n") | any(rtrimstr("\r") == "## Implementation Plan"))'
  ```

  - `false` → Plan mode (P3–P4 below).
  - `true` → Plan exists. How approval is signalled depends on whether a Workflow board is configured (the orchestrator passes this context in your CLAUDE.md):
    - **Board configured**: check whether a human with project write access (the board only lets those people move a card) has set the board status to **Approved**. If yes → skip to implementation. If not yet → re-post any revised plan as a new comment, mark Blocked, STOP (P3); in an interactive session, then wait as in [Waiting for Approval in an Interactive Session](#waiting-for-approval-in-an-interactive-session).
    - **No board**: check for an approval comment from a [trusted commenter](#trusted-commenters) posted **after** the plan comment (keywords: `approved` / `lgtm`, case-insensitive, whole word). If found → skip to implementation. If not → re-post any revised plan as a new comment, mark Blocked, STOP (P3); in an interactive session, then wait as in [Waiting for Approval in an Interactive Session](#waiting-for-approval-in-an-interactive-session).

  Either way, before skipping to implementation, check for an existing branch first (see [git.instructions.md#branching](git.instructions.md#branching)).

- **P3.** **Plan mode**: produce a concrete implementation plan using `/plan`, then post it as an issue comment in **exactly** this format:

  ```text
  ## Implementation Plan

  ### Files to change
  - `path/to/file`: reason

  ### Approach
  <one-paragraph description>

  ### Test strategy
  <what will be tested and how>

  ### Assumptions
  <list, using a lower-case alpha sequence (a., b., c., ...), or "None">

  ### Open questions
  <list, using a Q-prefixed numbered sequence (Q1., Q2., Q3., ...), or "None, ready to proceed pending approval">
  ```

  **Open questions vs. embedded conditional decisions:** any conditional or deferred decision point in the Approach or Files-to-change text — a decision the plan does not itself resolve (e.g. "needs policy sign-off", "pending a decision on X", an either/or left open) — must be lifted out into its own `Qn.` entry under Open questions, not left as prose in Approach/Files-to-change. Prose framing hides it from the Blocked/approval gate below, which only inspects Open questions; a `Qn.` entry is what actually forces it through that gate. See [Pre-Closure Decision Check](task-workflow.instructions.md#pre-closure-decision-check-mandatory) for the matching check when closing.

- **P4.** Mark the issue as Blocked and update the Workflow board to **Planning** (if the repo has a Workflow board), then **STOP**:

  ```bash
  gh issue edit <number> --repo <owner/repo> --add-label Blocked
  ```

  **Approval requires an explicit human action; the orchestrator never removes `Blocked` automatically (sole exception: live-chat approval in an interactive session, [P5](#waiting-for-approval-in-an-interactive-session)):**
  - **Board configured**: a human with project write access sets board status to **Approved** and removes `Blocked`.
  - **No board**: a [trusted commenter](#trusted-commenters) posts an approval comment (`approved` / `lgtm`) and removes `Blocked`.

  Revise a plan by posting a new `## Implementation Plan` comment, never by editing one in place, so approval is always judged against the latest plan comment.

  In an interactive session, keep watching the issue rather than ending the turn: see [Waiting for Approval in an Interactive Session](#waiting-for-approval-in-an-interactive-session).

**Check GitHub's live state, not just chat.** A human's approval action may land directly on the issue/PR (a comment, a label change, moving the board card) without also being repeated in chat — they already have to open the item to read the posted plan, so relaying it a second time in chat is not something to wait on. Before treating an item as approved, still blocked, or unchanged, re-check its live state (`gh issue view`/`gh pr view` for labels and comments, plus the board's workflow status via `cfwf workflow-status --check`) rather than relying on stale memory or assuming silence in chat means nothing has happened on GitHub. This cuts both ways: a literal chat-only approval (a human typing one of the keywords above directly into the chat session, rather than posting them as a GitHub comment) is still valid on its own, but must be mirrored as a GitHub comment per the live-chat rule in [Blocked Label](#blocked-label) so the record survives even if the chat session is lost — do not treat chat-only approval as a substitute for checking GitHub, and do not treat an unexplained GitHub-side state change as approval without confirming a human actually made it (an automated board rule or a stray process flipping a field is not a human decision).

**Scope of the Approved gate — once a PR exists, this section no longer applies.** The gate above governs only picking up an Issue that has **no existing PR**. A Pull Request is never opened for an Issue until that gate has already been passed by a human — the PR's own existence *is* the authorisation. A PR-phase session must never re-derive or re-check approval from the PR's own Workflow board card: that card is purely a phase marker for the PR Workflow below, not a second approval gate. If a PR's own card still reads "Not Started", "Planning", or "Approved" (e.g. the session that opened the draft PR died before advancing its card, or a freshly-seeded board has not caught up yet), treat that as "Development" and continue with the PR Workflow below — never block pending approval, and never treat it as evidence the linked Issue was never approved. The two cards are kept in step automatically (issue → PR, forward-only) by the orchestrator itself; this is not something a session needs to reconcile by hand.

#### Waiting for Approval in an Interactive Session

Interactive sessions only; an unattended run stops at Plan First P4 and must not poll. A session counts as interactive only once a human has typed a message in it; an injected prompt or task notification does not count. If unsure, assume it is unattended, because an unattended run treated as interactive would poll or wait for a reply that never comes. Only the Orchestrator decides the run mode. It states the mode (interactive or unattended) in every hand-off to a role whose rules depend on it, such as CI Monitor or a role applying [CI Checks](#ci-checks-mandatory), and that role uses the stated mode rather than judging it itself, because a sub-agent only ever sees an injected prompt and would always conclude it is unattended. A hand-off that states no mode means unattended.

- **P1.** After Plan First P4 has posted the plan and added `Blocked` (or, on resume, after Plan First P2 finds a plan that is not yet approved), "STOP" there means stop working on the issue, not stop watching it. Run the P2 read once now and take its `plan` as the baseline (P3), then start a dynamic-pacing loop instead of ending the turn:

  ```text
  /loop check whether issue <number> in <owner/repo> has been approved (plan baseline <plan>); if not, wait
  ```

- **P2.** Each tick, read all of the following, then decide (do not stop early, so a half-finished approval can be flagged):
  - The labels, the latest plan comment's `createdAt`, and the comments a [trusted commenter](#trusted-commenters) posted after it. Replace `<trusted logins>` with the trusted logins, each quoted and separated by commas, and `<bot login>` with the bot login ([Trusted Commenters](#trusted-commenters) P2), quoted; if no bot login is provided, delete the line that contains `<bot login>`:

    ```bash
    gh issue view <number> --repo <owner/repo> --json labels,comments \
      --jq '([.comments[] | select(.body | split("\n") | any(rtrimstr("\r") == "## Implementation Plan"))] | last | .createdAt) as $plan
            | {blocked: ([.labels[].name] | index("Blocked") != null),
               plan: $plan,
               afterPlan: [.comments[]
                 | select($plan != null and .createdAt > $plan
                   and .author.login != <bot login>
                   and (.author.login | IN(<trusted logins>)))
                 | .body]}'
    ```

    Read `afterPlan` and judge it as Plan First P2 does: a comment approves only if it uses one of the Plan First P4 keywords as an unconditional approval, not a question, a negation or a qualified approval ("approved, but ..."). The query also leaves out comments by the bot login ([Trusted Commenters](#trusted-commenters) P2), because a comment from the agent's own bot account is never a human approval, and its live-chat mirror comment quotes the keywords and is posted after the plan, so the timestamp filter alone would count it as approval. It matches the bot login by name rather than by `viewerDidAuthor`, so an approval from a trusted human whose account the agent runs as still counts.
  - **Board configured only**: the card's workflow status, read with `cfwf` as in [Updating and Reading the Board with `cfwf`](#updating-and-reading-the-board-with-cfwf). A non-zero exit means treat it as not approved:

    ```bash
    cfwf workflow-status --check --repo <owner/repo> --issue <number>
    ```

  Decide as follows, using the same rules as Plan First P2 and P4:
  - **Approved**: `blocked` is `false` and `plan` is not null, and either the card is `Approved` (board configured) or a comment in `afterPlan` approves (no board), and `plan` still equals the baseline (P3).
  - **Half-finished**: the approval signal is present but `blocked` is still `true` (the human has not finished clearing it). Keep waiting and tell the human in chat when you first see it.
  - **Otherwise**: not yet, and wait silently. If `plan` is null, no plan comment was found: tell the human and stop the loop.

- **P3.** The plan baseline is the `plan` value from the P1 read (the latest plan comment's `createdAt`, whether just posted or found on resume); carry it in the `/loop` prompt (P1) so it is explicit on every tick. The plan comment is found by its heading alone, so a plan posted under any account is seen. If a later tick returns a different `plan`, the plan changed and earlier approvals no longer count. On the board, a card only stays `Approved` for a plan that has not been re-posted, because every re-post resets the card to **Planning** (Plan First P4). If you revised the plan, restart from Plan First P4 with the new plan (`Blocked` re-added, board back to **Planning**, new baseline in the prompt); if someone else posted it, tell the human in chat and wait for their direction instead of treating it as the plan.

- **P4.** Pace the loop with `ScheduleWakeup`, as the `/loop` skill's dynamic mode does (it defines the parameters, including `noop` and `stop`), passing the `/loop` prompt from P1 back each tick. If `ScheduleWakeup` is unavailable, do not poll: tell the human the issue is waiting and that saying `approved` in chat (P5) will continue the work.
  - Wait `delaySeconds: 1200` (20 minutes) with `noop: true` while nothing has changed. There is no wait cap: the loop ends when the session does, and the 30-minute deadline in [Background Tasks and Monitor Tool](task-workflow.instructions.md#background-tasks-and-monitor-tool-mandatory) governs commands, not a wait for a human.
  - On approval, whether found on a tick or given in chat (P5), stop the loop with `ScheduleWakeup` and `stop: true`, check for an existing branch as in Plan First P2, and continue to implementation.

- **P5.** **Live-chat approval ends the wait immediately.** If the human's chat message opens with the literal word `approved` or `lgtm` (case-insensitive) and is otherwise an unconditional approval, do not wait for the next tick. This is the one place the agent acts on a chat message alone. A question ("is this approved yet?"), a negation ("not approved"), a qualified approval or a passing mention does not count; if in doubt, ask:
  - Re-run the P2 read and confirm the message refers to this issue, `plan` still equals the baseline, `Blocked` is only the plan-approval block and the plan has no unresolved Open questions; if any check fails, ask instead of acting. `Blocked` counts as only the plan-approval block when no comment posted after the latest plan comment asks a question, reports a failed baseline or a timeout, or carries an environment-block marker (`<!-- orchestrator:env-block`): read the comments after the plan and judge them, as in P2.
  - Post the mirror comment on the issue as in [Blocked Label](#blocked-label) P4.
  - Remove the label: `gh issue edit <number> --repo <owner/repo> --remove-label Blocked`.
  - If the repo has a Workflow board, set the workflow status to **Approved** with `cfwf workflow-status --set --repo <owner/repo> --issue <number> --status Approved` (see [Workflow Board](#workflow-board)).
  - Stop the loop and continue as in P4.

  This is the one documented exception to the rules that only a human clears `Blocked` ([Plan First](#issue-workflow-plan-first-new-issues-only) P4, [Blocked Label](#blocked-label) P2 and P4) and to the never-remove-labels rules in [task-workflow.instructions.md](task-workflow.instructions.md#label-management-mandatory): the human's chat instruction is the explicit action and the agent carries out the label and board changes on their behalf. It covers only the plan-approval `Blocked` of an issue in an interactive session; any other `Blocked` (a question, a failed baseline, an environment block) still waits for the human to clear it.

### PR Workflow: AI Review Loop

After all code changes are pushed and all required CI checks pass (a required check that skips while the PR is a draft counts as passed here, but it has not run yet: it first runs once [Phase E](#phase-e-mark-ready) marks the PR ready), **before** enabling auto-merge:

- The Orchestrator runs each review and judges convergence and thrash, but hands every file change, commit and push to the [review-fix route](task-workflow.instructions.md#review-fix-route), because the Orchestrator never implements directly.
- In an [interactive session](#waiting-for-approval-in-an-interactive-session), after each push the loop makes, hand CI to [CI Monitor](#ci-monitor), stating the run mode and the phase and step to resume at, and pause the loop until CI Monitor returns, because nothing else would watch that run or return control to the loop. CI Monitor's all-pass rule then decides whether the loop resumes there or restarts at Phase A. An unattended run carries on within the loop without waiting, because the PR stays in draft with auto-merge off until Phase E, and [Code Fixer](#code-fixer) and [CI Debugger](#ci-debugger) turn auto-merge off and convert to draft before any later fix, so GitHub cannot merge a fix the loop has not reviewed.

#### Phase A: Simplify (up to `MAX_SIMPLIFY_ITERATIONS` rounds)

- **P1.** Update Workflow board to **AI Simplify** (if the repo has a Workflow board).
- **P2.** Have Code Fixer run `/simplify` against the diff and keep its edits. `/simplify` applies reuse, simplification, efficiency, and altitude cleanups directly rather than just reporting them, so it has to run in the role that edits files, not in the Orchestrator. Code Fixer also applies [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the modified files.
- **P3.** If `/simplify` changed any files: send the edits through the rest of the [review-fix route](task-workflow.instructions.md#review-fix-route), with Changelog (correction) run against the resulting diff, then return to P2 to re-run against the resulting diff.
- **P4.** Once `/simplify` makes no further changes: have Code Fixer run the [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) for each construct in the net Phase A diff (the commits since P1), not per round, because rounds may revert each other and each sweep would widen the next round's diff. A change with no repeatable construct (a local rename or restructuring) has nothing to sweep. If the sweep changed files: send it through the review-fix route as in P3, then proceed to Phase B instead of returning to P2 (Phase B re-covers the swept code).
- **P5.** `/simplify` has its own iteration budget, separate from Phase B/C/D's own budgets below, because it is expected to run more rounds and give up without blocking:
  - Track each round's diff size (lines changed by that round's `/simplify` commit) against the previous round's.
  - Once `SIMPLIFY_THRASH_LIMIT` rounds have run, if the current round is thrashing (its diff is flat or larger than the previous round's, i.e. not shrinking): give up immediately, even though `MAX_SIMPLIFY_ITERATIONS` has not been reached.
  - Otherwise, keep re-running up to `MAX_SIMPLIFY_ITERATIONS` rounds total; once that hard cap is reached without converging to no changes, give up regardless of whether the diff was still shrinking.
  - Either way, giving up means: post a PR comment noting that simplify did not converge, run P4 in full (the sweep and its trip through the review-fix route) on the diff as it currently stands, then proceed to Phase B. Do not add `Blocked` and do not `STOP`: non-convergence in Phase A never blocks the PR, because `/code-review` in Phase B re-covers the same reuse/simplification/efficiency categories as a safety net (see Conflict Resolution below). Phases B and C below have their own, similarly non-blocking, self-detected-non-convergence exit; exhausting either phase's numeric round cap still blocks (see [Phase B's P4](#phase-b-convergence) and [Phase C's P4](#phase-c-convergence)). Phase D's coverage gate has its own, separately documented blocking conditions, not limited to cap exhaustion (see [Phase D's P3](#phase-d-on-failure)).

#### Phase B: Code review (up to `MAX_CODE_REVIEW_ITERATIONS` rounds)

- **P1.** Update Workflow board to **AI Review** (if the repo has a Workflow board).
- **P2.** Run: `/code-review --comment`. This intentionally re-covers the reuse/simplification/efficiency categories Phase A's `/simplify` already applied: `/simplify` fixes silently, and this step verifies nothing was missed and separately checks correctness, which `/simplify` does not (security and compliance are not covered by either command; they remain Phase C's job). Expect P2 to usually find nothing in the reuse/simplification/efficiency categories Phase A already handled. Also apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the modified files.
- **P3.** If NO findings were posted: proceed to Phase C.
- **P4.** <a id="phase-b-convergence"></a>Otherwise, judge convergence yourself from the PR's history of prior code-review comments: are this round's findings substantively new/distinct, or substantially a repeat of findings already reported (and left unresolved, or fixed and now recurring) in an earlier round? `MIN_REVIEW_CONVERGENCE_ROUNDS` must be set below `MAX_CODE_REVIEW_ITERATIONS`; otherwise the round-cap bullet below always fires first and the non-blocking exit can never trigger.
  - If `MAX_CODE_REVIEW_ITERATIONS` rounds have already run (judged from the PR's history of code-review comments) and findings remain, whether or not this round's findings are themselves new: post a PR comment listing the unresolved findings, add `Blocked` label, and **STOP**:

    ```bash
    gh pr edit <number> --repo <owner/repo> --add-label Blocked
    ```

  - Otherwise, if substantially repeating a prior round (not finding anything new; not converging) AND at least `MIN_REVIEW_CONVERGENCE_ROUNDS` rounds have now run: post a PR comment summarising the unresolved findings and stating that code review is not converging, advance the board to **AI Security Review** (if the repo has a Workflow board), post a one-line status comment, then proceed to Phase C. Do NOT add `Blocked`: this means no new correctness issues are surfacing, not that a known one is safe to ignore; the posted comment is what carries the unresolved findings forward to Human Review.
  - Otherwise (either substantially new, or a repeat but fewer than `MIN_REVIEW_CONVERGENCE_ROUNDS` rounds have run so far, so one failed fix attempt is not yet enough evidence to give up, and the round cap has not been reached): hand the findings, grouped by construct, to Code Fixer through the [review-fix route](task-workflow.instructions.md#review-fix-route), one fix change set per construct, each with a [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) (a finding that only re-reports the swept construct in files a sweep touched is not substantively new for the convergence judgment above; a new bug in those files is). Once the route has pushed, return to P2.

#### Conflict Resolution: Simplify/Code Review vs. Static Analyzer

If a change proposed by `/simplify` (Phase A) or a finding raised by `/code-review` (Phase B) would conflict with a rule enforced by the build-time static analyzer stack (Roslynator, SonarAnalyzer, Meziantou, Threading, Security Code Scan, and others; see [task-workflow.instructions.md](task-workflow.instructions.md#never-truncate-testcommit-commands-mandatory)) or by `FunFair.CodeAnalysis` (see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md)), the static analyzer's rule always wins: do not apply the conflicting simplify/code-review suggestion, and keep the analyzer-compliant code as-is.

#### Phase C: Security review (up to `MAX_SECURITY_REVIEW_ITERATIONS` rounds)

- **P1.** Update Workflow board to **AI Security Review** (if the repo has a Workflow board).
- **P2.** Run: `/security-review`. Also apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the modified files.
- **P3.** If NO findings are reported: proceed to Phase D.
- **P4.** <a id="phase-c-convergence"></a>This mirrors Phase B's [P4](#phase-b-convergence) exactly (substituting security-review for code-review); keep both in sync when editing either. Judge convergence yourself from the PR's history of prior security-review comments: are this round's findings substantively new/distinct, or substantially a repeat of findings already reported (and left unresolved, or fixed and now recurring) in an earlier round? `MIN_REVIEW_CONVERGENCE_ROUNDS` must be set below `MAX_SECURITY_REVIEW_ITERATIONS`; otherwise the round-cap bullet below always fires first and the non-blocking exit can never trigger.
  - If `MAX_SECURITY_REVIEW_ITERATIONS` rounds have already run (judged from the PR's history of security-review comments) and findings remain, whether or not this round's findings are themselves new: post a PR comment listing the unresolved findings, add `Blocked` label, **STOP**.
  - Otherwise, if substantially repeating a prior round (not finding anything new; not converging) AND at least `MIN_REVIEW_CONVERGENCE_ROUNDS` rounds have now run: post a PR comment summarising the unresolved findings and stating that security review is not converging, advance the board to **AI Coverage** (if the repo has a Workflow board), post a one-line status comment, then proceed to Phase D. Do NOT add `Blocked`: the same principle as Phase B's exit applies here (see Phase B's [P4](#phase-b-convergence) above).
  - Otherwise (either substantially new, or a repeat but fewer than `MIN_REVIEW_CONVERGENCE_ROUNDS` rounds have run so far, so one failed fix attempt is not yet enough evidence to give up, and the round cap has not been reached): post findings as a PR comment if not already inline, then hand the findings, grouped by construct, to Code Fixer through the [review-fix route](task-workflow.instructions.md#review-fix-route), one fix change set per construct, each with a [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) (a finding that only re-reports the swept construct in files a sweep touched is not substantively new for the convergence judgment above; a new bug in those files is). Once the route has pushed, return to P2.

#### Phase D: AI Coverage (up to `MAX_COVERAGE_ITERATIONS` rounds)

- **P1.** Update Workflow board to **AI Coverage** (if the repo has a Workflow board).
- **P2.** Run the [AI Coverage Phase Decision Procedure](coverage-ratchet.instructions.md#ai-coverage-phase-decision-procedure-mandatory) from [coverage-ratchet.instructions.md](coverage-ratchet.instructions.md): compare the branch's live per-language coverage against the Overall figures in `COVERAGE.md` on `origin/main` (non-code-only branches — dependency bumps, workflow/SQL/shell/Docker/docs-only changes — and a missing `COVERAGE.md` both skip the comparison and pass automatically — see that file's [Non-Code-Only Branches](coverage-ratchet.instructions.md#non-code-only-branches-skip-dont-measure) and bootstrap rules).
- **P3.** <a id="phase-d-on-failure"></a>On failure (any language's branch coverage below its baseline): the procedure judges the round cap and the round-over-round trend itself and acts accordingly (full branching, including the round-cap `Blocked` exit and the judged-unlikely-to-close `Blocked` exit, is at the decision procedure's [P6](coverage-ratchet.instructions.md#coverage-decision-on-failure)), unlike Phase B/C's findings-based judgment, because coverage has a natural numeric signal to trend on. **STOP** after the procedure's status/`Blocked` comment either way; a status-comment outcome means the next cycle picks the resulting Development work back up, a `Blocked` outcome needs a human.
- **P4.** On success: the procedure moves the board to **Human Review** and posts a status comment; proceed to Phase E.

#### Phase E: Mark ready

Only once all four phases have completed without a `Blocked` outcome (each phase passed outright, or exited via its own non-blocking convergence path noted in a PR comment, or there were no reviewable changes):

- **P1.** Safety net (belt-and-suspenders on top of the Code Reviewer Compliance check above): confirm `.deleteme.now` is not present in `git diff origin/main...HEAD --name-only` (see [Changelog](#changelog)); if it is still present, have Code Writer remove it and send it through the [review-fix route](task-workflow.instructions.md#review-fix-route) without Changelog (correction), with Committer giving it its own commit, because a template-skip item has no changelog entry to correct, then continue.
- **P2.** Update Workflow board to **Human Review** (if the repo has a Workflow board), unless Phase D already moved it there on success.
- **P3.** Mark the PR ready, but do not enable auto-merge yet:

   ```bash
   gh pr ready <number> --repo <owner/repo>
   ```

   <a id="phase-e-reviewed-head"></a>Then post a status comment in the form `### AI Review Loop: reviewed <full head SHA>`, reading the SHA with `gh pr view <number> --repo <owner/repo> --json headRefOid --jq '.headRefOid'`, even when the PR was already ready and `gh pr ready` changed nothing, because this comment is the record of which head the loop last reviewed, and neither the PR's draft state nor its `ready_for_review` events move forward when the loop re-runs on a PR that is already ready.

   <a id="unreviewed-commit"></a>The PR has an unreviewed commit when its head SHA differs from the SHA in the latest accepted `### AI Review Loop: reviewed` comment, or when there is no such comment, because unreviewed change must not merge. Accept such a comment only when its author login is the bot login or a trusted commenter, as [Trusted Commenters](#trusted-commenters) sources them (P2 for the bot login, P1 and P3 for trusted commenters), because anyone else could post one naming a head the loop never reviewed. The bot login counts here, unlike in P2, because P3 above posts this comment from it. A rebase therefore leaves an unreviewed commit and re-runs the loop, which is acceptable because re-reviewing is safe while wrongly skipping review is not, and the check needs no local git state.

   Phase E is the only step that takes a PR out of draft, because every other role leaves it as draft until the loop has reviewed every commit. Enable auto-merge only later, in [P4](#phase-e-enable-auto-merge), once every required check has a result produced after the PR was marked ready, because GitHub counts a check skipped on a draft as passed, so enabling auto-merge straight after marking ready can merge before lint, secret scanning and dependency review have run.

   Then, in an [interactive session](#waiting-for-approval-in-an-interactive-session), hand CI to [CI Monitor](#ci-monitor), stating the run mode and that the required checks skipped while the PR was a draft are now running for the first time, without stating that all required checks passed, because those checks have never run and nothing else would watch them. CI Monitor handles any failure as its P3 describes, the same as the CI failure row of the [routing rules](task-workflow.instructions.md#routing-rules). An unattended run needs no hand-off: stop here, because `oneshot` re-invokes the agent when the checks change, and that invocation runs P4.
- **P4.** <a id="phase-e-enable-auto-merge"></a>Enable auto-merge once every required check on the PR's current head has a result completed at or after the time the PR was last marked ready (a post-ready result), and none of those results failed, because a result from before that moment is the draft result, which GitHub counts as passed even when the check only skipped because the PR was a draft. A post-ready skip is a real result and counts as passed, because some required checks skip on a ready PR by design and never produce anything else, for example `include-changelog-entry` on a PR labelled `Changelog Not Required`, or `lint-code` on a `release/*` branch. Judge failure from the plain `gh pr checks <number> --repo <owner/repo> --required` as [CI Checks](#ci-checks-mandatory) describes. Read when the PR was last marked ready from the first command below, which prints its last `ready_for_review` timeline event. If it prints nothing, the ready time is not known yet: treat every required check as having no post-ready result and look again on the next tick or invocation, without keeping the empty result, because PR Submitter always opens the PR as a draft, so an empty read only means the event has not shown up yet ([GitHub State Lags Behind Writes](github-cli.instructions.md#github-state-lags-behind-writes-mandatory)). Read when each required check completed from the second:

   ```bash
   gh api repos/<owner>/<repo>/issues/<number>/timeline --paginate --jq '.[] | select(.event == "ready_for_review") | .created_at' | tail -n 1
   gh pr checks <number> --repo <owner/repo> --required --json name,completedAt --jq '.[] | "\(.completedAt)\t\(.name)"'
   ```

   A required check has a post-ready result when any of its rows completed at or after the ready time, because the same check can have one row from the run before the PR was marked ready and another from the run after it, and the run that marking ready starts can finish its skipped jobs within the same second. Compare the two times as text, because both are UTC ISO 8601 times ending in `Z`. A check that has not completed yet has no post-ready result. Use the `gh pr checks --json` output only for the completion times, never to judge pass or failure, because gh exits 0 with `--json` whatever the checks' state. Then:

   ```bash
   gh pr merge --auto --merge <number> --repo <owner/repo>
   ```

  - In an interactive session, do this when CI Monitor returns control after confirming all required checks pass with every one of them having a post-ready result (its P2 and P3), because that confirmation is the first moment the post-ready checks are known to have run and passed.
  - In an unattended run, do this on a later invocation when the PR is open and not a draft, `gh pr view <number> --repo <owner/repo> --json autoMergeRequest --jq '.autoMergeRequest'` prints `null`, the PR has no [unreviewed commit](#unreviewed-commit), [CI Checks](#ci-checks-mandatory) confirms all required checks passed, and every required check has a post-ready result as above, because that combination is what a PR waiting at this step looks like and `oneshot` re-invokes the agent when its checks change. While a required check has no post-ready result, stop and leave the PR as it is, because the post-ready run has not produced it yet.
  - If the PR has an [unreviewed commit](#unreviewed-commit), for example from a Rebase Agent or human push, first turn auto-merge off and convert the PR to draft exactly as [Code Fixer](#code-fixer) does, then run the [AI Review Loop](#pr-workflow-ai-review-loop) for it instead of enabling auto-merge, because unreviewed change must not merge, and a draft PR is what lets the loop's own Phase E mark it ready again and start the post-ready checks.

   The PR is already ready at this point, which matters because GitHub rejects enabling auto-merge on a draft PR. If enabling auto-merge fails (auto-merge not supported), leave the PR ready for a human to merge.

### Workflow Board

Each generated `CLAUDE.md` may contain Workflow board data in this format:

```text
Workflow board (see agent-roles.instructions.md for update commands):
  WF_PROJECT_ID=PVT_xxx
  WF_PROJECT_NUMBER=<number>
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

Its presence tells you the repo has a Workflow board (the "Board configured" branches above), but nothing that uses `cfwf` needs the ids. If it is absent, still try `cfwf`: only if `cfwf` finds no "Workflow" project linked to the repo is there no board, in which case skip board updates silently for the rest of the session.

#### Updating and Reading the Board with `cfwf`

**Always use `cfwf` for the Workflow board; never hand-compose `gh project`, `gh repo view --json projectsV2` or `gh api graphql` commands for it.** Every command names the item as `--repo <owner/repo>` plus `--pr <n>` or `--issue <n>`.

```bash
# Move an issue or PR to a status (the option's display name, matched without regard to case)
cfwf workflow-status --set --repo <owner/repo> (--pr <n> | --issue <n>) --status "AI Review"

# Read the current status
cfwf workflow-status --check --repo <owner/repo> (--pr <n> | --issue <n>)
```

- `--set` adds the item to the board if it is not already there and sets the workflow status, then prints `Set <url> to <status>`. Exit 0 means GitHub accepted the write; a non-zero exit means the write failed. It deliberately does not read the value back, because GitHub lags behind writes: see [GitHub State Lags Behind Writes](github-cli.instructions.md#github-state-lags-behind-writes-mandatory).
- `--check` prints the current workflow status and exits non-zero if the item is not on the board. The output starts with the status name: match the name exactly and ignore anything after it.

### On Hold Label

An issue labelled `On Hold` is not ready to be worked on: it needs further thought or cannot be implemented at this time. Do not pick up or assign yourself to an `On Hold` issue. If the label is removed, re-evaluate priority and proceed normally.

### Blocked Label

When asking a question in a PR or issue comment and waiting for an answer before continuing:

- **P1.** Add the `Blocked` label to the PR or issue immediately after posting the question:
  - Issue: `gh issue edit <number> --repo <owner/repo> --add-label "Blocked"`
  - PR: `gh pr edit <number> --repo <owner/repo> --add-label "Blocked"`
- **P2.** Do **not** continue working on the item until the label is removed.
- **P3.** Use **only** the `Blocked` label for this purpose; do **not** use labels like `do not merge`, `needs review`, or any other substitute. The orchestrator only recognises `Blocked` when deciding whether to skip an item.
- **P4.** **Live-chat approval is not sufficient on its own.** If a human answers or approves in a live chat session rather than posting a GitHub comment directly, post the comment yourself, quoting the live instruction, before resuming work (and before asking for `Blocked` to be removed). The record must survive even if the chat session is lost. Exception (waives only the human-clears-`Blocked` requirement, never the mirror comment): plan-approval `Blocked` in an interactive session; see [Waiting for Approval in an Interactive Session](#waiting-for-approval-in-an-interactive-session) P5.
- **P5.** <a id="blocked-label-cite-rule"></a>Whenever `Blocked` is added, the accompanying comment (or, for a new issue, its body) must name the specific instruction that requires the stop, as a link to its section; for an issue held under [AI-Initiated Issues](git.instructions.md#ai-initiated-issues-mandatory) that instruction is AI-Initiated Issues itself. A judgement such as "out of scope", "pre-existing" or "also fails on main" is never such an instruction; if no instruction requires the stop, do not add `Blocked` and carry on with the work, because an unjustified `Blocked` stalls the item until a human notices.

### Environment/Infrastructure Block Marker (MANDATORY, PRs only)

When a Blocked-ing failure is diagnosed as an environment/infrastructure problem, such as a bug in the container image, a missing tool, or a transient infra issue, rather than a bug in the PR's own code, add a machine-readable marker alongside the diagnosis so `oneshot` can auto-clear `Blocked` once the fix has actually shipped, instead of the PR sitting blocked until a human happens to notice:

- **P1.** Post the full human-readable diagnosis as normal: root cause, evidence, and (if known) the fix needed.
- **P2.** Append a single trailer line to that same comment:

  ```text
  <!-- orchestrator:env-block image-sha=${IMAGE_SHA_DEVELOPMENT_AGENT} -->
  ```

  Read `IMAGE_SHA_DEVELOPMENT_AGENT` from your own container environment (the same value printed at session start as part of "Image layer provenance"); this records which image build was current when you made the diagnosis.
- **P3.** Apply `Blocked` exactly as in the section above.
- **P4.** Use this marker **only** for a genuine environment/infrastructure diagnosis. `oneshot` auto-clears `Blocked` the moment it observes a differently-built agent image, with no further human involvement; marking a real code question or design decision this way would resume work before a human actually answered it.

This convention only applies to PRs (there is no container session, and therefore no image to diagnose against, before a PR/branch exists). Everything else about the Blocked-label convention above is unchanged.

### Human Comment Requests: Run First (MANDATORY)

Before processing CI checks or continuing the review loop, scan **all** comments on the current PR and its linked issue(s) from [trusted commenters](#trusted-commenters) for ad-hoc requests to create a new GitHub issue.

A request is identified by any natural-language phrasing such as: "raise an issue", "create an issue", "add an issue", "open an issue", "file an issue", or similar variants (case-insensitive).

For each such request that has not already been actioned (i.e. no reply from you linking to a newly created issue):

- **P1.** Search for an existing open **or closed** issue covering the same topic; do not create duplicates.
- **P2.** If no duplicate exists, create the issue immediately:

  ```bash
  gh issue create --repo <owner/repo> \
    --title "<concise title from the request>" \
    --body "<description from the request>" \
    --label "<priority label from the request, or 'Medium' if unspecified>"
  ```

- **P3.** Reply to the original comment with the new issue number. Use the correct command depending on where the request appeared:

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

- **P4.** Only after all such requests are actioned, continue with the normal CI/review workflow.

The same rule applies when picking up an **issue**: if any comment on that issue requests a sub-issue to be raised, create it and reply (using `gh issue comment`) before proceeding with implementation work.

### Comment Replies (MANDATORY)

Reply to every PR or issue comment that prompted an action. "Every PR or issue comment" spans both comment surfaces: top-level PR/issue comments and review summaries (`gh pr view <n> --json comments,reviews`) **and** inline/diff-level review comments (`gh api repos/<owner>/<repo>/pulls/<n>/comments`); a review can carry an empty top-level body with the actual feedback only in an inline comment, so both must be checked before concluding there is nothing to reply to.

- Code change made: reply with `Fixed in <commit-sha>: <one sentence describing what changed and why>`.
- [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) found further occurrences: add `Swept in <sha>: <files touched>` on the next line, one line per commit that carries sweep hunks (the fix SHA when every hit was in a file the fix touched); the per-file reasons are in that commit's body.
- Already fixed by an earlier sweep in this PR (no new commit): reply with `Already swept in <sha>`, citing the commit whose body carries the `Construct:` line.
- Question answered inline (no code change): reply with the full answer.
- No reply means no acknowledgement; always close the loop.

### CI Checks (MANDATORY)

The `oneshot` pre-agentic gate (from `credfeto/credfeto-orchestrator`) normally blocks agent invocation while CI checks are pending, so in an unattended run the rules below are a safety net for edge cases. An interactive session has no such gate. A role running as a sub-agent uses the run mode stated in its hand-off (see [Waiting for Approval in an Interactive Session](#waiting-for-approval-in-an-interactive-session)).

When working on a PR, check CI state **once**, required checks only, because the PR is mergeable without the optional ones and the plain output does not say which checks are required:

```bash
gh pr checks <number> --repo <owner/repo> --required
```

Judge the result by the exit code of this plain form together with its state column, not `--json`, because with `--json` gh exits 0 even when a check has failed or is pending. It exits 0 when every reported required check passed, was skipped or was cancelled, 8 when one is pending, and 1 when one failed or when it prints `no required checks reported on the '<branch>' branch` or `no checks reported on the '<branch>' branch`. It also exits 1 on a gh or API error (authentication, network, proxy, rate limit, or a PR that does not exist), which it hits before it prints any check rows. gh only reports checks that have already registered on the PR's head commit, so a required workflow that has not been queued yet is missing from the list rather than shown as pending.

- A row whose state column (the second tab-separated field) reads `fail` means that required check failed, whatever the exit code, because gh prints `fail` there for a cancelled check as well as a failed one but does not count a cancelled check towards exit code 1. GitHub treats a cancelled required check as unsatisfied, so the PR cannot merge until it is re-run. This holds for the output an agent's shell receives, which is not a terminal; on a terminal gh shows `-` for both skipped and cancelled checks, so the two cannot be told apart there.
- Either `no ... checks reported` message counts as pending, never as pass or failure, because it usually means the run for the head commit has not registered yet.
- Exit code 1 with no check rows in the output and no `no ... checks reported` message is a gh or API error, not a failed check, because gh failed before it could read any check.
- If the repo has no required checks configured at all, drop `--required` and judge all checks instead, because otherwise gh reports `no required checks reported` for ever. It has none when neither the base branch's protection (`gh api repos/<owner>/<repo>/branches/<base> --jq '.protection.required_status_checks.contexts'`) nor its rulesets (a `required_status_checks` rule in `gh api repos/<owner>/<repo>/rules/branches/<base>`) list any. Look this up once per PR, not on every check.

Then act immediately; do **not** busy-loop, sleep, or use `--watch`, in any mode, because a blocking wait holds the session for the whole CI run:

- All required checks passed → accept it only if it is still true on the next check after it was first seen, because a fast required workflow can pass before a slower one has even been queued. In an unattended run, the `oneshot` gate's check before invoking the agent is the first sighting, so this check confirms it; proceed with the next step. In an interactive session this check is the first sighting, so hand the PR to [CI Monitor](#ci-monitor), stating in the hand-off that all required checks passed, so that its first tick confirms it.
- Any required check failed, including a cancelled one → first count the `### CI Debugger:` status comments for that check on the PR, as [CI Debugger](#ci-debugger-round-count) defines which ones count; if there are already 3, treat the PR as [CI consistently failing](#ci-consistently-failing) instead of routing the check again, in every run mode, because this loop spans separate invocations and only the PR's comment history persists between them. Otherwise route it as the CI failure row of the [routing table](task-workflow.instructions.md#routing-rules) (CI Debugger finds the cause and pushes a fix, or re-runs a cancelled check that needs no code change, as [CI Debugger](#ci-debugger) describes) rather than fixing it yourself, because the Orchestrator never implements directly, and post a status comment, even while other checks are still pending, because waiting for the slowest check would delay the fix by the whole CI run. Do not wait for the new run to complete. Then:
  - Unattended run → stop; `oneshot` re-invokes the agent once the new run finishes.
  - [Interactive session](#waiting-for-approval-in-an-interactive-session) → if CI Debugger pushed a fix or re-ran a check, hand the PR to [CI Monitor](#ci-monitor) instead of stopping, stating in the hand-off that the session is interactive, because nothing else would watch the new run or return control to the Orchestrator. If CI Debugger escalated, handle the escalation yourself instead (for example by following [Environment/Infrastructure Block Marker](#environmentinfrastructure-block-marker-mandatory-prs-only) for an environment diagnosis), because CI Monitor would see the unchanged failure and hand it straight back to CI Debugger.
- Any required check pending or in_progress, or none reported yet, and none failed:
  - Unattended run → stop silently; do not post a status comment. CI checks are bound by GitHub's own timeouts and will eventually pass, fail, or time out without agent intervention, and `oneshot` re-invokes the agent once they do.
  - [Interactive session](#waiting-for-approval-in-an-interactive-session) → hand the PR to [CI Monitor](#ci-monitor) instead of stopping, stating in the hand-off that the session is interactive.
- gh or API error → report the error rather than routing a CI failure, because no check has failed and CI Debugger would look for a failure that does not exist.
- <a id="ci-consistently-failing"></a>CI consistently failing and cannot be fixed → mark the PR blocked: `gh pr edit <number> --repo <owner/repo> --add-label "Blocked"`. A required check still failing after 3 CI Debugger rounds for that check on the PR also counts, whether [CI Monitor](#ci-monitor) reports it or the failed-check branch above finds it, because further rounds would only repeat the cycle.

An analyzer findings comment (`<!-- sarif-summary: ... -->`) on the PR is handled as in [Suppressed Analyzer Findings](code-quality.instructions.md#suppressed-analyzer-findings-sarif-summary-mandatory).

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
- After fixing a bug, run the [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) and append its sweep record to the hand-off report.
- Apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the files written or changed.
- Do not commit, push, or update the changelog; hand off to Code Tester when done.
- List each pre-existing bug found outside the current change's scope in the hand-off report for Orchestrator rather than fixing it, because the report is free text with no dedicated field and an unlisted bug is lost; see [Pre-Existing Bugs Found During Work](code-quality.instructions.md#pre-existing-bugs-found-during-work-mandatory).

## Code Tester

- Run build and all tests after Code Writer or Code Fixer finishes.
- Check coverage against `git diff origin/main...HEAD`.
- Apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the changed files.
- On build failure, test failure, or uncovered code: report file paths/line ranges to the calling agent; stop, do not proceed.
- Loop with Code Writer until build passes, all tests pass, and all new/changed code is covered.
- Carry any sweep record and any pre-existing bug list in the incoming hand-off through to the outgoing report unchanged, because the next role only sees what this report passes on and the Orchestrator collects each pre-existing bug list from the reports it receives.
- Do not modify code or tests; report and verify only.

## Code Reviewer

- Run `git diff origin/main...HEAD`.
- Apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the changed files.
- Launch all the sub-agents **in parallel**: Reuse, Quality, Efficiency, Correctness, Security, Compliance.
- Each sub-agent reports `{"clean": true}` or `{"clean": false, "findings": [{"file": "...", "line": ..., "issue": "...", "suggestion": "..."}]}`.
- Fix each construct (real findings grouped by construct) as its own change set, with a [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) handed over as for Code Writer; skip false positives. Re-run Code Tester after fixes. The outgoing report carries every sweep record and every pre-existing bug, incoming and own, unchanged.
- If fixing a finding requires knowledge outside the instruction files, invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, leave the finding unresolved and escalate to Orchestrator with the explanation.
- Report `{"clean": true, "sweeps": [...], "preExistingBugs": [...]}` or `{"clean": false, "fixes": [...], "sweeps": [...], "preExistingBugs": [...]}`, where `sweeps` carries every sweep record and `preExistingBugs` lists each pre-existing bug reported but not fixed (file, line, description), incoming (from a Code Writer or Code Fixer hand-off) and own, because without its own field such a bug is either dropped or misread as a fix. Cap at 5 iterations.
- After 5 iterations, report any unresolved findings to the Orchestrator; Orchestrator adds each as a PR comment for human consideration.
- Report a pre-existing bug found outside the current change's scope to Orchestrator in `preExistingBugs` rather than fixing it; see [Pre-Existing Bugs Found During Work](code-quality.instructions.md#pre-existing-bugs-found-during-work-mandatory).

### Code Reviewer: **Reuse**

- Identify opportunities to reuse existing code instead of writing new code. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Reuse: Critical Instructions

- MINIMISE FALSE POSITIVES: Only flag cases where an existing utility or helper clearly covers the same need without modification.
- FOCUS ON IMPACT: Prioritise reuse that eliminates duplication across multiple call sites.
- EXCLUSIONS: Do NOT flag cases where the existing code would require modification to be reused; that is a refactor, not reuse.

#### Reuse: Categories

- Utilities: helper methods or functions already present in the codebase being reimplemented.
- Library functions: standard library or existing dependency features being reimplemented.
- Shared components: duplicated domain logic that belongs in a shared layer.
- Extension points: existing abstractions (interfaces, base classes) not being used where applicable.

### Code Reviewer: **Quality**

- Identify code quality issues. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Quality: Critical Instructions

- MINIMISE FALSE POSITIVES: Only flag clear violations, not stylistic preferences.
- FOCUS ON IMPACT: Prioritise issues that harm maintainability or introduce technical debt.
- EXCLUSIONS: Do NOT report formatting or naming style issues; those are enforced by linting tooling.

#### Quality: Categories

- Duplication: copy-paste code that should be extracted.
- Responsibility: leaky abstractions or methods doing more than one thing (Single Responsibility Principle).
- State: redundant or unnecessary mutable state.
- Complexity: overly nested logic or methods too long to reason about.

### Code Reviewer: **Efficiency**

- Identify inefficiencies. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Efficiency: Critical Instructions

- MINIMISE FALSE POSITIVES: Only flag issues with measurable impact, not micro-optimisations.
- FOCUS ON IMPACT: Prioritise hot paths, loops, and data access patterns.
- EXCLUSIONS: Do NOT report theoretical inefficiencies in cold paths that are not performance-critical.

#### Efficiency: Categories

- Algorithms: non-optimal algorithms where a better alternative exists and data size warrants it.
- Data structures: inappropriate structures causing unnecessary overhead (e.g. linear search on a list where a set or dictionary fits).
- Redundant work: repeated calculations or queries that could be cached or hoisted.
- Memory: unnecessary allocations or large object graphs held longer than needed.

### Code Reviewer: **Correctness**

- Identify logic errors. Scope: newly changed code for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Correctness: Critical Instructions

- MINIMISE FALSE POSITIVES: Only flag cases where the logic provably does not match the intent of the change.
- FOCUS ON IMPACT: Prioritise errors that could cause incorrect results, data corruption, or silent failures.
- EXCLUSIONS: Do NOT flag style or structural issues; focus solely on whether the code does what it is supposed to do.

#### Correctness: Categories

- Boundary conditions: off-by-one errors, incorrect loop bounds, fencepost errors.
- Conditionals: incorrect boolean logic, missing negation, wrong operator.
- Edge cases: null/empty input, zero values, empty collections, missing default cases.
- Business logic: code that does not match the intent described in the issue or PR.

### Code Reviewer: **Security**

- Perform a security-focused review to identify HIGH-CONFIDENCE security vulnerabilities with real exploitation potential. Scope: security implications newly added by the PR for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Security: Critical Instructions

- MINIMISE FALSE POSITIVES: Only flag issues where you're >80% confident of actual exploitability.
- FOCUS ON IMPACT: Prioritise vulnerabilities that could lead to unauthorised access, data breaches, or system compromise.
- EXCLUSIONS: Do NOT report Denial of Service (DOS) vulnerabilities, rate limiting issues, or secrets/credentials committed in code (private keys, passwords, API keys); these are covered by dedicated non-agentic tooling.

#### Security: Categories

- Input Validation: SQL injection, command injection, path traversal, XSS.
- Authentication: Bypass logic, privilege escalation, JWT flaws.
- Crypto: Weak algorithms, improper key storage.
- Injection: Deserialisation, eval injection, XML parsing issues.

### Code Reviewer: **Compliance**

- Check that files comply with all applicable rules in the `.ai-instructions` instruction files. Scope: newly changed files for Code Reviewer; full file set when dispatched by Repo Auditor.

#### Compliance: Critical Instructions

- MINIMISE FALSE POSITIVES: Only flag clear violations of explicit rules, not inferred or implied guidance.
- FOCUS ON IMPACT: Prioritise violations that would cause the files to fail review or break established conventions.
- EXCLUSIONS: Do NOT re-report issues already in scope for Reuse, Quality, Efficiency, Correctness, or Security sub-agents.

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
- Process groups sequentially. For each group, apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the group's files, then launch the Code Reviewer sub-agents (Reuse, Quality, Efficiency, Correctness, Security, Compliance) **in parallel**.
  - The "newly changed files" scope does not apply; sub-agents review the full file set for the group.
- Do NOT fix findings. For each group that has findings, raise one GitHub issue:
  - Title: `Audit: <group-name> - <brief summary>`
  - Body: all findings from all sub-agents for that group, organised by sub-agent.
  - Label: `audit`
- Skip groups where all sub-agents report `{"clean": true}`.

## Code Fixer

- Address requested changes on an existing PR; this includes GitHub `CHANGES_REQUESTED` review status, verbal/chat requests for changes on an open PR, and [AI Review Loop](#pr-workflow-ai-review-loop) findings (Phase A's `/simplify` edits and Phase B and C findings), which reach Code Fixer through the [review-fix route](task-workflow.instructions.md#review-fix-route).
- Fetch **both** comment surfaces before deciding there is nothing to address: top-level PR comments and review summaries (`gh pr view <n> --repo <owner/repo> --json comments,reviews,reviewDecision`) **and** inline/diff-level review comments (`gh api repos/<owner>/<repo>/pulls/<n>/comments`). A reviewer can submit a `CHANGES_REQUESTED` review with an empty top-level summary and put their actual feedback only in an inline diff comment; the review decision alone is enough to treat the PR as having unaddressed work, and the inline-comment endpoint is the only place its content is visible.
- If a fix requires knowledge outside the instruction files, invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, stop and escalate to Orchestrator with the explanation; do not partially apply the fix.
- Before starting, turn auto-merge off, then convert to draft (`gh pr ready <number> --repo <owner/repo> --undo`). Converting to draft alone is not enough, because GitHub does not turn auto-merge off when a PR becomes a draft, so the PR would merge as soon as it is marked ready again, before the AI Review Loop has reviewed the fix. Turn auto-merge off with `gh pr merge <number> --repo <owner/repo> --disable-auto` only when `gh pr view <number> --repo <owner/repo> --json autoMergeRequest --jq '.autoMergeRequest'` prints something other than `null`, because GitHub does not document what `--disable-auto` does on a PR with no auto-merge request.
- One fix change set per construct (comments grouped by construct), with a [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) handed over as for Code Writer. Apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the fixed files. Hand off to Code Tester after each fix and its sweep.
- Respond to **every** review comment without exception, per [Comment Replies](#comment-replies-mandatory). A reply that cites a SHA is posted once Committer has pushed, so the sweep record's file placement is final.
- List each pre-existing bug found outside the current change's scope in the hand-off report for Orchestrator rather than fixing it, because the report is free text with no dedicated field and an unlisted bug is lost; see [Pre-Existing Bugs Found During Work](code-quality.instructions.md#pre-existing-bugs-found-during-work-mandatory).

## Rebase Agent

- Rebase the named branch onto `origin/main`.
- CHANGELOG conflicts: keep entries from both sides.
- Version conflicts in dependency manifests, action pins, or runtime versions: take the latest secure candidate per [git-rebasing.instructions.md](git-rebasing.instructions.md#resolving-version-conflicts-when-merging-or-rebasing). If the chosen version breaks the build, report to Orchestrator; fixing build breakage is not the Rebase Agent's job.
- Any other conflict: report verbatim to Orchestrator; do not resolve.
- If the branch has an open PR, turn auto-merge off and convert the PR to draft exactly as [Code Fixer](#code-fixer) does before force-pushing, because a rebase changes the head, so the rebased commit is unreviewed and GitHub could otherwise merge it as soon as its checks pass, before the AI Review Loop reviews it.
- Force-push with `--force-with-lease` only after all conflicts are resolved.
- Does not run `pre-commit-check` or fix what it reports: that is the Post-Rebase Check in [After Every Rebase](git-rebasing.instructions.md#after-every-rebase-mandatory), which the Orchestrator runs once this role returns, because this role is mechanical and must not interpret or fix failures.

## CI Debugger

- Read full logs (`gh run view --log-failed`), identify root cause.
- Fix if code-related, with a [Pattern Sweep](code-quality.instructions.md#pattern-sweep-mandatory) committed after the fix per [Pattern Sweep Commits](git-commits.instructions.md#pattern-sweep-commits), since no Committer follows this role; apply [IDE MCP Code Analysis](code-quality.instructions.md#ide-mcp-code-analysis-mandatory) to the fixed files; escalate to Orchestrator with a clear description if environmental or infrastructure; use the Environment/Infrastructure Block Marker convention above so the block can auto-clear once the fix ships.
- Before pushing a code fix, turn auto-merge off and convert the PR to draft exactly as [Code Fixer](#code-fixer) does, because GitHub could otherwise merge the fix as soon as its checks pass, before the AI Review Loop reviews it. A re-run needs neither, because it adds no commit.
- After each pushed fix or re-run, post a one-line status comment on the PR naming each required check it addressed, using the check name exactly as `gh pr checks` reports it, in the form `### CI Debugger: <fix pushed|re-run started> for <check name>`, because [CI Monitor](#ci-monitor) and [CI Checks](#ci-checks-mandatory) count these comments to cap rounds for one check, and a fixed form is what lets them count them reliably. The Orchestrator's own status comment in [CI Checks](#ci-checks-mandatory) does not count towards the cap, because it records a routing decision rather than a round.
- <a id="ci-debugger-round-count"></a>Count a `### CI Debugger:` comment for a check towards the cap only when both of these hold:
  - Its author is accepted as for the [`### AI Review Loop: reviewed` comment](#unreviewed-commit), because anyone else could post one and get the PR marked `Blocked` before a single fix attempt.
  - It was posted after the PR's latest `auto_merge_enabled` timeline event, or since the PR opened if there is none, because Phase E's [P4](#phase-e-enable-auto-merge) enables auto-merge only once every required check has passed on the ready PR, so rounds for failures fixed before then must not use up the budget for a new failure. Do not count from the latest `### AI Review Loop: reviewed` comment or `ready_for_review` event instead, because Phase E posts both on every draft, fix, ready cycle, so the count would never reach the cap. Read the time with:

    ```bash
    gh api repos/<owner>/<repo>/issues/<number>/timeline --paginate --jq '.[] | select(.event == "auto_merge_enabled") | .created_at' | tail -n 1
    ```

- A cancelled required check counts as failed, as in [CI Checks](#ci-checks-mandatory). If nothing in the code caused the cancellation (a manual cancel or a runner shutdown), re-run it with `gh run rerun <run-id> --repo <owner/repo>` rather than pushing, because GitHub keeps the cancelled result on the head commit until the check runs again and the PR cannot merge until then; report the re-run to the calling role as you would a pushed fix.
- If a code-related fix requires knowledge outside the instruction files, invoke Coding Researcher first; do not guess or fabricate. If Coding Researcher returns **Not possible**, escalate to Orchestrator with the explanation.
- Fix a pre-existing bug that causes the CI failure as part of the current work, including one the change merely exposes, because leaving it would keep the PR's required checks failing with nothing permitted to clear them. Report any other pre-existing bug found outside the current change's scope to the calling role (Orchestrator, or CI Monitor, which passes it on to Orchestrator) rather than fixing it; see [Pre-Existing Bugs Found During Work](code-quality.instructions.md#pre-existing-bugs-found-during-work-mandatory).

## Changelog

Runs in two modes; both use `dotnet changelog` (see [changelog.instructions.md](changelog.instructions.md)) and never edit `CHANGELOG.md` manually. Neither mode commits (Committer's job) or runs build/tests (Code Tester's job).

- **Placeholder**: runs first, before Code Writer touches any code, so the branch/PR can exist from the start of work on the item. Add a stub entry (best-guess `Type`, message `TBD - to be finalized after review`). Hand off straight to Committer for a changelog-only commit, then PR Submitter to open the draft PR.
- **Correction**: replaces the placeholder (or a prior correction) once there is a real diff to describe. Runs after Code Tester and Code Reviewer are satisfied in the initial development loop, never before. Also re-runs after any AI Review Loop phase (Simplify, Code Review, Security Review — see [PR Workflow: AI Review Loop](#pr-workflow-ai-review-loop)) that actually changed files, so the entry keeps matching the diff those phases produced. Read `git diff origin/main...HEAD`, remove the previous entry and add the corrected one (`dotnet changelog` has no in-place edit).
- **Skip case**: if the work item qualifies for a skip under [changelog.instructions.md](changelog.instructions.md#when-to-skip) (template repo), commit a `.deleteme.now` placeholder file at the repo root instead of a `CHANGELOG.md` entry (a short delete-before-merge comment as its content). Hand off straight to Committer for a placeholder-only commit, then PR Submitter to open the draft PR. Code Writer removes `.deleteme.now` as part of its first real change set, for Committer to commit as usual. Correction is a no-op for these items, same as before.
- Both modes carry any sweep record and any pre-existing bug list in the incoming hand-off through to the outgoing report unchanged, because the next role only sees what this report passes on and the Orchestrator collects each pre-existing bug list from the reports it receives.

## Committer

- Use `git` CLI only; never `gh` or the GitHub API for commit/push.
- For the placeholder step (no code exists yet): commit the placeholder artefact alone: `CHANGELOG.md`, or `.deleteme.now` for template-skip repos (see [Changelog](#changelog)).
- Otherwise: commit the handed-over change set as one GPG-signed commit (Conventional Commits). When the hand-off carries sweep records, stage by whole file: everything except the sweep-only files is the fix commit (one per construct where change sets share no file; change sets that share a file form one fix commit whose body carries each `Construct:` line), then build once, then commit the sweep-only files as the sweep commit per [Pattern Sweep Commits](git-commits.instructions.md#pattern-sweep-commits), one per construct. Commit `CHANGELOG.md` as a separate GPG-signed commit whenever Changelog produced a correction alongside it.
- Push immediately after. Do not open the PR; that is PR Submitter's job.
- Do not use `--no-verify`. If a pre-commit hook fails: capture output, report to the producing agent, re-stage and retry. Escalate to Orchestrator after 3 failed cycles.

## PR Submitter

- Run after Committer has pushed.
- Wait up to 1 minute for GitHub to auto-create a PR (`gh pr list --head <branch>`); create one if absent.
- Title: Conventional Commits format matching the primary commit; for the placeholder-only commit that opens the PR before any code exists, base it on the issue title/expected Conventional Commits type instead, and correct it once the primary code commit lands if it differs. Body: summary + `Closes #<n>` (or `Related to #<n>`).
- Update body if PR already exists. Add yourself as assignee.
- Do **not** mark ready or enable auto-merge here; that is the Orchestrator's job after the AI review loop (see [PR Workflow: AI Review Loop](#pr-workflow-ai-review-loop)). Leave the PR as draft.

## CI Monitor

Dormant in unattended runs, where the `oneshot` gate covers pending checks (see [CI Checks](#ci-checks-mandatory)). Active in an [interactive session](#waiting-for-approval-in-an-interactive-session), where nothing else would pick the PR back up once CI finishes. The Orchestrator states the run mode in its hand-off and CI Monitor uses that mode rather than judging it itself, because as a sub-agent it only sees an injected prompt and would always conclude it is unattended; a hand-off that states no mode means unattended. CI Monitor does not handle bot-authored dependency-update PRs (Dependabot or another bot), because [Dependency Updater](#dependency-updater) owns their CI and merge decision.

- **P1.** Before starting, look up once the required checks configured for the base branch, as in [CI Checks](#ci-checks-mandatory), keeping their names (`contexts`, and each rule's `parameters.required_status_checks[].context`), and if it has none, drop `--required` from every check below. Set a time limit long enough for one of the repo's normal CI runs, and restart it whenever CI Debugger pushes a fix or re-runs a check, because the limit covers one CI run and would otherwise cut short the rerun that CI Debugger started. Then watch the PR's checks in the background with a scheduling/loop mechanism the tool provides, so the session stays free while CI runs (see [Background Tasks and Monitor Tool](task-workflow.instructions.md#background-tasks-and-monitor-tool-mandatory)). Pace it with long idle intervals, never tight polling. The 30-minute deadline in that section governs commands, not this wait; the time limit bounds this wait instead, because a check that never reports would otherwise keep the watch running until the session ends.
- **P2.** Each tick, check the required checks' state once with `gh pr checks <number> --repo <owner/repo> --required`; never use `--watch`. Only required checks decide the outcome, matching [CI Checks](#ci-checks-mandatory), because the PR is mergeable without the optional ones. Read the result by exit code and state column as that section describes, so a cancelled required check counts as failed, and treat either `no ... checks reported` message as pending, never as pass or failure, because the run for the head commit may not have registered yet. Treat a configured required check from P1 that has no row as pending too, never as passed, because gh exits 0 once every *reported* required check passes while GitHub keeps blocking the merge until every configured one reports, and a path-filtered or skipped required workflow may never report at all. Only CI Monitor applies this rule, not the single check in [CI Checks](#ci-checks-mandatory), because an unattended run has no human to report a never-reporting check to and `oneshot` re-invokes the agent anyway. Treat exit code 1 with no check rows and no `no ... checks reported` message as a gh or API error, as that section describes, never as a failed check. When the hand-off says the required checks skipped while the PR was a draft are now running for the first time (from [Phase E](#phase-e-mark-ready)) and the `state:` line of `gh pr view <number> --repo <owner/repo>` does not read `DRAFT` on this tick, treat a required check with no post-ready result as pending too, never as passed, reading both times as Phase E's [P4](#phase-e-enable-auto-merge) describes, because gh exits 0 for a check skipped on the draft and that result is not from the post-ready run. The ready time does not change while the PR stays ready, so once the lookup prints a time, keep it; while it prints nothing, treat every required check as having no post-ready result and look again on the next tick, as in P4, because the event may not have shown up yet. A post-ready skip counts as passed, as in P4, because some required checks skip on a ready PR by design. While the PR is a draft, for example after CI Debugger converted it back to fix a failure, use the normal rule instead, where a skipped check counts as passed, because the draft-gated checks cannot run again until Phase E marks the PR ready, so otherwise the all-pass result and the restart at Phase A in P3 could never happen.
- **P3.** Act on the result:
  - gh or API error → retry on the next tick, because a transient network, proxy or rate-limit error usually clears by then. If the next tick errors too, tell the human the error and stop the watch, because a repeated error (authentication, or a PR that does not exist) will not clear by itself.
  - Any required check fails, including a cancelled one → if the PR's comment history already holds 3 CI Debugger status comments that count (see [CI Debugger](#ci-debugger-round-count)) for that same required check, stop handing off: report the check to the Orchestrator, which treats it as [CI consistently failing](#ci-consistently-failing) and marks the PR `Blocked`, and stop, because each round restarts the time limit, so nothing else would end the cycle. Count across the whole PR since its latest auto-merge enable, not just this watch, the same way Phases B, C and D judge their round caps, because a fix that makes a check skipped on draft PRs pass ends the watch, the AI Review Loop then restarts and Phase E starts a new watch, so a per-watch count would never reach the cap. Otherwise invoke CI Debugger at once, even while other checks are still pending, because waiting for the slowest check would delay the fix by the whole CI run. Wait for CI Debugger to finish before the next tick, so the same failure is never handed off twice. Then act on what it returned:
    - It pushed a fix or re-ran a check → keep watching the new run, and restart the time limit as P1 describes.
    - It escalated → pass the escalation on to the Orchestrator and stop.
    - It did neither → tell the human which required checks failed and stop, because every later tick would show the same failure with nothing left to act on it.
    - In every case, pass any pre-existing bug report from CI Debugger on to the Orchestrator, because CI Monitor does not handle it and it would otherwise be lost.
  - All required checks pass for the first time since CI Debugger's last push or re-run, or since the watch started if the hand-off did not state that all required checks passed → wait for the next tick, because a fast required workflow can pass before a slower one has even been queued. When the hand-off states that the Orchestrator saw all required checks pass, that check was the first sighting (see [CI Checks](#ci-checks-mandatory)), so an all-pass on the first tick confirms it as below.
  - All required checks still pass on the tick after that first sighting → run `gh pr checks <number> --repo <owner/repo>` once without `--required` and mention any failed optional check to the human; an optional failure never blocks completion or triggers CI Debugger. Then stop the watch and return control to the Orchestrator, which runs the [AI Review Loop](#pr-workflow-ai-review-loop) if the PR has an [unreviewed commit](#unreviewed-commit), that is, its head differs from the one named in the loop's latest accepted `### AI Review Loop: reviewed` comment from [Phase E](#phase-e-reviewed-head), or there is no such comment. Otherwise, if the PR is ready and has no auto-merge request (the watch Phase E handed over), the Orchestrator's next step is to enable auto-merge as Phase E's [P4](#phase-e-enable-auto-merge) describes, not to re-run the loop, because this confirmed all-pass is the result P4 waits for; in any other case it does nothing more. This is because unreviewed change must not merge, and Phase E is what marks the PR ready again. When the hand-off came from inside the loop and named a phase and step to resume at, the Orchestrator resumes there instead, so the loop does not restart after every fix. The exception is when CI Debugger pushed a fix during the watch: the loop then restarts at Phase A, because no phase has reviewed that commit. Phases B, C and D judge their round caps from the PR's comment history, so a restart does not reset them. Phase A's round budget does restart, which is acceptable because exhausting it never blocks the PR.
  - Otherwise (required checks pending or in_progress, a configured one not yet reported, or none reported yet, and none failed) → wait for the next tick. A later all-pass then counts as a new first sighting.
- **P4.** If the time limit for the current run is reached, tell the human which required checks are still pending, naming any configured required check that never reported (usually a path-filtered or skipped workflow), and, when P2's post-ready rule applied on the last tick, any required check with no post-ready result, as Phase E's [P4](#phase-e-enable-auto-merge) defines it, so the human knows which workflow to look at and, in that case, that auto-merge has not been enabled, and stop the watch.
- **P5.** If the tool provides no scheduling mechanism, check once as in P2 and act as in P3, except that when required checks are pending and none has failed, or all pass on this single check without a hand-off stating that all required checks passed and so cannot be confirmed, you tell the human CI is still running and stop, when the check hits a gh or API error, you tell the human the error and stop, because there is no next tick to retry on, and when CI Debugger pushed a fix or re-ran a check, you tell the human which fix was pushed or which re-run was started and that CI is running again, and stop, leaving the human or the next session to check again, because you cannot wait for the new run.

## Dependency Updater

- Review Dependabot PRs: auto-merge safe patch/minor bumps with no advisories and passing CI.
- Flag major version bumps and breaking changes to the user. Never merge on CI failure or major bump without confirmation.
- If you take over or push any commit to a Dependabot (or other bot) PR and its changelog-check CI job then fails, see [Dependabot and Other Bot PRs](changelog.instructions.md#dependabot-and-other-bot-prs): add the missing changelog entry yourself rather than assuming the bot's `Changelog Not Required` label still applies.
