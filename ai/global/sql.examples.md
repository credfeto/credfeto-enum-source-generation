# SQL Examples

[Back to SQL Instructions](sql.instructions.md) | [Back to Global Instructions Index](index.md)

Reference examples for [sql.instructions.md](sql.instructions.md). Load when writing or debugging SQL, database connection scripts, or stored procedures.

## Local Database Connection

`$HOME/.database` (machine-specific, never committed):

```dotenv
SERVER=localhost
USER=sa
PASSWORD=<password>
```

`<repo>/.database` (committed, repo-specific):

```dotenv
DB=Treasury
```

A direct `sqlcmd` invocation may be blocked in some agent environments (e.g. to prevent
reading `.database` credential files via a raw command line). Use `<repo>/testdb` (or an
equivalent repo-local query wrapper script, e.g. `querydb`, if the repo has one) for ad-hoc
queries instead: it sources both `.database` files and invokes `sqlcmd` internally as its
own subprocess. A repo with no such wrapper script is missing one; that is a gap to raise,
not something to route around with a direct `sqlcmd` call.

## Performance Baseline

Run before and after optimising a stored procedure or view:

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
-- run the example EXEC call
SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```
