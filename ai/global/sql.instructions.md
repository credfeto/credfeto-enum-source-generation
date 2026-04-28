# SQL Instructions

[Back to Global Instructions Index](index.md)

## Linting

- When SQL is used in a project, a linter appropriate for the SQL dialect in use must be run before every commit.
- Refer to local AI instructions for the specific linter and command to use for the project's dialect.

## Local Database Connection (MS SQL Server)

Two `.database` files together provide the local SQL Server connection:

- **`$HOME/.database`** — machine-specific credentials, never committed. Contains:
  ```sh
  SERVER=localhost
  USER=sa
  PASSWORD=<password>
  ```
- **`<repo>/.database`** — committed, repo-specific. Contains:
  ```sh
  DB=Treasury
  ```

The `./testdb` script sources both automatically — do NOT pre-source them before calling `./testdb`. For ad-hoc `sqlcmd` use, source them first:

```sh
. "$HOME/.database" && . .database && sqlcmd -S "$SERVER" -U "$USER" -P "$PASSWORD" -d "$DB" ...
```

## Performance Optimization

When working on a stored procedure or view, try to reduce IO and CPU before committing:

1. **Baseline** — run the example EXEC call with statistics on:
   ```sql
   SET STATISTICS IO ON;
   SET STATISTICS TIME ON;
   -- run the example
   SET STATISTICS IO OFF;
   SET STATISTICS TIME OFF;
   ```
2. **Identify hotspots** — look for high logical read counts on large tables; a table scan where an index seek is possible is a likely win.
3. **Apply optimizations** — in rough priority order (note that this is not an exhaustive list so should be considered a minimum set):
    - Add `WITH (NOLOCK)` to every table/view reference in read-only SPs and views (no data-integrity risk for reporting queries)
    - Prefer filtering early: push `WHERE` predicates into subqueries/CTEs so SQL Server can seek before joining
    - Use covering indexes where they exist (check `sys.indexes` / actual execution plan)
    - Prefer table variables with a primary key (for small sets) or `#temp` tables with statistics (for large sets) over repeated scans of the same filter
    - Avoid scalar UDFs in `WHERE`/`JOIN` clauses — they suppress parallelism
4. **No query hints** — do not use `OPTION (...)`, `FORCE ORDER`, `USE PLAN`, or index hints unless there is genuinely no structural alternative and you have measured proof they help.
5. **Linter constraint (SRP0009)** — the build rules block `DATEPART()` / function calls on any column in a WHERE clause, even on the parameter side of a comparison. Use local `DECLARE` variables to pre-compute values outside the query rather than inline function calls on columns.
6. **Record the result** — update `docs/Benchmarks/<Schema>.md` (create if it doesn't exist) with before/after logical-read and Workfile totals whenever a change is material. Include the example parameters used.
7. **Known Bottlenecks** — if a performance issue is identified but cannot be fixed immediately (e.g. requires a schema change, index rebuild, or further investigation), document it as a "Known Bottleneck" entry in `docs/Benchmarks/<Schema>.md` **and** raise a GitHub issue describing the bottleneck, the affected procedure/view, observed stats, and the proposed fix. The issue number must be referenced in the benchmark file entry.
