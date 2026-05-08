# SQL Examples

[Back to Global Instructions Index](index.md)

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

Ad-hoc `sqlcmd` invocation (only needed outside `<repo>/testdb`):

```sh
. "$HOME/.database" && . .database && sqlcmd -S "$SERVER" -U "$USER" -P "$PASSWORD" -d "$DB" ...
```

## Performance Baseline

Run before and after optimising a stored procedure or view:

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
-- run the example EXEC call
SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```
