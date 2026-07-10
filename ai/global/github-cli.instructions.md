# GitHub CLI (`gh`) Instructions

[Back to Global Instructions Index](index.md)

Reference for every `gh` invocation used across this instruction set. Other files describe **when**/**why** to call `gh` (see [task-workflow.instructions.md](task-workflow.instructions.md) and [agent-roles.instructions.md](agent-roles.instructions.md)); this file is the single source of truth for **exact command syntax**. If you need to pre-approve/whitelist `gh` invocations, the commands below are the full set.

Always pass `--repo <owner>/<repo>` explicitly rather than relying on the current directory's remote — required when `GH_HOST` is set (see below), and safer in general when scripting.

## `GH_HOST` Proxy Behavior (MANDATORY when set)

Read this section first — it constrains every command in the rest of this document.

When `GH_HOST` is set to a value other than `github.com`, `gh` routes through a proxy:

- **`gh pr create`:** always pass both `--repo <owner>/<repo>` and `--head <owner>:<branch>`. Without `--repo`, `gh` performs a client-side check that a git remote URL's hostname matches `GH_HOST` — since remotes use `github.com` but `GH_HOST` is the proxy host, no remote matches and `gh` refuses before any API request reaches the proxy. Without `--head`, `gh` may try to detect the branch from git remotes, leading to a blank head ref at the proxy's GraphQL layer.

  ```bash
  gh pr create \
    --repo <owner>/<repo> \
    --head <owner>:<branch-name> \
    --base main \
    --draft \
    --title "..." \
    --body "..."
  ```

- **Commit and push operations are always rejected by the proxy — never use `gh` for these, no exceptions.** Use the `git` CLI directly, always against the real `github.com` remote. This includes never running `gh auth setup-git` (see [Authentication](#authentication) below) — refuse the request outright rather than trying it and working around the failure.
- If a `gh` command fails, raise an issue on `credfeto/github-api-proxy` with the exact subcommand and flags, the API method (if visible), and the full error message verbatim.
- **Every command example in this document already passes `--repo` explicitly** (or, for `gh repo view`/`gh repo list`, the equivalent `[<repository>]`/`[<owner>]` positional argument, which is just as explicit and not subject to the same remote-detection check) for exactly this reason. The only commands without repo scoping are `gh search repos` (searches for repositories themselves — there is no single repo to scope to) and account-level calls (`gh auth status`, `gh api user`). If you add a new example to this doc, keep it proxy-compatible: explicit `--repo`, and `--head` on anything that creates a PR.

## Authentication

```bash
gh auth status
```

Only use `gh` to manage issues/PRs if this succeeds — see [task-workflow.instructions.md](task-workflow.instructions.md#issue-tracking). Never attempt `gh auth login` or manipulate credentials yourself; if auth is broken, stop and report it.

**Never run `gh auth setup-git` — refuse the request outright, even if asked directly.** It wires git's HTTP credential helper to `gh`, writing `url.*.insteadOf`/`pushInsteadOf` rewrite rules into git config that reroute commit/push traffic through `gh` (and, when `GH_HOST` is set, through the proxy — see [`GH_HOST` Proxy Behavior](#gh_host-proxy-behavior-mandatory-when-set) above). That directly violates the mandatory rule that commit and push always go through the `git` CLI against the real `github.com` remote. The rewrite rules also persist in the repo's local `.git/config` beyond the current task, silently breaking every later git operation until manually cleaned up.

## Issues

```bash
# Create
gh issue create --repo <owner>/<repo> \
  --title "<concise title>" \
  --body "<description>" \
  --label "<label>"

# View (human-readable, with comments)
gh issue view <number> --repo <owner>/<repo> --comments

# View (JSON — see Available JSON Fields below)
gh issue view <number> --repo <owner>/<repo> --json title,body,state,labels,assignees

# List
gh issue list --repo <owner>/<repo> --state open --json number,title,labels

# Edit — assign, label, link relationships
gh issue edit <number> --repo <owner>/<repo> --add-assignee @me
gh issue edit <number> --repo <owner>/<repo> --add-label "Blocked"
gh issue edit <number> --repo <owner>/<repo> --add-sub-issue <child-number>

# Comment (see HEREDOC rule below for multi-line bodies)
gh issue comment <number> --repo <owner>/<repo> --body "<text>"

# Close / reopen
gh issue close <number> --repo <owner>/<repo>
gh issue reopen <number> --repo <owner>/<repo>
```

### Available JSON Fields — `gh issue view`/`gh issue list`

```text
assignees, author, blockedBy, blocking, body, closed, closedAt,
closedByPullRequestsReferences, comments, createdAt, id, isPinned, issueType,
labels, milestone, number, parent, projectCards, projectItems, reactionGroups,
state, stateReason, subIssues, subIssuesSummary, title, updatedAt, url
```

Confirm with `gh issue view --help` if a field is ever in doubt — do not guess a field name.

## Pull Requests

```bash
# Create
gh pr create --repo <owner>/<repo> \
  --base main \
  --head <branch-name> \
  --draft \
  --title "<title>" \
  --body "<body>"

# View (human-readable)
gh pr view <number> --repo <owner>/<repo>

# View (JSON — see Available JSON Fields below)
gh pr view <number> --repo <owner>/<repo> --json state,mergedAt
gh pr view <number> --repo <owner>/<repo> --json closingIssuesReferences --jq '.closingIssuesReferences[].number'

# List
gh pr list --state open --repo <owner>/<repo> --json number,title,author,headRefName,url
gh pr list --repo <owner>/<repo> --label dependencies
gh pr list --repo <owner>/<repo> --head <branch-name>

# Edit — assign, label (note: --add-assignee/--add-label, NOT --assignee/--label — see Common Mistakes)
gh pr edit <number> --repo <owner>/<repo> --add-assignee @me
gh pr edit <number> --repo <owner>/<repo> --add-label "Blocked"

# Checks
gh pr checks <number> --repo <owner>/<repo>

# Comment
gh pr comment <number> --repo <owner>/<repo> --body "<text>"

# Draft / ready
gh pr ready <number> --repo <owner>/<repo> --undo   # convert to draft
gh pr ready <number> --repo <owner>/<repo>          # mark ready

# Merge
gh pr merge --auto --merge <number> --repo <owner>/<repo>

# Close / diff / checkout
gh pr close <number> --repo <owner>/<repo>
gh pr diff <number> --repo <owner>/<repo>
gh pr checkout <number> --repo <owner>/<repo>
```

### Available JSON Fields — `gh pr view`/`gh pr list`

```text
additions, assignees, author, autoMergeRequest, baseRefName, baseRefOid, body,
changedFiles, closed, closedAt, closingIssuesReferences, comments, commits,
createdAt, deletions, files, fullDatabaseId, headRefName, headRefOid,
headRepository, headRepositoryOwner, id, isCrossRepository, isDraft, labels,
latestReviews, maintainerCanModify, mergeCommit, mergeStateStatus, mergeable,
mergedAt, mergedBy, milestone, number, potentialMergeCommit, projectCards,
projectItems, reactionGroups, reviewDecision, reviewRequests, reviews, state,
statusCheckRollup, title, updatedAt, url
```

## Labels

```bash
gh label list --repo <owner>/<repo>
gh label create "<name>" --repo <owner>/<repo> --description "<description>" --color "<hex, no #>"
```

**Never** use `--label` on `gh issue edit`/`gh pr edit` — it **replaces** the entire label set. Always use `--add-label` (never `--remove-label` for automation-applied labels — see [task-workflow.instructions.md](task-workflow.instructions.md#label-management-mandatory)):

```bash
gh pr edit <number> --repo <owner>/<repo> --add-label "Security" --add-label "Urgent"
```

`--label`/`-l` is valid only on `gh issue create` / `gh pr create` (setting labels at creation time), never on `edit`.

## Search

```bash
gh search issues "<query>" --repo <owner>/<repo>
gh search prs "<query>" --repo <owner>/<repo>
gh search code "<query>" --repo <owner>/<repo>
gh search repos "<query>"
```

## Repos, Workflows, Runs

```bash
gh repo view <owner>/<repo> --json defaultBranchRef,description
gh repo list <owner> --limit 100

gh workflow list --repo <owner>/<repo>

gh run list --repo <owner>/<repo> --limit 10
gh run view <run-id> --repo <owner>/<repo> --log-failed
gh run rerun <run-id> --repo <owner>/<repo>
```

## REST and GraphQL API (`gh api`)

Use `gh api` for anything not covered by a dedicated subcommand (project boards, review-comment threads, collaborator management, releases lookups).

```bash
# REST — simple GET
gh api repos/<owner>/<repo>/collaborators --jq '.[].login'
gh api user --jq '.login'

# REST — releases (used when auditing GitHub Actions version pins, see github-workflows.instructions.md)
gh api repos/<owner>/<action>/releases/latest --jq '.tag_name'
gh api --paginate repos/<owner>/<action>/releases --jq '.[].tag_name'

# REST — POST with typed/raw fields
gh api repos/<owner>/<repo>/issues/<number>/comments \
  --method POST \
  --field body="<text>"

# GraphQL
gh api graphql \
  -f query='query($l:String!){user(login:$l){id}}' \
  -f l="<login>" \
  --jq '.data.user.id'
```

For the ProjectV2 Workflow-board update pattern (resolve node ID → add item to project → set status field), see [agent-roles.instructions.md](agent-roles.instructions.md#workflow-board) — that sequence is workflow-specific and lives there, not duplicated here.

### Inline PR Review Comments via `gh api`

```bash
gh api repos/<owner>/<repo>/pulls/<number>/comments \
  --method POST \
  --field commit_id="<sha>" \
  --field path="<repo-relative file path>" \
  --field line=<line-number> \
  --field side="RIGHT" \
  --field body="<comment text>"
```

`commit_id` must be the PR's **current head SHA** (`gh pr view <number> --json headRefOid --jq '.headRefOid'`), and `path`/`line` must fall inside that commit's actual diff — otherwise the request fails (see Common Mistakes below).

To reply to an existing review comment thread — note `-F` (typed), not `-f`, for `in_reply_to`: the API requires it as a number, and `-f` sends it as a string, failing with `"in_reply_to" is not a permitted key"` / `is not a number`:

```bash
gh api repos/<owner>/<repo>/pulls/<number>/comments \
  -X POST \
  -f body="<reply text>" \
  -F in_reply_to=<comment-id>
```

## Comment and Body Text (MANDATORY — HEREDOC, never `\n`)

When posting any `--body` (or `-f body=`) that contains, or may contain, newlines — `gh issue comment`, `gh pr comment`, `gh issue create`, `gh pr create`, `gh issue edit`, `gh pr edit`, `gh api ... -f body=`, etc. — always build it with a HEREDOC so real newline characters are embedded. **Never** use escaped `\n` sequences; GitHub renders them as literal backslash-n characters, not line breaks:

```bash
gh issue comment <number> --repo <owner>/<repo> --body "$(cat <<'COMMENT'
First paragraph.

Second paragraph.
COMMENT
)"
```

## Common Mistakes (Learned From Real Failures)

These are documented because each one has actually broken a live session — check here before assuming a flag or field exists.

1. **`--assignee`/`--label` are create-only flags.** `gh issue create`/`gh pr create` accept `--assignee`/`--label`. `gh issue edit`/`gh pr edit` do **not** — they fail with `unknown flag: --assignee` / `unknown flag: --label`. Use `--add-assignee`/`--add-label` (and `--remove-assignee`/`--remove-label`) on `edit`. There is also no `gh issue assign` subcommand — `gh issue assign <n> --assignee @me` fails; use `gh issue edit <n> --add-assignee @me`.

2. **`--json` only accepts the fixed field list for that subcommand** (see the field lists above). `timelineItems` is not a valid field on `gh issue view` — it fails with `Unknown JSON field: "timelineItems"`. To find the PR(s) that closed an issue, use `closedByPullRequestsReferences` on the issue, or `closingIssuesReferences` on the PR — do not reach for GraphQL timeline scraping first.

3. **Never merge stderr into a pipe feeding `jq`** (`gh ... 2>&1 | jq ...`). Any warning `gh` writes to stderr lands in the same stream as the JSON body and corrupts it before `jq` sees valid input — even on an otherwise-successful call — producing errors like `jq: parse error: Invalid numeric literal ...`. Let stderr go to the terminal (`gh ... | jq ...`) or redirect it to a file if you need to inspect it separately (`gh ... 2>err.log | jq ...`).

4. **Inline PR review comments via `gh api .../pulls/<n>/comments`** fail with `422 Validation Failed: pull_request_review_thread.path could not be resolved` if `commit_id` is stale or `path`/`line` don't fall inside that commit's diff. Fetch the current head SHA and the actual changed files first (`gh pr view <n> --json headRefOid`, `gh api repos/<owner>/<repo>/pulls/<n>/files --jq '.[].filename'`) rather than guessing.

5. **GraphQL ProjectV2 collaborator mutations**: the input type is `ProjectV2Collaborator`, not the plausible-looking `ProjectV2CollaboratorInput` (the latter errors with `isn't a defined input type`). Any connection field selected in a mutation's return payload (e.g. `collaborators { nodes { ... } }`) needs an explicit `first`/`last` pagination argument, or the whole mutation is rejected with `MISSING_PAGINATION_BOUNDARIES` — even though the mutation itself already took effect.

6. **`gh api -f`/`-F` are not interchangeable.** `-f`/`--raw-field` always sends a string; `-F`/`--field` sends a typed value (numbers, booleans, `@file`). Fields the API schema declares as a number — e.g. `in_reply_to` when replying to a PR review comment — must use `-F`. Using `-f in_reply_to=<id>` fails with `"in_reply_to" is not a permitted key" / "is not a number"`, because the string form doesn't match any of the schema's `oneOf` variants.

When a `gh` command's exact flags/fields are uncertain, run `gh <command> --help` (or `gh <command> <subcommand> --help`) rather than guessing from memory or from a similar-looking command.
