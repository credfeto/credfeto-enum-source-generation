# GitHub Command Instructions

[Back to Global Instructions Index](index.md)

Reference for every GitHub operation used across this instruction set: the `cfwf` MCP tools and `cfwf` commands that cover them. Other files describe **when**/**why** to call them (see [task-workflow.instructions.md](task-workflow.instructions.md) and [agent-roles.instructions.md](agent-roles.instructions.md)); this file is the single source of truth for **exact command syntax**. `cfwf help <command>` is authoritative for a `cfwf` command's options and exit codes.

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

Run **one `cfwf` command per Bash call**, on its own: no `;`, `&&`, `|` or `echo exit=$?` around it. Read its exit code from the tool result. A compound command that contains `cfwf` is refused in unattended runs and stalls the session. When you need several facts about a PR at once (its metadata, board status, check verdict, when it was marked ready, recent comment activity), run `cfwf pr status` once instead of several commands: it exists so you never need to chain them.

There is no other route to GitHub for agents. `cfwf` (in the `credfeto/credfeto-orchestrator` agent image) is where every GitHub operation an agent needs is provided as a standardised, pre-canned command. If `cfwf` is not installed or a command fails, follow [Missing CLI Tools](task-workflow.instructions.md#missing-cli-tools-mandatory) and report it.

When no `cfwf` command or MCP tool covers an operation you need, raise an issue on `credfeto/credfeto-orchestrator` asking for it:

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

## Issues

```bash
# Create (title per Issue Titles below); also puts it on the Workflow board as Not Started
cfwf issue create --repo <owner>/<repo> --priority <Security|Urgent|High|Medium|Low> \
  --title "<prefix>: <concise title>" --body-file - [--label "<label>" ...] <<'BODY'
<description>
BODY

# View: title, state, labels, assignees, then the body; --comments adds the comments after a
# `section<TAB>comments` line (the last such line is the real separator)
cfwf issue view --repo <owner>/<repo> --issue <number> [--comments]

# Whether it has a plan, whether it is Blocked, and who commented since the plan
cfwf issue plan-status --repo <owner>/<repo> --issue <number>

# List open issues, highest priority first / search one repository's issues
cfwf issue list --repo <owner>/<repo>
cfwf issue search --repo <owner>/<repo> --text "<words>" [--state open|closed|all]

# Edit: title, body (replaces the whole body), assign yourself
cfwf issue edit --repo <owner>/<repo> --issue <number> --assign-self
cfwf issue edit --repo <owner>/<repo> --issue <number> [--title "<title>"] [--body-file <file|->]

# Comment (see HEREDOC rule below for multi-line bodies) / correct one of your own comments by its id
cfwf comment add --repo <owner>/<repo> --issue <number> --body-file - <<'COMMENT'
<text>
COMMENT
cfwf comment edit --repo <owner>/<repo> --comment-id <id> --issue-comment --body-file <file|->

# Read the top-level comments with their bodies, oldest first (also works for a PR's conversation comments)
cfwf issue comments --repo <owner>/<repo> --issue <number> [--since <YYYY-MM-DDTHH:MM:SSZ>] [--author <login>]

# Read one issue or PR conversation comment by its id (header lines, then the `|`-prefixed body)
cfwf comment view --repo <owner>/<repo> --comment-id <id>

# Post the implementation plan: comment, then Blocked, then the board to Planning
cfwf issue post-plan --repo <owner>/<repo> --issue <number> --body-file - <<'PLAN'
## Implementation Plan
<plan>
PLAN

# Ask a blocking question: comment, then Blocked; the board is left as it is
cfwf issue block --repo <owner>/<repo> --issue <number> --body-file - <<'QUESTION'
<question, citing the instruction that requires the stop>
QUESTION

# Close with a reason (optional comment first) / reopen
cfwf issue close --repo <owner>/<repo> --issue <number> --reason <completed|not-planned> [--body-file <file|->]
cfwf issue reopen --repo <owner>/<repo> --issue <number> [--body-file <file|->]

# Link a sub-issue to its parent (both in the same repository)
cfwf issue add-sub-issue --repo <owner>/<repo> --issue <parent> --sub-issue <child>

# When each label was added or removed, and by whom
cfwf issue label-history --repo <owner>/<repo> (--issue <n> | --pr <n>) [--label <name>]
```

Labels are in [Labels](#labels). `cfwf issue plan-status` lists who commented after the plan, without the bodies; read the bodies with `cfwf issue comments --since <plan-at>`.

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

# Whole state in one call: metadata, Workflow board status, ready-at, the required-check verdict with the failing and pending
# check names, the changed-file count and comment activity since ready; exit codes as pr checks (0 pass, 3 failed, 4 pending, 5 none).
# Start every PR phase with this, and use the single-purpose commands below only for detail it does not print.
cfwf pr status --repo <owner>/<repo> --pr <number>

# View: state and metadata, one key and value a line; name the fields you need
cfwf pr view --repo <owner>/<repo> --pr <number>
cfwf pr view --repo <owner>/<repo> --pr <number> --field state --field merged-at
cfwf pr view --repo <owner>/<repo> --pr <number> --field head-sha

# Labels of the issues the PR closes (Blocked and On Hold left out) / add them to the PR, never removing one
cfwf closing-issue-labels --repo <owner>/<repo> --pr <number>
cfwf pr sync-labels --repo <owner>/<repo> --pr <number> [--assign-self]

# List open PRs (optionally those for an issue, a head branch, or carrying every --label given)
cfwf pr list --repo <owner>/<repo> [--issue <n>] [--head <branch-name>] [--label <name> ...]

# Edit: title, body (replaces the whole body), assign yourself
cfwf pr edit --repo <owner>/<repo> --pr <number> --assign-self
cfwf pr edit --repo <owner>/<repo> --pr <number> [--title "<title>"] [--body-file <file|->]

# Checks: judge the required checks on the head once (exit 0 pass, 3 failed, 4 pending, 5 none at all)
cfwf pr checks --repo <owner>/<repo> --pr <number> [--all]

# When the PR was last marked ready
cfwf pr ready-at --repo <owner>/<repo> --pr <number>

# After merge: judge the Actions runs on the merge commit, on the base branch (exit codes as pr checks-wait)
cfwf pr post-merge-runs --repo <owner>/<repo> --pr <number>

# Comment / read every comment, review and review thread / reply to an inline review comment
cfwf comment add --repo <owner>/<repo> --pr <number> --body-file - <<'COMMENT'
<text>
COMMENT
cfwf pr comments --repo <owner>/<repo> --pr <number>
cfwf pr reply --repo <owner>/<repo> --pr <number> --comment-id <inline id> --body-file - <<'REPLY'
<reply text>
REPLY
# Start a new inline review comment on a file and line (the head commit is read for you)
cfwf pr review-comment --repo <owner>/<repo> --pr <number> --path <file> --line <n> --body-file - <<'COMMENT'
<comment>
COMMENT

# Ask a blocking question: comment, then Blocked; the board is left as it is
cfwf pr block --repo <owner>/<repo> --pr <number> --body-file - <<'QUESTION'
<question or diagnosis, citing the instruction that requires the stop>
QUESTION

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

# Close your own PR without merging it (optional comment first; the branch is kept) / reopen it
cfwf pr close --repo <owner>/<repo> --pr <number> [--body-file <file|->]
cfwf pr reopen --repo <owner>/<repo> --pr <number> [--body-file <file|->]
```

Labels are in [Labels](#labels). Never mark a PR ready, convert it to draft, or turn auto-merge on or off by any route other than `cfwf pr hand-off` and `cfwf pr take-back`: they and `cfwf pr take-back` do those steps, in the safe order, together with the board and the review record. Judge each `cfwf` command by its exit code, as `cfwf help <command>` documents it.

## Labels

```bash
# Add or remove standard labels on an issue or PR: one --label per label, never a comma-joined list
cfwf label add --repo <owner>/<repo> (--issue <n> | --pr <n>) --label "<name>" [--label "<name>" ...]
cfwf label remove --repo <owner>/<repo> (--issue <n> | --pr <n>) --label "<name>" [--label "<name>" ...]

# List a repository's labels / create one of the standard labels in it
cfwf label list --repo <owner>/<repo>
cfwf label create --repo <owner>/<repo> --name "<name>" [--color <hex>] [--description "<text>"]
```

Set labels at creation time with `cfwf issue create --label`, and read an item's labels with `cfwf issue view` or `cfwf pr view --field labels`. Add `Blocked` with the command that posts its explanation (`cfwf issue post-plan`, `cfwf issue block` or `cfwf pr block`) rather than with `cfwf label add`, so `Blocked` is never set without the comment that explains it. Copy a PR's closing issues' labels with `cfwf pr sync-labels`. Never replace an item's whole label set: only add labels, and never remove automation-applied labels (see [task-workflow.instructions.md](task-workflow.instructions.md#label-management-mandatory)).

- `cfwf label add` and `cfwf label remove` accept only the standard labels, and refuse priority labels (`Security`, `Urgent`, `High`, `Medium`, `Low`); change a priority with `cfwf issue edit --priority` or `cfwf pr edit --priority`.
- `cfwf label remove` never removes `On Hold`, and removes `Blocked` only when the agent's own login added it last; a `Blocked` a human added is lifted by that human. Exit 8 means refused, with nothing changed.

## Search

```bash
# One repository's issues / pull requests, by words in the title or body
cfwf issue search --repo <owner>/<repo> --text "<words>" [--state open|closed|all]
cfwf pr search --repo <owner>/<repo> --text "<words>" [--state open|closed|merged|all] [--limit <n>]

# One repository's code (its default branch, as GitHub indexes it): prints matching paths, never content
cfwf code search --repo <owner>/<repo> --text "<words>" [--limit <n>]

# One owner's repositories, by words in the name or description
cfwf repo search --owner <owner> --text "<words>" [--include-archived] [--limit <n>]
```

`--text` takes plain words only: no qualifiers, quoted phrases or `AND`/`OR`/`NOT`, as `cfwf help <command>` describes.

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

# An owner's repositories (archived ones left out unless asked for) / a repository's Actions workflows
cfwf repo list --owner <owner> [--include-archived]
cfwf workflow list --repo <owner>/<repo>
```

## Raw API Calls

Agents never call the GitHub REST or GraphQL API directly. Comments, labels, search, collaborators, the authenticated login, releases and tags, review threads, replies and new inline review comments, run logs and the board all have `cfwf` commands (above). For anything else, raise a `tool(cfwf):` issue as [Choosing a GitHub Tool](#choosing-a-github-tool-mandatory) describes.

### Inline PR Review Comments

To reply to an existing review comment thread, use `cfwf pr reply --repo <owner>/<repo> --pr <number> --comment-id <inline id> --body-file <file|->`, taking the `inline` id from `cfwf pr comments`. To start a new inline review comment on a file and line, use `cfwf pr review-comment --repo <owner>/<repo> --pr <number> --path <file> --line <n> [--start-line <n>] [--side RIGHT|LEFT] --body-file <file|->`: it reads the PR's head commit itself (never pass a commit SHA), prints the same `inline` and `url` lines as `cfwf pr reply`, and exits 1 with a clear message when the line is not part of the diff; post once and check `cfwf pr comments` before posting again.

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
