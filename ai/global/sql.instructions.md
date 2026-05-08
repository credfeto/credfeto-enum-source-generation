# SQL Instructions

> Load when: any `.sql` file or SQL project is present.

[Back to Global Instructions Index](index.md)

## Linting

Run a SQL linter appropriate for the dialect before every commit. Refer to local AI instructions for the specific linter and command.

## Local Database Connection (MS SQL Server)

Two `.database` files provide the local connection — see [sql.examples.md](sql.examples.md) for their contents and the ad-hoc `sqlcmd` invocation.

- **`$HOME/.database`** — machine-specific credentials, never committed.
- **`<repo>/.database`** — committed, repo-specific (database name).

The `<repo>/testdb` script sources both automatically — do NOT pre-source them before calling `<repo>/testdb`.

## Performance Optimisation

Before committing a stored procedure or view, reduce IO and CPU:

1. **Baseline** — run the example EXEC with `SET STATISTICS IO/TIME ON` — see [sql.examples.md](sql.examples.md).
2. **Identify hotspots** — high logical reads on large tables; prefer index seeks over table scans.
3. **Optimise** (minimum set):
   - Add `WITH (NOLOCK)` to every table/view reference in read-only SPs and views.
   - Push `WHERE` predicates into subqueries/CTEs to seek before joining.
   - Use covering indexes where available.
   - Prefer table variables with a primary key (small sets) or `#temp` tables with statistics (large sets) over repeated scans.
   - Avoid scalar UDFs in `WHERE`/`JOIN` clauses — they suppress parallelism.
4. **No query hints** — avoid `OPTION (...)`, `FORCE ORDER`, `USE PLAN`, or index hints unless measured proof justifies them.
5. **Linter constraint (SRP0009)** — build rules block `DATEPART()` / function calls on columns in `WHERE` clauses; use `DECLARE` variables to pre-compute values outside the query.
6. **Record** — update `docs/Benchmarks/<Schema>.md` with before/after logical-read and Workfile totals for material changes, including example parameters.
7. **Known Bottlenecks** — if a performance issue cannot be fixed immediately, document it as a "Known Bottleneck" in `docs/Benchmarks/<Schema>.md` and raise a GitHub issue with the bottleneck details, affected procedure/view, observed stats, and proposed fix. Reference the issue number in the benchmark file.
