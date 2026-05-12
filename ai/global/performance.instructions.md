# Performance Instructions

> Load when: writing or optimising performance-critical code.

[Back to Global Instructions Index](index.md)

## Design and Code

- Optimise for speed first, then memory (allocations).
- Prefer algorithms and data structures with better time and space complexity.
- Avoid unnecessary allocations — reuse instances, use pooling for frequently allocated objects.
- Avoid copying data unnecessarily — work in-place or pass by reference where safe.
- Avoid blocking calls on hot paths — prefer async.
- Avoid high-level abstractions (functional pipelines, heavy reflection, dynamic dispatch) on hot paths; prefer lower-level constructs.
- Use low-allocation APIs when processing strings or buffers.
- Cache expensive computed values that do not change within a given scope.
- Be mindful of implicit conversions, boxing, or type coercions on hot paths.

## Benchmarks and Optimisation

- Write benchmarks for performance-critical code; commit them alongside the code they measure.
- In .NET repositories, write benchmarks as xUnit tests in the benchmark test project, using `FunFair.Test.Common` helpers such as `Benchmark<BenchmarkClass>()` to run the benchmark and `SummaryExtensions.AssertAllocationsAtMost(...)` to assert the measured thresholds.
- Record a baseline — regressions against it are not acceptable.
- Assert memory allocation limits using `FunFair.Test.Common` extensions (zero or explicit byte threshold); do not roll custom helpers.
- Ensure tests and benchmarks pass before optimising; commit them first as a standalone commit.
- Only commit an optimisation if it produces a measurable gain against the baseline; otherwise discard it but keep the tests and benchmarks.
