# Code Quality Instructions

[Back to Global Instructions Index](index.md)

## Code Coverage

- 100% code coverage must be maintained.
- Test organisation in non-.NET projects is detailed in local AI instructions.

### Infrastructure-Dependent Success Paths

Some methods open real network connections, file handles, or database sessions, and their success-path (the line that returns a live resource) is only reachable when actual infrastructure is available. In a unit-test environment that path is unreachable.

- Do **not** add `[ExcludeFromCodeCoverage]`, `[SuppressMessage]`, or any `coverage.settings.xml` `<Functions>` exclusion for these gaps.
- Accept the coverage gap and note it; do not block work on it.
- Prefer mocking the success path instead: if the underlying type or interface can be substituted, write a test that exercises it.
- If the path is genuinely unreachable in unit tests **and** is not covered by an integration-test project, raise a GitHub issue labelled `AI-Work`, `Low`, and `Blocked` to track getting it covered by integration tests.

## Pre-Commit

- Write unit tests before every commit; every new behaviour must have corresponding tests.
- See [git.instructions.md](git.instructions.md) for mandatory build and test verification before committing.

### Fixing Pre-Commit Failures (MANDATORY)

Pre-commit and its component tools (e.g. `dotnet buildcheck`, analyzers, linters) are improved incrementally precisely by encountering and fixing the problems they surface. If pre-commit reports an error that was not present before the current work started — whether caused by your own edits or a component tool catching something pre-existing — fixing it is part of the current work, not a reason to stop.

