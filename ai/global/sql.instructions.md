# SQL Instructions

> Load when: any `.sql` file or SQL project is present.

[Back to Global Instructions Index](index.md)

## Linting

Run a SQL linter appropriate for the dialect before every commit. Refer to local AI instructions for the specific linter and command.

## Local Database Connection (MS SQL Server)

Two `.database` files provide the local connection: see [sql.examples.md](sql.examples.md) for their contents and the ad-hoc `sqlcmd` invocation.

- **`$HOME/.database`**: machine-specific credentials, never committed.
- **`<repo>/.database`**: committed, repo-specific (database name).

The `<repo>/testdb` script sources both automatically; do NOT pre-source them before calling `<repo>/testdb`.

## Database Schema Management: DACPAC + Migration Scripts (MANDATORY)

When a .NET SQL Server project uses **both** a DACPAC (via `MSBuild.Sdk.SqlProj`) **and** a runtime schema-upgrade tool (e.g. `DbUp-SqlServer`, `EvolveDb`, or similar), **every schema change must appear in both places**. Neither can be omitted.

### Why both?

- The **DACPAC** (`db/` directory, consumed by the `*.Storage.Database` MSBuild project) provides a complete, authoritative DDL picture of the schema for diff-based deploy, schema comparison, and additional build-time validation.
- The **migration scripts** (`src/<project>/migrations/*.sql`) are what the application actually runs at startup via DbUp (or equivalent) to upgrade a live database incrementally.

### DACPAC directory structure (pure DDL, no `IF EXISTS`, no `GO`)

Organise by schema first, then object type. The `dbo` schema uses `dbo` as the directory name:

```text
db/
  <SchemaName>/
    Schema/                     <SchemaName>.sql              -- CREATE SCHEMA [SchemaName]
    Table/                      <TableName>.sql               -- CREATE TABLE (singular name)
    View/                       <ViewName>.sql                -- CREATE OR ALTER VIEW
    StoredProcedure/            <TableName>_<Action>.sql      -- CREATE OR ALTER PROCEDURE
    UserDefinedTableType/       <TypeName>.sql                -- CREATE TYPE AS TABLE
    Function/                   <FunctionName>.sql            -- CREATE OR ALTER FUNCTION
  dbo/                          (standard schema, no explicit Schema/ file needed)
    Table/                      <TableName>.sql
    StoredProcedure/            <TableName>_<Action>.sql
    ...
```

Examples:

- `db/DefiLlama/Schema/DefiLlama.sql`: `CREATE SCHEMA [DefiLlama]`
- `db/DefiLlama/Table/Pool.sql`: `CREATE TABLE [DefiLlama].[Pool]`
- `db/DefiLlama/StoredProcedure/Pool_GetAll.sql`: `CREATE OR ALTER PROCEDURE [DefiLlama].[Pool_GetAll]`
- `db/dbo/Table/ApiCache.sql`: `CREATE TABLE [dbo].[ApiCache]`

Rules for DACPAC files:

- **No `GO` separators**: each file contains exactly one DDL statement.
- **No `IF [NOT] EXISTS` guards**: DACPAC compares state and applies the delta; guards break the diff.
- **No `CREATE OR ALTER`** on tables, schemas, or types; only on SPs, views, and functions.
- **No `SQLCMD` directives**: they are not valid in MSBuild.Sdk.SqlProj source files.
- **Table and view names must be singular**: use `Pool`, not `Pools`; use `Author`, not `Authors`.
- **Stored procedure file naming**: `<TableName>_<Action>.sql`, e.g. `Pool_GetAll.sql`, `Pool_Sync.sql`.

### Migration script rules (DbUp-style with guards and `GO`)

- Scripts live in `src/<project>/migrations/` and are numbered sequentially: `001_...sql`, `002_...sql`.
- Each script is idempotent: wrap DDL in `IF [NOT] EXISTS` guards and separate batches with `GO`.
- Use `CREATE OR ALTER PROCEDURE/VIEW/FUNCTION` for routines (idempotent by design).
- `MERGE`-based upsert SPs are safe to use; prefer them for sync operations.

### Checklist for every schema change

When adding or modifying a schema object, update **both**:

| Change | DACPAC file to create/update | Migration script action |
| ------ | ----------------------------- | ----------------------- |
| New schema | `db/<Schema>/Schema/<Schema>.sql` | `IF NOT EXISTS ... CREATE SCHEMA` in new migration |
| New table | `db/<Schema>/Table/<Table>.sql` | `IF NOT EXISTS ... CREATE TABLE` in new migration |
| New TVP | `db/<Schema>/UserDefinedTableType/<Type>.sql` | `IF NOT EXISTS ... CREATE TYPE AS TABLE` in new migration |
| New/changed SP | `db/<Schema>/StoredProcedure/<Table>_<Action>.sql` | `CREATE OR ALTER PROCEDURE` in new migration |
| New/changed view | `db/<Schema>/View/<View>.sql` | `CREATE OR ALTER VIEW` in new migration |
| Column added | Update `db/<Schema>/Table/<Table>.sql` | `IF NOT EXISTS ... ALTER TABLE ADD COLUMN` in new migration |
| Column type change | Update DACPAC table file | `ALTER TABLE ALTER COLUMN` in new migration |
| Index added | `db/<Schema>/Index/<Table>_<IndexName>.sql` (if applicable) | `IF NOT EXISTS ... CREATE INDEX` in new migration |

### Validation

After adding any `db/` files, build the DACPAC project to verify there are no SQL compilation errors:

```bash
dotnet build src/<AssemblyName>.Storage.Database/<AssemblyName>.Storage.Database.csproj -c Release
```

A clean build confirms the DACPAC SQL is syntactically valid and all object references resolve.

### `.sqlfluffignore` (MANDATORY)

The Release build of `MSBuild.Sdk.SqlProj` emits `<AssemblyName>_Create.sql` and related files into `bin/` and `obj/`. These contain SQLCMD directives (`:on error exit`) that sqlfluff cannot parse. Always exclude generated output:

```text
src/*/bin/
src/*/obj/
```

Add this to `.sqlfluffignore` at the repo root if it does not exist.

## Performance Optimisation

Before committing a stored procedure or view, reduce IO and CPU:

1. **Baseline**: run the example EXEC with `SET STATISTICS IO/TIME ON`; see [sql.examples.md](sql.examples.md).
2. **Identify hotspots**: high logical reads on large tables; prefer index seeks over table scans.
3. **Optimise** (minimum set):
   - Add `WITH (NOLOCK)` to every table/view reference in read-only SPs and views.
   - Push `WHERE` predicates into subqueries/CTEs to seek before joining.
   - Use covering indexes where available.
   - Prefer table variables with a primary key (small sets) or `#temp` tables with statistics (large sets) over repeated scans.
   - Avoid scalar UDFs in `WHERE`/`JOIN` clauses; they suppress parallelism.
4. **No query hints**: avoid `OPTION (...)`, `FORCE ORDER`, `USE PLAN`, or index hints unless measured proof justifies them.
5. **Linter constraint (SRP0009)**: build rules block `DATEPART()` / function calls on columns in `WHERE` clauses; use `DECLARE` variables to pre-compute values outside the query.
6. **Record**: update `docs/Benchmarks/<Schema>.md` with before/after logical-read and Workfile totals for material changes, including example parameters.
7. **Known Bottlenecks**: if a performance issue cannot be fixed immediately, document it as a "Known Bottleneck" in `docs/Benchmarks/<Schema>.md` and raise a GitHub issue with the bottleneck details, affected procedure/view, observed stats, and proposed fix. Reference the issue number in the benchmark file.
