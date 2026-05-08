# GitHub Workflows Instructions

[Back to Global Instructions Index](index.md)

## Third-Party Action Policy

Classify every `uses:` reference before adding or reviewing:

- **Always allowed**: `actions/*` and `github/*`
- **Convert to github-script or local action**: all other third-party actions
- **Acceptable as-is**: actions requiring specialised external tooling not expressible via the GitHub API or bash — see [Cannot Convert](#actions-that-cannot-be-converted)

When encountering existing third-party actions (including `credfeto/*`), replace with local equivalents where practical.

## Converting to github-script (wrap in local composite action)

Use `actions/github-script` to replace actions that:

- Read a file from the workspace
- Create, find, approve, or merge a pull request
- Delete a branch
- Add or sync labels on a PR or issue
- Assign a user to a PR or issue
- Enable auto-merge on a PR (via GraphQL)
- Check PR commits for merge commits
- Check repository visibility (public vs private)

Wrap the `actions/github-script` step in a **local composite action** at `.github/actions/<name>/action.yml` — never inline the script in workflow files.

The local action must:

- Mirror inputs and outputs of the replaced action so callers only change the `uses:` line
- Use `runs.using: composite`
- Pin `actions/github-script` to a specific version (e.g. `@v9.0.0`)
- Pass file paths or user-controlled strings via `env:` rather than string-interpolating into the `script:` body

### Minimal local action template

See [github-workflows.examples.md](github-workflows.examples.md) for the composite action scaffold, explicit inputs declaration, and env-var validation step template.

## Simple Bash Replacements

Replace these with a bash step — no `github-script` needed:

- **Merge conflict markers**: `git grep -rl '^<<<<<<< ' --` — fails if any file contains conflict markers
- **Case sensitivity conflicts**: `git ls-files | sort -f | awk 'BEGIN{prev=""} tolower($0)==tolower(prev){print prev; print $0} {prev=$0}'`
- **Tracked files matching `.gitignore`**: `git ls-files -i --exclude-standard`
- **Dotnet SDK version from global.json**: `jq -r '.sdk.version' src/global.json` — set `DOTNET_VERSION`; fall back to a default if absent

Keep step names consistent with the original so PR history is legible.

## Actions That Cannot Be Converted

Do not replace these — specialised tooling required:

- **Docker toolchain**: `docker/build-push-action`, `docker/login-action`, `docker/setup-buildx-action`, `docker/setup-qemu-action`
- **AWS credential management**: `aws-actions/configure-aws-credentials`
- **Git operations** (rebase, auto-commit): `stefanzweifel/git-auto-commit-action`, `bbeesley/gha-auto-dependabot-rebase`
- **Security scanning**: `trufflesecurity/trufflehog`
- **Multi-language linting**: `super-linter/super-linter`
- **Complex config-driven label sync**: `crazy-max/ghaction-github-labeler`

## Version Pinning

Pin all `uses:` to a specific released version tag. Never use `@latest`, `@main`, `@master`, bare major tags (e.g. `@v6`), or branch refs.

Correct: `uses: actions/github-script@v9.0.0`
Wrong: `@latest`, `@v6`, branch refs

## Keeping Actions Up to Date

Whenever you add or modify a `uses:` reference, check all actions in that file are on the latest released version:

1. For each `uses:`, run `gh api repos/<owner>/<action>/releases/latest --jq '.tag_name'`.
2. If behind, update in the same commit.
3. Never leave a file with a mix of updated and stale versions after touching it.

## Bash Steps vs github-script

**Default to `actions/github-script`** for any step doing more than a single command.

Use `actions/github-script` for:

- Any step calling the GitHub REST or GraphQL API
- Any step processing git log output to make API decisions (e.g. creating branches, detecting missing releases)
- Any step reading or writing files as part of a workflow (not a build tool)
- Any step manipulating PR metadata (labels, assignees, draft state)
- Any step with conditional logic — `if/else` in JavaScript is clearer than nested bash conditionals
- Any step reading structured data (JSON, YAML) — use `JSON.parse` rather than `jq` pipelines

Bash is acceptable only for steps meeting **all** of:

- Single command or small sequence with no branching logic
- No GitHub API calls
- No structured data parsing
- Intent immediately obvious without comments (e.g. `sudo chown -R "$USER:$USER" "$GITHUB_WORKSPACE"`, `rm -fr "$HOME/.dotnet"`)

Extract non-trivial bash logic appearing in more than one workflow or composite action (after checkout) into a local composite action at `.github/actions/<name>/action.yml`.

## Step Field Ordering

Use this consistent field order — omit fields not needed. `name:` is always first.

### `run` steps

> **`shell:` is mandatory on every `run:` step. A `run:` step without `shell:` is invalid.**

```yaml
      - name: "Step Name"
        id: step-id           # only when output is referenced downstream
        if: <condition>       # only when conditionally run
        shell: bash
        run: |
          <commands>
        env:
          VARIABLE: "value"   # only when step-level env vars are needed
```

### `uses` steps

```yaml
      - name: "Step Name"
        id: step-id           # only when output is referenced downstream
        if: <condition>       # only when conditionally run
        uses: owner/action@vX.Y.Z
        env:
          MY_VAR: ${{inputs.some-input}}  # pass user-controlled values via env to prevent injection
        with:
          setting: "value"
```

**Rules:**

- `id:` follows `name:` — only when output is referenced downstream.
- `if:` follows `id:` (or `name:` when no `id:`).
- **`shell: bash` is mandatory on every `run:` step.**
- `env:` and `with:` values are **maps** — never list syntax.
- All string values must be double-quoted.
- Indentation: 2 spaces per YAML level; steps under `steps:` indented 6 spaces, fields within a step 8 spaces.

## Step Output Formatting

> Applies to **GitHub Actions workflow steps only**. Standalone shell scripts use ANSI-coloured `✓`/`✗` — see [shell-scripts.instructions.md](shell-scripts.instructions.md#visual-indicators).

| State | Character | Usage |
| --- | --- | --- |
| Pass / found / enabled | `✅` | Present, correct, or succeeded |
| Fail / missing / disabled | `❌` | Absent, wrong, or failed |
| Warning / skipped | `⚠️` | Skipped or needs attention |
| Info / in-progress | `ℹ️` | Neutral informational output |

See [github-workflows.examples.md](github-workflows.examples.md) for `core.info`/`core.setFailed` and bash `echo` usage examples.

Use `echo "::error::..."` (bash) or `core.setFailed(...)` (github-script) only for conditions that must fail the step.

### Surfacing key values without log diving

Emit important values (version numbers, PR URLs, branch names) with **both** `core.info` and `core.notice`. `core.notice` creates a job annotation visible in the run summary — no log scrolling needed.

See [github-workflows.examples.md](github-workflows.examples.md) for the `core.info` + `core.notice` pattern with ANSI colouring.

> **ANSI escape sequences — use the literal string, never the raw byte.**
> Write `\u001b` as six characters (`\`, `u`, `0`, `0`, `1`, `b`) inside JavaScript strings. Never insert the actual ESC byte (0x1B) into a YAML file — YAML forbids control characters and the workflow will be rejected.

Use `core.notice` for values a human would want to see first: build version, deployment target, branch created. Not for internal diagnostics.

## Dead Steps

Remove a step only if **both** are true:

1. Its output is never referenced by any subsequent step or job output.
2. It has no meaningful side effect — does not configure the environment, install tools, run a check that can fail the job, or produce an artifact.

Steps with side effects are never dead:

- `aws-actions/configure-aws-credentials` — configures shell environment with credentials
- `actions/setup-dotnet` / `actions/setup-node` — installs a runtime
- `trufflesecurity/trufflehog` — fails the job if secrets are found
- `super-linter/super-linter` — fails the job on lint errors
- Any step writing to `$GITHUB_ENV` as its primary purpose

## Checkout Configuration

Use the minimum depth and tag fetching the job requires:

- **Default**: `fetch-depth: 1` — sufficient for read, build, or scan
- **Full history** (`fetch-depth: 0`): required only for `git diff --merge-base`, `git log`, `git rev-list`, or history traversal (changelog diffs, trufflehog, missing-release detection)
- **`fetch-tags: true`**: required only when the job reads, creates, or compares tags
- **`clean: true`**: include on self-hosted runners; may be omitted on GitHub-hosted

## Permissions

- Declare `permissions:` at workflow level (default: `contents: read`).
- Override at job level as needed.
- Never rely on default `GITHUB_TOKEN` write permissions without an explicit `permissions:` declaration.

## Workflow Structure

- Use `concurrency:` with `cancel-in-progress: true` on push-triggered feature-branch workflows.
- Use `cancel-in-progress: false` on release or dependency-merge workflows.
- Check required secrets with an explicit `run:` step before any dependent operation — fail fast with a clear error.

## Collapsing Multi-Step Groups

Before extracting into a composite action, consider collapsing into a **single `actions/github-script` step**. If the logic fits in 20–30 lines with no reuse value, collapsing is preferable.

Collapse when:

- Steps are conditional variants of the same operation
- No external tool requirements — only API calls or env-var writes
- Script body is readable without comments

Extract to a composite action when:

- Same sequence appears in two or more workflows or actions
- Non-trivial inputs/outputs callers need to vary
- Steps mix `actions/github-script` with other `uses:` steps

## Composite Action Placement

All local composite actions live under `.github/actions/<name>/action.yml`. Extract any step pattern that appears in more than one workflow or action — do not copy-paste.

> **Any `uses:` step — local or remote — must only appear after `actions/checkout` has run.** Keep pre-checkout steps as inline `run:` bash steps (secret checks, workspace ownership fixes, env setup).

### Infrastructure steps that repeat across every job

Boilerplate setup blocks are high-value extraction targets. Examples:

- Secret presence check → thin local action or inline `github-script` step failing with ❌ if empty
- Workspace ownership fix (`sudo chown`) and active env var setup → keep as inline `run:` bash steps

When extracting pure-infrastructure steps, the action may have no `outputs:` and only optional inputs for minor variants.

### Unifying invocations that differ only in value source

When the same action is called in multiple places with identical parameters except one value sourced differently, extract into a local action requiring that value as an explicit input. See [github-workflows.examples.md](github-workflows.examples.md) for the caller pattern.

## Composite Action Inputs vs Environment Variables

Composite actions must never silently depend on caller-set env vars — invisible contracts cause obscure failures.

### Prefer explicit inputs

Declare every required value as a named input with `required: true`. See [github-workflows.examples.md](github-workflows.examples.md) for a full action.yml + caller example.

### When env vars cannot be avoided

Add a validation step as the **first step**, checking each required env var and failing immediately with a clear ❌ naming the missing variable. See [github-workflows.examples.md](github-workflows.examples.md) for the template.

**Rules:**

- Prefer `inputs:` over env var dependencies — explicit over implicit.
- Never read an env var without declaring it as an input or validating its presence first.
- Validation must be the first step.
- Error messages must name every missing variable.
