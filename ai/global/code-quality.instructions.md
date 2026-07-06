# Code Quality Instructions

[Back to Global Instructions Index](index.md)

## Code Coverage

- 100% code coverage must be maintained.
- Test organisation in non-.NET projects is detailed in local AI instructions.

### Infrastructure-Dependent Success Paths

Some methods open real network connections, file handles, or database sessions, and their success-path (the line that returns a live resource) is only reachable when actual infrastructure is available. In a unit-test environment that path is unreachable.

- Do **not** add `[ExcludeFromCodeCoverage]`, `[SuppressMessage]`, or any `coverage.settings.xml` `<Functions>` exclusion for these gaps.
- Accept the coverage gap and note it; do not block work on it.
- Prefer mocking the success path instead — if the underlying type or interface can be substituted, write a test that exercises it.
- If the path is genuinely unreachable in unit tests **and** is not covered by an integration-test project, raise a GitHub issue labelled `AI-Work`, `Low`, and `Blocked` to track getting it covered by integration tests.

## Pre-Commit

- Write unit tests before every commit — every new behaviour must have corresponding tests.
- See [git.instructions.md](git.instructions.md) for mandatory build and test verification before committing.

## Dead Code

- Remove unreachable code rather than writing tests around it.
- Dead/unreachable code removal: separate commit from test changes, after running tests on the entire handler or app; one method or function per commit.
- Shared code removal: only after the entire codebase has 100% coverage; each removal is its own commit.

## Asynchronous Code

- Prefer async over sync wherever supported.
- Never block on async operations — always await or use async continuations.
- Propagate async through the call stack — no synchronous wrappers around async operations.

## Immutability

Prefer immutable objects wherever possible — especially in async and multi-threaded code. Only break this for performance reasons when explicitly requested; note the reason in a comment.

## Parameterised Tests

Prefer parameterised tests over duplicated test methods — each behavioural variant is a data point, not a separate method. Use the idiomatic mechanism for the framework (xUnit `[Theory]`/`[InlineData]`, JUnit `@ParameterizedTest`, pytest `parametrize`, Jest `it.each`).

## Test Quality

- Tests must meet the same code quality standards as production code.
- Test behaviour, not implementation — refactoring production code must not unnecessarily break tests.
- Use constants, builders, or factory helpers rather than hardcoded values likely to change.

## Mock Setup Helpers

When a mock setup expression (NSubstitute, Moq, or equivalent) is used in more than one test, extract it into a dedicated `private static` method named `Mock<InterfaceName><MethodName>` — for example, `MockBranchClassificationIsPullRequest`. The helper accepts the mock instance and any variable arguments, and returns the configured mock (or `void` if chaining is not needed). Do not inline the same setup expression across multiple tests.

## Refactoring

- Review code after writing and testing to determine whether refactoring is needed.
- Refactoring must be a separate commit from feature/fix changes.
- Tests must pass after every refactoring commit.

## Incidental File Cleanup

- If a file you are already working on has issues unrelated to your current change (e.g. unused imports/usings, unreachable branches, inconsistent formatting, stale comments), clean them up so the file is the best it can be — while keeping to existing project standards, not inventing new ones.
- Commit this cleanup separately from the feature/fix change.
- If there are multiple distinct fix types in the file (e.g. unused imports and stale comments), fix and commit them one type at a time — each fix type is its own commit, per file.
- Tests must pass after every cleanup commit.

## Compile-Time Configuration

Cover compile-time configuration (environment constants, build-time feature flags) with unit tests — not runtime assertions, which pollute production code.

## Deprecation Warnings During Tests

When deprecation warnings appear in test output (e.g. framework or runtime warnings about deprecated APIs):

- **If the warning is new and caused by your change** — fix the deprecation before committing; do not leave it for later.
- **If the warning is pre-existing and not caused by your change** — first check for an existing open GitHub issue in the current repository covering the same warning (for example by searching for the deprecated API, the warning text, and the affected component or dependency).
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
- If you feel a doc comment is needed to explain what something does, that is a signal that the code is too complex or the names are wrong — fix the code, not the documentation.
- The only acceptable inline comments explain a non-obvious **why**: a hidden constraint, a subtle invariant, a deliberate workaround for a known bug. If removing the comment would not confuse a future reader, do not write it.
- Do not write comments that describe what the code does — well-named identifiers already do that.
- Do not reference the current task, issue, PR, or caller in comments — those belong in the commit message or PR description and rot as the codebase evolves.

## Code Complexity

- Prefer clean code — readable, well-named, single-responsibility.
- Cyclomatic complexity must stay below 20 per method; refactor if it exceeds this.
- Keep cognitive complexity low — if a method is hard to read at a glance, simplify it.
- Prefer weak (static) connascence (Name, Type, Meaning) over strong (dynamic) forms (Execution, Timing, Identity) — see [connascence.io](https://connascence.io/).
- Where stronger connascence is unavoidable, keep it local (within a single method or class).
