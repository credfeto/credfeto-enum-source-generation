# CodeGraph Instructions

[Back to Global Instructions Index](index.md)

This file covers using a CodeGraph index, through the `codegraph_explore` MCP tool, to understand and
locate code. It applies only to a repository that has a `.codegraph/` directory at its root. For
choosing between other tools, see [tool-preferences.instructions.md](tool-preferences.instructions.md).

## Use CodeGraph First When Indexed (MANDATORY)

- When a `.codegraph/` directory exists at the repository root, use the `codegraph_explore` MCP tool
  before `grep`, `find` or reading files to understand or locate code.
- `codegraph_explore` answers most code questions in one call. It returns the verbatim source of the
  relevant symbols plus the call paths between them, including dynamic-dispatch hops that `grep`
  cannot follow.
- Naming a file or symbol in the query returns its current, line-numbered source.
- If the tool is listed but deferred, load it by name through tool search, then call it.

## MCP Tool Only (MANDATORY)

Use only the `codegraph_explore` MCP tool. Never run the `codegraph` command line, and never offer it
as a shell fallback: the agent container denies it.

## No Index, No CodeGraph (MANDATORY)

When there is no `.codegraph/` directory at the repository root, skip CodeGraph entirely and use the
other tools as normal. In the agent container the index is built automatically; on a workstation,
whether to index a repository is the user's decision.

## Index Lag

The index may lag an edit made moments earlier by a couple of seconds. If a result looks stale,
query again or read the file directly.
