# Code Quality Instructions

[Back to Global Instructions Index](index.md)

## Code Coverage

- 100% code coverage must be maintained by tests.
- How tests are organised in non-.NET projects should be detailed in the local AI instructions for each repository.

## Pre-Commit

- Unit tests must be written before every commit — every new behaviour or change must have corresponding tests.
- See [git.instructions.md](git.instructions.md) for the mandatory build and test verification rules that apply before every commit.

## Dead Code

- If code cannot be reached by any code path (dead code), remove it rather than writing tests around it.

## Asynchronous Code

- Prefer asynchronous code over synchronous code wherever the language and framework support it.
- Never block on asynchronous operations synchronously — always await or use the appropriate async continuation mechanism.
- Propagate asynchrony through the call stack — do not introduce synchronous wrappers around async operations.

## Immutability

- Code should prefer immutable objects wherever possible.
- Immutability is especially important in asynchronous and multi-threaded code, where mutable shared state is a common source of bugs.
- This rule may only be broken for performance reasons, and only when explicitly requested — any such exception should be noted in a comment explaining why mutability was necessary.

## Parameterised Tests

- Prefer parameterised tests over duplicated test methods — use the equivalent of xUnit's `[Theory]` with `[InlineData]` or equivalent data-driven mechanisms wherever the language and test framework support it.
- Each variant of a behaviour should be a data point, not a separate test method.
- This applies to all languages and test frameworks; use whatever the idiomatic equivalent is (e.g. `@ParameterizedTest` in JUnit, `pytest.mark.parametrize` in pytest, `it.each` in Jest).

## Test Quality

- Tests must be held to the same code quality standards as production code — they are not exempt from readability, maintainability, or refactoring rules.
- Tests must not be brittle — they should test behaviour, not implementation details, so that refactoring production code does not unnecessarily break tests.
- Avoid hardcoding values or assumptions that are likely to change; use constants, builders, or factory helpers where appropriate.

## Refactoring

- After code is written and tested, review it to determine whether refactoring is needed.
- Refactoring must be done in a separate commit from the original code and test changes — never mix refactoring with feature or fix work.
- Tests must continue to pass after any refactoring commit.

## Compile-Time Configuration

- Compile-time configuration (e.g. environment-specific constants, feature flags resolved at build time) must be covered by a unit test rather than verified with a runtime check.
- This keeps production code clean and free of defensive assertions that only exist for testing purposes.