- Do not stop or escalate merely because the failure is unexpected, was not present originally, or requires changes outside the files you set out to edit, including the pre-commit configuration or a component tool's own rules/config.
- Only stop and ask if the issue is genuinely fatal: pre-commit cannot possibly be made to pass (e.g. a required external tool is missing from the environment and cannot be installed, or the cause is infrastructure outside the repo's control).
- This does not relax [Build and Test Verification](git.instructions.md#build-and-test-verification-mandatory-before-any-commit-or-push): the fix must be a genuine fix, not a suppression, skip, or exclusion, unless separately authorised.

## Dead Code

- Remove unreachable code rather than writing tests around it.
- Dead/unreachable code removal: separate commit from test changes, after running tests on the entire handler or app; one method or function per commit.
- Shared code removal: only after the entire codebase has 100% coverage; each removal is its own commit.

## Asynchronous Code

- Prefer async over sync wherever supported.
- Never block on async operations, always await or use async continuations.
- Propagate async through the call stack; no synchronous wrappers around async operations.

## Immutability

Prefer immutable objects wherever possible, especially in async and multi-threaded code. Only break this for performance reasons when explicitly requested; note the reason in a comment.

## Parameterised Tests

Prefer parameterised tests over duplicated test methods: each behavioural variant is a data point, not a separate method. Use the idiomatic mechanism for the framework (xUnit `[Theory]`/`[InlineData]`, JUnit `@ParameterizedTest`, pytest `parametrize`, Jest `it.each`).

## Test Quality

- Tests must meet the same code quality standards as production code.
- Test behaviour, not implementation: refactoring production code must not unnecessarily break tests.
- Use constants, builders, or factory helpers rather than hardcoded values likely to change.

## Mock Setup Helpers

When a mock setup expression (NSubstitute, Moq, or equivalent) is used in more than one test, extract it into a dedicated `private static` method named `Mock<InterfaceName><MethodName>`, for example, `MockBranchClassificationIsPullRequest`. The helper accepts the mock instance and any variable arguments, and returns the configured mock (or `void` if chaining is not needed). Do not inline the same setup expression across multiple tests.

## Refactoring

- Review code after writing and testing to determine whether refactoring is needed.
- Refactoring must be a separate commit from feature/fix changes.
- Tests must pass after every refactoring commit.

## Incidental File Cleanup

- If a file you are already working on has issues unrelated to your current change (e.g. unused imports/usings, unreachable branches, inconsistent formatting, stale comments, duplicated code, code-analysis warnings, or suppressions of code-analysis warnings), clean them up so the file is the best it can be, while keeping to existing project standards, not inventing new ones.
- Duplication is not limited to the file itself: if the file duplicates code found elsewhere in the repo, eliminate the duplication (e.g. extract to a shared location) as part of this cleanup.
- Resolve code-analysis warnings in the file, including pre-existing ones unrelated to your change. Prefer removing an existing suppression by refactoring the underlying code over leaving the suppression in place. Do not add a new suppression as a way to close this out — adding one is prohibited without explicit written permission (see [Warning Suppression and Errors](dotnet.instructions.md#warning-suppression-and-errors) for the .NET-specific mechanics; the same fix-the-root-cause-don't-suppress principle applies in every language).
- Commit this cleanup separately from the feature/fix change.
- If there are multiple distinct fix types in the file (e.g. unused imports and stale comments), fix and commit them one type at a time: each fix type is its own commit, per file.
- Tests must pass after every cleanup commit.

## Pattern Sweep (MANDATORY)

After fixing a bug, or accepting a finding from `/simplify`, `/code-review`, `/security-review`, or a human PR review comment, search the entire repository for other occurrences of the same construct before moving on. A fix applied to one site while the same construct survives elsewhere is an incomplete fix.

- Search for the construct, not the symptom: the same API misuse, boundary condition, missing guard, duplicated helper, or insecure call. Use whatever search fits the construct (identifier, call shape, regular expression).
- Before fixing a round's findings, group them by construct; each group gets one fix commit and one sweep. One sweep, and one sweep commit, per construct, not per finding or per occurrence: when several findings report the same construct at different sites, one sweep covers them all. Skip the sweep if a commit already in this PR carries a `Construct:` line for the same construct and no later commit reintroduced it; a site a later review round deliberately reverted is not reintroduced.
- Sweep in the working tree before handing off for build/test verification, so one run covers both the fix and the sweep; then commit fix first, sweep second, in the same PR. Anything a fix-touched file depends on is part of the fix, not the sweep, and the committing role builds once between the two commits so the fix commit stands on its own. Exception: Phase A of the [PR review loop](agent-roles.instructions.md#pr-workflow-ai-review-loop) sweeps once after `/simplify` converges rather than after each round.
- The sweep record handed to a committing role is: a `Construct:` line naming the construct searched for, the finding or comment reference, and one line per file the sweep touched with why that site matches, each marked sweep-only or fix-touched (or `Swept: none` when nothing was found). The producing role appends it to its hand-off report, and every intermediate role (Code Tester, Code Reviewer, Changelog) carries it in its own report unchanged. Staging is by whole file: sweep-only files form the sweep commit; sweep hunks and covering tests in files the fix touches go into the fix commit and are listed in its body with the same rationale. When the origin commits are already pushed (a Phase A sweep), every hunk goes into the sweep commit.
- A sweep commit changes only the matching sites and the tests that cover them. Files touched only by the sweep are exempt from [Incidental File Cleanup](#incidental-file-cleanup) (a file the fix also touches follows it as normal); raise a GitHub issue for anything else noticed there, as for pre-existing [deprecation warnings](#deprecation-warnings-during-tests).
- A sweep that would touch more than 25 files is not applied silently: post the site list on the PR, add `Blocked`, and wait for a human decision on whether it belongs in this PR or a follow-up issue. This gate applies in every phase, including Phase A, and is the one reason Phase A may add `Blocked`.
- Commit body: see [Pattern Sweep Commits](git-commits.instructions.md#pattern-sweep-commits). Review-comment reply: see [Comment Replies](agent-roles.instructions.md#comment-replies-mandatory). When every hit is in a file the fix touches, there is no sweep commit: the fix commit body carries the `Construct:` line and per-file rationale, and the reply cites the fix SHA.
- If the sweep finds nothing, the fix commit body carries the `Construct:` line and `Swept: none`; no sweep commit or extra comment is needed. A Phase A no-hit sweep has no fix commit of its own, so it is recorded in Phase A's status comment on the PR instead.
- A sweep hit that the build-time static analyser stack already enforces differently is left as-is, on the same principle as [Conflict Resolution](agent-roles.instructions.md#conflict-resolution-simplifycode-review-vs-static-analyzer).

## Compile-Time Configuration

Cover compile-time configuration (environment constants, build-time feature flags) with unit tests, not runtime assertions, which pollute production code.

## Deprecation Warnings During Tests

When deprecation warnings appear in test output (e.g. framework or runtime warnings about deprecated APIs):

- **If the warning is new and caused by your change**: fix the deprecation before committing; do not leave it for later.
- **If the warning is pre-existing and not caused by your change**: first check for an existing open GitHub issue in the current repository covering the same warning (for example by searching for the deprecated API, the warning text, and the affected component or dependency).
  - If a matching open issue already exists, update it with any new context you found or reference it in your work; do not create a duplicate issue.
  - If no matching open issue exists, raise a new GitHub issue in the current repository with:
    - A clear title describing the deprecated API.
    - The full warning text.
    - The component or dependency responsible.
    - What needs to be done to resolve it.
    - Label the issue `AI-Work`.

Do not suppress or ignore deprecation warnings.

## Code Comments (MANDATORY)

- **Never write XMLDoc (`///`) or Javadoc (`/** */`) comments.** Code must speak for itself through well-chosen names and clear structure.
- If you feel a doc comment is needed to explain what something does, that is a signal that the code is too complex or the names are wrong: fix the code, not the documentation.
- The only acceptable inline comments explain a non-obvious **why**: a hidden constraint, a subtle invariant, a deliberate workaround for a known bug. If removing the comment would not confuse a future reader, do not write it.
- Do not write comments that describe what the code does; well-named identifiers already do that.
- Do not reference the current task, issue, PR, or caller in comments; those belong in the commit message or PR description and rot as the codebase evolves.

## Code Complexity

- Prefer clean code: readable, well-named, single-responsibility.
- Cyclomatic complexity must stay below 20 per method; refactor if it exceeds this.
- Keep cognitive complexity low; if a method is hard to read at a glance, simplify it.
- Prefer weak (static) connascence (Name, Type, Meaning) over strong (dynamic) forms (Execution, Timing, Identity); see [connascence.io](https://connascence.io/).
- Where stronger connascence is unavoidable, keep it local (within a single method or class).
