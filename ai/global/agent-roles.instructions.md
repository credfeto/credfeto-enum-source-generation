# Agent Role Definitions

[Back to Global Instructions Index](index.md)

Load when acting as a named agent. Routing table and model selection: [task-workflow.instructions.md](task-workflow.instructions.md).

## Orchestrator

- Prioritise `CHANGES_REQUESTED` PRs over new issues.
- Determine work type and route via the routing table. Never implement directly.
- If a delegated role escalates a task as infeasible (Coding Researcher **Not possible** result), do not re-route it unchanged. Record the finding on the issue/PR and surface it to the user for a decision — re-scope, accept the suggested alternative, or drop.

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

- Identify opportunities to reuse existing code instead of writing new code. Focus only on newly changed code.

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

- Identify code quality issues in newly changed code.

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

- Identify inefficiencies in newly changed code.

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

- Identify logic errors in newly changed code.

#### Correctness — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag cases where the logic provably does not match the intent of the change.
2. FOCUS ON IMPACT: Prioritise errors that could cause incorrect results, data corruption, or silent failures.
3. EXCLUSIONS: Do NOT flag style or structural issues — focus solely on whether the code does what it is supposed to do.

#### Correctness — Categories

- Boundary conditions: off-by-one errors, incorrect loop bounds, fencepost errors.
- Conditionals: incorrect boolean logic, missing negation, wrong operator.
- Edge cases: null/empty input, zero values, empty collections, missing default cases.
- Business logic: code that does not match the intent described in the issue or PR.
- Rule Breaking: there should not be any files that change the linting rules or building rules that weaken the repo.

### Code Reviewer: **Security**

- Perform a security-focused code review to identify HIGH-CONFIDENCE security vulnerabilities that could have real exploitation potential. Focus only on security implications newly added by the PR.

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

- Check that newly changed files comply with all applicable rules in the `.ai-instructions` instruction files.

#### Compliance — Critical Instructions

1. MINIMISE FALSE POSITIVES: Only flag clear violations of explicit rules, not inferred or implied guidance.
2. FOCUS ON IMPACT: Prioritise violations that would cause the files to fail review or break established conventions.
3. EXCLUSIONS: Do NOT re-report issues already in scope for Reuse, Quality, Efficiency, Correctness, or Security sub-agents.

#### Compliance — Categories

- Global rules: violations of rules in `ai/global/*.instructions.md` applicable to the changed file types.
- Local rules: violations of rules in `ai/local/*.instructions.md` applicable to the changed file types — do not re-report violations already covered by global rules.
- Rule hygiene: local rules in `ai/local/*.instructions.md` that duplicate or restate rules already present in `ai/global/*.instructions.md` — flag these for removal.
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
  - If the comment required a code change: reply with `Fixed in <commit-sha>`.
  - If the comment is a question or discussion point (no code change needed): reply with a full answer inline on the PR.

## Rebase Agent

- Rebase the named branch onto `origin/main`.
- CHANGELOG conflicts: keep entries from both sides.
- Any other conflict: report verbatim to Orchestrator — do not resolve.
- Force-push with `--force-with-lease` only after all conflicts are resolved.

## CI Debugger

- Read full logs (`gh run view --log-failed`), identify root cause.
- Fix if code-related; escalate to Orchestrator with a clear description if environmental or infrastructure.
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
- Mark ready (`gh pr ready <number>`) only if Code Tester and Code Reviewer signed off — rebase first (`git fetch origin && git rebase origin/main`). Otherwise leave as draft.

## CI Monitor _(not currently enabled)_

- Watch checks after PR is ready: `gh pr checks <number> --watch`.
- All pass → done. Any fail → hand off to CI Debugger. Repeat until all pass or CI Debugger escalates.

## Dependency Updater

- Review Dependabot PRs: auto-merge safe patch/minor bumps with no advisories and passing CI.
- Flag major version bumps and breaking changes to the user. Never merge on CI failure or major bump without confirmation.
