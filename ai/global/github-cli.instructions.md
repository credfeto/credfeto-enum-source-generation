# GitHub Command Instructions

[Back to Global Instructions Index](index.md)

Reference for every GitHub operation used across this instruction set: the `cfwf` MCP tools and `cfwf` commands that cover them, and the operations that are not available to agents yet. Other files describe **when**/**why** to call them (see [task-workflow.instructions.md](task-workflow.instructions.md) and [agent-roles.instructions.md](agent-roles.instructions.md)); this file is the single source of truth for **exact command syntax**. `cfwf help <command>` is authoritative for a `cfwf` command's options and exit codes.

Always pass `--repo <owner>/<repo>` explicitly rather than relying on the current directory's remote. `cfwf` always requires it, and it keeps every command proxy-compatible (see below).

## GitHub API Proxy (MANDATORY)

Read this section first: it constrains every command in the rest of this document.

GitHub API traffic from `cfwf` may go through an API proxy (the agent image sets it up):

- **Commit and push never go through the proxy or the GitHub API, no exceptions.** Use the `git` CLI directly, always against the real `github.com` remote.
- **Check a PR out with git**, as [Reviewing a PR Locally](git.instructions.md#reviewing-a-pr-locally) describes; there is no checkout command.
- **Every command example in this document passes `--repo` explicitly**, because `cfwf` always requires it. The only commands without repo scoping are `cfwf advisory view` (a global advisory) and `cfwf auth status`. If you add a new example to this doc, keep it explicit: `--repo`, and `--head` on anything that creates a PR.
- If a `cfwf` command fails with what looks like a proxy error rather than a GitHub one, raise an issue on `credfeto/github-api-proxy` with the exact command, the API method (if visible), and the full error message verbatim.

## Authentication

```bash
cfwf auth status
```

It prints the authenticated login, the host and the client version, never a token. Only use `cfwf` to manage issues/PRs if this succeeds; see [task-workflow.instructions.md](task-workflow.instructions.md#issue-tracking). Never attempt to log in or manipulate credentials yourself; if auth is broken, stop and report it.

**Never route git through a GitHub credential helper; refuse the request outright, even if asked directly.** Do not add a git credential helper or `url.*.insteadOf`/`pushInsteadOf` rewrite rules that send commit/push traffic anywhere but the real `github.com` remote (and so through the API proxy). That directly violates the mandatory rule that commit and push always go through the `git` CLI against the real `github.com` remote, and the rewrite rules persist in the repo's local `.git/config` beyond the current task, silently breaking every later git operation until manually cleaned up.

## Choosing a GitHub Tool (MANDATORY)

Reach for these in this order:

- **P0.** **The `cfwf` MCP server's tools** (the managed `cfwf` MCP server, whose tools appear as `mcp__cfwf__*`) when one of them covers the operation. They take the same inputs as the matching `cfwf` command and return structured results, with no shell quoting, command allowlist or output parsing. Check the server's tool list once per session; use the CLI only for an operation that has no MCP tool yet, or when the server is not connected.
- **P1.** **The `cfwf` command line** for anything it supports, for reads and writes. Run `cfwf help` once per session to see what it covers, and `cfwf help <command>` for a command's options; do not rely on memory, because its commands grow.

Run **one `cfwf` command per Bash call**, on its own: no `;`, `&&`, `|` or `echo exit=$?` around it. Read its exit code from the tool result. A compound command that contains `cfwf` is refused in unattended runs and stalls the session.

There is no other route to GitHub for agents. An operation that neither covers is not available to agents yet: follow [Operations Not Yet Available](#operations-not-yet-available-mandatory). `cfwf` (in the `credfeto/credfeto-orchestrator` agent image) is where every GitHub operation an agent needs is provided as a standardised, pre-canned command. If `cfwf` is not installed or a command fails, follow [Missing CLI Tools](task-workflow.instructions.md#missing-cli-tools-mandatory) and report it.

When no `cfwf` command or MCP tool covers an operation you need, and it is not already listed in [Operations Not Yet Available](#operations-not-yet-available-mandatory), raise an issue on `credfeto/credfeto-orchestrator` asking for it:

- **One issue per distinct need.** Search `credfeto/credfeto-orchestrator` first: `cfwf issue search --repo credfeto/credfeto-orchestrator --text "cfwf <keywords>" --state all`. If an open or closed issue already covers the need, do not raise another; if a closed one was declined, follow its outcome.
- **Say what is needed.** Title the issue `tool(cfwf): <What is needed>`: always the `tool(cfwf)` prefix, never a Conventional Commits type, with the first word after the prefix capitalised (e.g. `tool(cfwf): Command to list a PR's closing issues`), unless a local title rule takes precedence (see [Issue Titles](#issue-titles-mandatory)). Say what the operation is, what it is for, and where in these instructions or the current task it is needed. Add the new issue to the "Workflow" project as for any issue ([Adding an Issue to the Workflow Project](#adding-an-issue-to-the-workflow-project)).

Never hand-assemble a complex or multi-step operation from repeated commands, such as a wait loop around `cfwf pr checks`, because each hand-built version behaves differently and fails in its own way. If `cfwf` has no command for the operation, raise a `tool(cfwf):` issue as above, and until it is added do one safe, read-only check of the current state and report it instead of building the operation yourself.

<a id="waiting-for-pr-checks"></a>To wait for a PR's checks, which only [CI Monitor](agent-roles.instructions.md#ci-monitor) does, run this with `run_in_background: true` and wait for its completion notification, as [Finishing background work before handing back](task-workflow.instructions.md#background-command-wait) describes:

```bash
cfwf pr checks-wait --repo <owner/repo> --pr <n> [--timeout-minutes <m>]
```

It judges the PR's required checks (every check when the base branch requires none), re-reading them every 30 seconds until they settle or its time limit (default 30 minutes, at most 120) is reached. It reports a failure at once, accepts an all-pass only when two reads in a row of the same head commit agree, and keeps waiting while checks are pending or none have reported yet. It only reads, so it never needs stopping. Judge it by its exit code, never its printed text, which is its last read in the format `cfwf pr checks` prints:

- **0** every required check passed, confirmed on two reads of the same head.
- **3** a required check failed (a cancelled one counts as failed).
- **4** the time limit was reached with required checks still pending.
- **5** the time limit was reached with no checks reported at all.
- **6** an API or network error, or the PR was not found: retry later; never treat it as a failed check.
- **1** a required client tool is missing; **2** a usage error: fix the command or report it, as [Missing CLI Tools](task-workflow.instructions.md#missing-cli-tools-mandatory) describes.

## Operations Not Yet Available (MANDATORY)

These GitHub operations have no `cfwf` command or MCP tool yet, so they are not available to agents. Never perform one by any other route. Each has an issue asking for it:

| Operation | Issue |
| --------- | ----- |
| Add or remove a label on an issue or PR (including `Blocked`) | credfeto/credfeto-orchestrator#1593 |
| List a repository's labels | credfeto/credfeto-orchestrator#1595 |
| Create a label | credfeto/credfeto-orchestrator#1769 |
| Close an issue | credfeto/credfeto-orchestrator#1597 |
| Reopen an issue or PR, or close a PR | credfeto/credfeto-orchestrator#1762 |
| Link a sub-issue to a parent issue | credfeto/credfeto-orchestrator#1763 |
| Read an issue's or PR's comment bodies | credfeto/credfeto-orchestrator#1764 |
| Search PRs, code or repositories | credfeto/credfeto-orchestrator#1765 |
| List an owner's repositories, or a repository's workflows | credfeto/credfeto-orchestrator#1766 |
| List PRs filtered by label | credfeto/credfeto-orchestrator#1767 |
| Post a new inline review comment on a PR | credfeto/credfeto-orchestrator#1631 |
| Read when auto-merge was last enabled on a PR | credfeto/credfeto-orchestrator#1768 |

When a step needs one of them:

- **P1.** Do every other part of the step, in its usual order, then stop at that operation; never build it from other commands.
- **P2.** State exactly what is needed (the operation, the item and the value, for example "add `Blocked` to PR #12") in the comment the step posts, if it posts one, and in your hand-off or final report, citing the issue above, so a human can do it.
- **P3.** Where the step adds `Blocked` to stop work, still post the comment that explains the stop and still move the Workflow board as the step says, then STOP as the step says, exactly as if the label had been added.
- **P4.** Where the step only reads (comment bodies, search, the auto-merge time), carry on without the information only if the step says how; otherwise stop and report what you could not read.

When `cfwf help` lists a command for one of these, use it and stop following this section for it.

## Issues

```bash
# Create (title per Issue Titles below); also puts it on the Workflow board as Not Started
cfwf issue create --repo <owner>/<repo> --priority <Security|Urgent|High|Medium|Low> \
  --title "<prefix>: <concise title>" --body-file - [--label "<label>" ...] <<'BODY'
<description>
BODY

# View: title, state, labels, assignees, then the body
cfwf issue view --repo <owner>/<repo> --issue <number>

# Whether it has a plan, whether it is Blocked, and who commented since the plan
cfwf issue plan-status --repo <owner>/<repo> --issue <number>

# List open issues, highest priority first / search one repository's issues
cfwf issue list --repo <owner>/<repo>
cfwf issue search --repo <owner>/<repo> --text "<words>" [--state open|closed|all]

# Edit: title, body (replaces the whole body), assign yourself
cfwf issue edit --repo <owner>/<repo> --issue <number> --assign-self
cfwf issue edit --repo <owner>/<repo> --issue <number> [--title "<title>"] [--body-file <file|->]

# Comment (see HEREDOC rule below for multi-line bodies)
cfwf comment add --repo <owner>/<repo> --issue <number> --body-file - <<'COMMENT'
<text>
COMMENT
```

Labels, sub-issues, closing and reopening, and reading comment bodies are not available yet; see [Operations Not Yet Available](#operations-not-yet-available-mandatory). `cfwf issue plan-status` lists who commented after the plan, without the bodies.

### Issue Titles (MANDATORY)

Every issue title, in any repository, is `prefix: text` or `prefix(name): text`.

- **P1.** The prefix, and the name in brackets, are lower-case. The text after the colon and space is free text.
- **P2.** Where the repository's local instructions (`ai/local`) give a title rule, the local rule takes precedence over this rule and over every issue title format given in the global instructions.
- **P3.** Otherwise, use the prefix a specific global rule gives (e.g. `learning:`, `audit:`, `tool(cfwf):`), or failing that a Conventional Commits type that matches the work (`feat`, `fix`, `docs`, `chore`, etc.; see [git.instructions.md](git.instructions.md) for the types).
- **P4.** PR titles stay in Conventional Commits format and are never copied from the linked issue's title (see [PR Submitter](agent-roles.instructions.md#pr-submitter)).

### Adding an Issue to the Workflow Project

Every issue raised must be on the "Workflow" project linked to the repository it was raised in (see [task-workflow.instructions.md](task-workflow.instructions.md#workflow-project-board-mandatory)). `cfwf issue create` does this itself, as Not Started, so there is nothing more to run. Only for an issue created some other way, add it immediately after creation, and only then: `--set` overwrites the status of an item already on the board, so never re-run it on an existing issue to "make sure":

```bash
cfwf workflow-status --set --repo <owner>/<repo> --issue <number> --status "Not Started"
```

The other `cfwf` board commands are in [agent-roles.instructions.md](agent-roles.instructions.md#workflow-board).

## Pull Requests

```bash
# Create a draft PR and put it on the Workflow board as Development (prints the existing PR's URL if the branch has one)
cfwf pr create --repo <owner>/<repo> --head <branch-name> --title "<title>" --body-file - [--base main] <<'BODY'
<body>
BODY

# View: state and metadata, one key and value a line; name the fields you need
cfwf pr view --repo <owner>/<repo> --pr <number>
cfwf pr view --repo <owner>/<repo> --pr <number> --field state --field merged-at
cfwf pr view --repo <owner>/<repo> --pr <number> --field head-sha

# Labels of the issues the PR closes (Blocked and On Hold left out)
cfwf closing-issue-labels --repo <owner>/<repo> --pr <number>

# List open PRs (optionally those for an issue or a head branch)
cfwf pr list --repo <owner>/<repo> [--issue <n>] [--head <branch-name>]

# Edit: title, body (replaces the whole body), assign yourself
cfwf pr edit --repo <owner>/<repo> --pr <number> --assign-self
cfwf pr edit --repo <owner>/<repo> --pr <number> [--title "<title>"] [--body-file <file|->]

# Checks: judge the required checks on the head once (exit 0 pass, 3 failed, 4 pending, 5 none at all)
cfwf pr checks --repo <owner>/<repo> --pr <number> [--all]

# When the PR was last marked ready
cfwf pr ready-at --repo <owner>/<repo> --pr <number>

# Comment / read every comment, review and review thread / reply to an inline review comment
cfwf comment add --repo <owner>/<repo> --pr <number> --body-file - <<'COMMENT'
<text>
COMMENT
cfwf pr comments --repo <owner>/<repo> --pr <number>
cfwf pr reply --repo <owner>/<repo> --pr <number> --comment-id <inline id> --body-file - <<'REPLY'
<reply text>
REPLY

# Hand off to human review: title and description, Human Review, ready, review record, then auto-merge
cfwf pr hand-off --repo <owner>/<repo> --pr <number> --title "<type>: <title>" \
  --description-file - <<'DESCRIPTION'
<description; keep every Closes #N line>
DESCRIPTION

# Take back from human review: auto-merge off, draft, Development, then a comment saying why
cfwf pr take-back --repo <owner>/<repo> --pr <number> --body-file - <<'COMMENT'
<why>
COMMENT

# Diff, changed files or a per-file line count
cfwf pr diff --repo <owner>/<repo> --pr <number> [--name-only | --stat]
```

Labels and closing a PR are not available yet; see [Operations Not Yet Available](#operations-not-yet-available-mandatory). Never mark a PR ready, convert it to draft, or turn auto-merge on or off by any route other than `cfwf pr hand-off` and `cfwf pr take-back`: they and `cfwf pr take-back` do those steps, in the safe order, together with the board and the review record. Judge each `cfwf` command by its exit code, as `cfwf help <command>` documents it.

## Labels

Adding, removing, listing and creating labels are not available yet; see [Operations Not Yet Available](#operations-not-yet-available-mandatory). Set labels at creation time with `cfwf issue create --label`, and read an item's labels with `cfwf issue view` or `cfwf pr view --field labels`. When label commands arrive, never replace an item's whole label set: only add labels, and never remove automation-applied labels (see [task-workflow.instructions.md](task-workflow.instructions.md#label-management-mandatory)).

## Search

```bash
# One repository's issues: cfwf
cfwf issue search --repo <owner>/<repo> --text "<words>" [--state open|closed|all]
```

Searching PRs, code and repositories is not available yet; see [Operations Not Yet Available](#operations-not-yet-available-mandatory).

## Repos, Workflows, Runs

```bash
# Default branch, visibility, merge methods and description / collaborators and roles
cfwf repo view --repo <owner>/<repo>
cfwf repo collaborators --repo <owner>/<repo>

# Security advisories: the repository's, or one global advisory by GHSA id
cfwf repo advisories --repo <owner>/<repo>
cfwf advisory view --ghsa <GHSA-xxxx-xxxx-xxxx>

# The checks a branch requires, from its protection and rulesets
cfwf branch required-checks --repo <owner>/<repo> --branch <name>

# Releases and tags (used when auditing GitHub Actions version pins, see github-workflows.instructions.md)
cfwf release view --repo <owner>/<action>
cfwf release list --repo <owner>/<action>
cfwf tag view --repo <owner>/<action> --tag <tag>

# Actions runs: list for a branch or commit, a run's jobs, its failed steps' log tail, rerun its failed jobs
cfwf run list --repo <owner>/<repo> --branch <name> [--workflow <name>] [--limit <n>]
cfwf run list --repo <owner>/<repo> --sha <commit> [--workflow <name>] [--limit <n>]
cfwf run view --repo <owner>/<repo> --run <run-id>
cfwf run log --repo <owner>/<repo> --run <run-id> [--job <id>] [--lines <n>]
cfwf run rerun --repo <owner>/<repo> --run <run-id>
```

Listing an owner's repositories or a repository's workflows is not available yet; see [Operations Not Yet Available](#operations-not-yet-available-mandatory).

## Raw API Calls

Agents never call the GitHub REST or GraphQL API directly. Comments, collaborators, the authenticated login, releases and tags, review threads and replies, run logs and the board all have `cfwf` commands (above). For anything else, follow [Operations Not Yet Available](#operations-not-yet-available-mandatory) or raise a `tool(cfwf):` issue as [Choosing a GitHub Tool](#choosing-a-github-tool-mandatory) describes.

### Inline PR Review Comments

To reply to an existing review comment thread, use `cfwf pr reply --repo <owner>/<repo> --pr <number> --comment-id <inline id> --body-file <file|->`, taking the `inline` id from `cfwf pr comments`. Starting a new inline review comment is not available yet (see [Operations Not Yet Available](#operations-not-yet-available-mandatory)); put the finding in a PR comment instead, naming the file and line.

## GitHub State Lags Behind Writes (MANDATORY)

GitHub's API is asynchronous: a change can take seconds, sometimes longer, to show up in a read. This applies to anything that lags, including Workflow board fields, labels and closing-issue references. It is GitHub's behaviour, not a fault in `cfwf`, the orchestrator or the API proxy, so do not raise issues on `credfeto/credfeto-orchestrator` or `credfeto/github-api-proxy` for it.

- **A write whose call succeeded is done.** Do not re-read it to confirm.
- **Never spam GitHub while waiting for a change to show.** Do not repeat a write, or poll or loop on a read, because a read straight after a write has not caught up yet. (Waiting for a human to act, as in [Waiting for Approval in an Interactive Session](agent-roles.instructions.md#waiting-for-approval-in-an-interactive-session), is a different, sanctioned wait.)
- **A read that disagrees with a write you just made is lag, not a failure.** If a later step reads it anyway, carry on and check again at a later step; repeat the write only if the value is still wrong then. There is no fixed wait.

## Comment and Body Text (MANDATORY: HEREDOC, never `\n`)

Every `cfwf` command that posts text takes it from a file (`--body-file`, `--description-file`), with `-` reading it from stdin; there is no inline `--body`. Pass multi-line text on stdin with a HEREDOC, or write it to a file first, so real newline characters are embedded. **Never** use escaped `\n` sequences in `cfwf` input; GitHub renders them as literal backslash-n characters, not line breaks:

```bash
cfwf comment add --repo <owner>/<repo> --issue <number> --body-file - <<'COMMENT'
First paragraph.

Second paragraph.
COMMENT
```

Only one input of a command can be `-`. For `cfwf pr hand-off`, pass the description on stdin and leave out `--body-file` unless the review record needs extra text.

## Common Mistakes

Check here before assuming a `cfwf` flag or field exists.

- **Assigning yourself is `cfwf issue edit`/`cfwf pr edit --assign-self`.** There is no separate assign command.

- **`cfwf` has no `--json`/`--jq`.** Its output has one fixed format per command, so name what you need with its own options (for example `cfwf pr view --field`), as `cfwf help <command>` lists them.

- **Text goes in a file or on stdin.** Every body, description and comment is `--body-file`/`--description-file`, never an inline argument; see [Comment and Body Text](#comment-and-body-text-mandatory-heredoc-never-n).

- **The Claude Code hooks reject some shell shapes outright**, such as `env`/`unset` wrappers and the `xargs` and loop shapes covered by [Ad-Hoc Commands Never Use `xargs` or `while IFS= read` Loops](claude-hooks.instructions.md#ad-hoc-commands-never-use-xargs-or-while-ifs-read-loops-mandatory). Use flat commands instead: run `cfwf` directly and scope it with its own flags (for example `--repo <owner>/<repo>`) rather than wrapping it to set or clear a variable, and replace the `xargs` and loop shapes as that section describes.

When a command's exact flags are uncertain, run `cfwf help <command>` rather than guessing from memory or from a similar-looking command.
