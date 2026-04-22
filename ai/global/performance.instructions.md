# Performance Instructions

[Back to Global Instructions Index](index.md)

## General Principles

- Performance must be considered as part of any design decision, not treated as an afterthought.
- Priorities in order:
  1. **Speed** — minimise latency and execution time.
  2. **Memory** — reduce allocations and memory usage wherever possible.

## Design

- Prefer algorithms and data structures with better time and space complexity when there is a realistic choice.
- Avoid unnecessary object allocations — reuse instances and use pooling for frequently allocated objects where the language and runtime support it.
- Avoid copying data unnecessarily — work in-place or pass by reference where safe to do so.
- Avoid blocking calls on hot paths — prefer asynchronous operations to keep threads free.

## Code

- Avoid high-level abstractions (e.g. functional pipelines, heavy reflection, dynamic dispatch) in performance-critical paths where they introduce unnecessary overhead; prefer lower-level constructs instead.
- Use low-allocation APIs and patterns when processing strings or buffers, where the language or runtime provides them.
- Cache computed values that are expensive to recalculate and do not change within a given scope.
- Be mindful of implicit conversions, wrapping, or type coercions that incur overhead on hot paths.

## Benchmarks

- For performance-critical code, write benchmarks as tests.
- Record a baseline result that new changes must match or improve upon — a regression against the baseline is not acceptable.
- Benchmarks should be committed alongside the code they measure.

## Optimisation Workflow

- Before starting any optimisation work, ensure both tests and benchmarks exist and pass — do not begin optimising without them.
- Commit and push the tests and benchmarks first, as a standalone commit, even before any optimisation is attempted — this is a worthwhile exercise in its own right.
- Only commit the optimisation itself if it produces a measurable gain against the recorded baseline.
- If no gain is achieved, discard the optimisation change but keep the tests and benchmarks.
