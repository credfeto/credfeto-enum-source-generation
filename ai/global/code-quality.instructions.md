# Code Quality Instructions

[Back to Global Instructions Index](index.md)

## Code Coverage

- 100% code coverage must be maintained.
- Test organisation in non-.NET projects is detailed in local AI instructions.

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

## Refactoring

- Review code after writing and testing to determine whether refactoring is needed.
- Refactoring must be a separate commit from feature/fix changes.
- Tests must pass after every refactoring commit.

## Compile-Time Configuration

Cover compile-time configuration (environment constants, build-time feature flags) with unit tests — not runtime assertions, which pollute production code.

## Code Complexity

- Prefer clean code — readable, well-named, single-responsibility.
- Cyclomatic complexity must stay below 20 per method; refactor if it exceeds this.
- Keep cognitive complexity low — if a method is hard to read at a glance, simplify it.
- Prefer weak (static) connascence (Name, Type, Meaning) over strong (dynamic) forms (Execution, Timing, Identity) — see [connascence.io](https://connascence.io/).
- Where stronger connascence is unavoidable, keep it local (within a single method or class).
