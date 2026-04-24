# GitHub Workflows Instructions

[Back to Global Instructions Index](index.md)

## Third-Party Action Policy

When adding or reviewing a `uses:` reference in a workflow or composite action, categorise it as follows:

- **Always allowed**: `actions/*` and `github/*` (official GitHub-owned actions)
- **Convert to github-script or local action**: all other third-party actions — see the sections below for guidance on which conversion to apply
- **Acceptable as-is**: actions that require specialised external tooling that cannot be expressed via the GitHub API or a simple bash step — see [Cannot Convert](#actions-that-cannot-be-converted)

Do not add new third-party actions without first determining which category they fall into. When encountering existing third-party actions (including `credfeto/*`), replace them with local equivalents where practical.

## Converting to github-script (wrap in local composite action)

Use `actions/github-script` to replace third-party actions that perform any of the following:

- Reading a file from the workspace
- Creating, finding, approving, or merging a pull request
- Deleting a branch
- Adding or syncing labels on a PR or issue
- Assigning a user to a PR or issue
- Enabling auto-merge on a PR (via GraphQL)
- Checking PR commits for merge commits
- Checking repository visibility (public vs private)

When converting, always wrap the `actions/github-script` step in a **local composite action** placed at `.github/actions/<name>/action.yml`. Do not inline the script directly in workflow files — use the local action instead. This keeps the conversion reusable and the workflow files clean.

The local action must:
- Mirror the inputs and outputs of the replaced action so callers need only change the `uses:` line
- Use `runs.using: composite`
- Pin `actions/github-script` to a specific version (e.g. `@v9.0.0`)
- Pass any file paths or user-controlled strings via `env:` rather than string interpolation into the `script:` body, to prevent injection

### Minimal local action template

```yaml
name: "Action Name"
description: "What it does"

inputs:
  my-input:
    description: "..."
    required: true

outputs:
  my-output:
    description: "..."
    value: ${{steps.the-step.outputs.my-output}}

runs:
  using: composite
  steps:
    - name: "Do Thing"
      id: the-step
      uses: actions/github-script@v9.0.0
      env:
        MY_INPUT: ${{inputs.my-input}}
      with:
        script: |
          // use process.env.MY_INPUT, not ${{inputs.my-input}}, in the script body
          core.setOutput('my-output', result);
```

## Simple Bash Replacements

Some third-party actions perform tasks simple enough to replace with a bash step directly — no `github-script` needed:

- **Merge conflict markers**: `git grep -rl '^<<<<<<< ' --` — fails if any file contains conflict markers
- **Case sensitivity conflicts**: `git ls-files | sort -f | awk 'BEGIN{prev=""} tolower($0)==tolower(prev){print prev; print $0} {prev=$0}'`
- **Tracked files matching `.gitignore`**: `git ls-files -i --exclude-standard`
- **Dotnet SDK version from global.json**: `jq -r '.sdk.version' src/global.json` — read once, set `DOTNET_VERSION` env var; fall back to a default if the file is absent

When replacing an action with a bash step, keep the step name consistent with what it was before so PR history is legible.

## Actions That Cannot Be Converted

Do not attempt to replace these with github-script or bash — they require specialised tooling that the GitHub API does not provide:

- **Docker toolchain**: `docker/build-push-action`, `docker/login-action`, `docker/setup-buildx-action`, `docker/setup-qemu-action`
- **AWS credential management**: `aws-actions/configure-aws-credentials`
- **Git operations** (rebase, auto-commit): `stefanzweifel/git-auto-commit-action`, `bbeesley/gha-auto-dependabot-rebase`
- **Security scanning**: `trufflesecurity/trufflehog`
- **Multi-language linting**: `super-linter/super-linter`
- **Complex config-driven label sync**: `crazy-max/ghaction-github-labeler`

## Version Pinning

All `uses:` references must be pinned to a specific released version tag. Never use `@latest`, `@main`, `@master`, or a bare major version tag (e.g. `@v6`). Branch references are also forbidden — they can change without notice.

Correct: `uses: actions/github-script@v9.0.0`
Wrong:   `uses: actions/github-script@latest`
Wrong:   `uses: actions/labeler@v6`
Wrong:   `uses: some-owner/some-action@fix/some-branch`

## Keeping Actions Up to Date

Whenever you add or modify a `uses:` reference — whether in a workflow file or a composite action — check that all actions referenced in that file are on the latest released version:

1. For each `uses:` in the file, run `gh api repos/<owner>/<action>/releases/latest --jq '.tag_name'` to find the current latest release.
2. If the pinned version is behind, update it in the same commit as the other changes to that file.
3. Do not leave a file with a mix of updated and stale versions after touching it.

This applies to all actions in the file, not just the one being added or changed.

## Bash Steps vs github-script

**Default to `actions/github-script`** for any step that does more than a single command. Bash is acceptable only for steps that are genuinely simpler as shell — see the exhaustive list of exceptions below.

Use `actions/github-script` for:

- Any step that calls the GitHub REST or GraphQL API
- Any step that processes git log output to make API decisions (e.g. creating branches, detecting missing releases)
- Any step that reads or writes files as part of a workflow (not as part of a build tool)
- Any step that manipulates PR metadata (labels, assignees, draft state)
- Any step with conditional logic that compares or transforms values before deciding what to do — `if/else` in JavaScript is clearer than nested bash conditionals
- Any step that reads structured data (JSON, YAML) — use `JSON.parse` rather than `jq` pipelines

Bash is acceptable only for steps that meet **all** of the following:
- Single command or a small sequence with no branching logic
- No GitHub API calls
- No structured data parsing
- The intent is immediately obvious without comments (e.g. `sudo chown -R "$USER:$USER" "$GITHUB_WORKSPACE"`, `rm -fr "$HOME/.dotnet"`)

If a bash step performing non-trivial logic appears in more than one workflow or composite action after the repository checkout step, extract it into a local composite action at `.github/actions/<name>/action.yml` rather than duplicating it.

## Step Field Ordering

All steps must use a consistent field order. `name:` is always first; optional fields follow in the order shown below. Omit any field that is not needed.

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

- `name:` is always first.
- `id:` immediately follows `name:`, and only appears when the step output is referenced by a later step or job output.
- `if:` follows `id:` (or `name:` when there is no `id:`).
- **`shell: bash` is mandatory on every `run:` step — a `run:` step without `shell:` is invalid and must not be written or accepted in review.**
- `env:` and `with:` values are **maps** (`KEY: value` pairs) — never use list syntax (`- key: value`).
- All string values must be double-quoted.
- Indentation is 2 spaces per YAML level throughout. Steps under `steps:` are indented 6 spaces (2 for the job body + 2 for the list item + 2 for the `-`). Fields within a step are indented 8 spaces.

## Step Output Formatting

Make step output easy to scan at a glance. Use tick and cross characters to signal pass/fail state, and prefix status lines consistently:

| State | Character | Usage |
|-------|-----------|-------|
| Pass / found / enabled | `✅` | Something is present, correct, or succeeded |
| Fail / missing / disabled | `❌` | Something is absent, wrong, or failed |
| Warning / skipped | `⚠️` | Something was skipped or needs attention |
| Info / in-progress | `ℹ️` | Neutral informational output |

### In `actions/github-script` steps

```javascript
core.info(`✅ Branch ${branchName} already exists`);
core.info(`✅ Updated global.json to version ${version}`);
core.info(`❌ Branch ${branchName} not found — creating`);
core.setFailed(`❌ Found ${mergeCommits.length} merge commit(s). Please rebase.`);
core.warning(`⚠️ No releases found — nothing to do`);
```

### In bash steps

```bash
echo "✅ No merge conflict markers found."
echo "❌ Merge conflict markers found — resolve before merging."
echo "✅ src/global.json found — detected SDK version: $version"
echo "⚠️ src/global.json not found — using fallback version: 10.0.*"
```

Use `echo "::error::..."` (bash) or `core.setFailed(...)` (github-script) only for conditions that must fail the step. Use the tick/cross prefix in the message body for readability regardless of which output function is used.

### Surfacing key values without log diving

For any value that is important to see at a glance after a run (version numbers, deployed stack names, PR URLs, branch names), emit it with **both** `core.info` and `core.notice`. `core.notice` creates a job annotation that surfaces at the top of the run summary and in the PR checks UI — no log scrolling needed.

```javascript
// \u001b[38;5;6m = ANSI 256-colour cyan; colours the value in the step log
// core.notice surfaces it as a job annotation visible in the run summary
core.info(`Version: \u001b[38;5;6m${version}`);
core.notice(`Version: ${version}`);
```

Use `core.notice` for values a human would want to see first: build version, deployment target, branch created, cache hit/miss status. Do not use it for internal diagnostic details that are only useful when debugging.

## Dead Steps

A step is dead and should be removed only if **both** of the following are true:

1. Its output is never referenced by any subsequent step or job output.
2. It has no meaningful side effect of its own — i.e. it does not configure the environment, install tools, run a check that can fail the job, or produce an artifact.

Steps with side effects must be kept even when their outputs are not referenced. Examples of steps that have side effects and are never dead:

- `aws-actions/configure-aws-credentials` — configures shell environment with credentials
- `actions/setup-dotnet` / `actions/setup-node` — installs a runtime
- `trufflesecurity/trufflehog` — fails the job if secrets are found
- `super-linter/super-linter` — fails the job on lint errors
- Any step that writes to `$GITHUB_ENV` as its primary purpose

A step whose *only* purpose is to produce an output that nothing reads — for example a visibility check with no `id:` and no downstream `steps.<id>.outputs.*` references — is dead and should be removed.

## Checkout Configuration

Minimise the checkout depth and tag fetching to exactly what the job requires:

- **Default**: `fetch-depth: 1` — sufficient for jobs that only read files, run build tools, or scan the working tree
- **Full history** (`fetch-depth: 0`): required only when the job uses `git diff --merge-base`, `git log`, `git rev-list`, or any command that traverses commit history (e.g. changelog diff checks, trufflehog, missing-release detection)
- **`fetch-tags: true`**: required only when the job reads, creates, or compares tags
- **`clean: true`**: include on self-hosted runners to ensure a clean workspace; may be omitted on GitHub-hosted runners if not needed

Jobs that check static file content, scan for markers, or invoke build/lint tools never need deep history or tags.

## Permissions

- Declare `permissions:` at workflow level with the minimum required (default to `contents: read`).
- Override at job level with narrower or broader write permissions as needed.
- Never rely on the default `GITHUB_TOKEN` write permissions without an explicit `permissions:` declaration.

## Workflow Structure

- Use `concurrency:` with a meaningful `group:` key and `cancel-in-progress: true` on workflows triggered by pushes to feature branches (to avoid wasted runs on rapid pushes).
- Use `cancel-in-progress: false` on workflows where in-progress runs must not be interrupted (e.g. release workflows, dependency merge).
- Check required secrets with an explicit `run:` step before performing any operation that depends on them. Fail fast with a clear error message.

## Collapsing Multi-Step Groups

Before extracting a group of steps into a composite action, consider whether the group can be collapsed into a **single `actions/github-script` step** with a few inline `if` statements. If the combined logic fits in 20–30 lines and has no reuse value elsewhere, collapsing is preferable to creating a new composite action file.

A group is a good candidate for collapsing when:
- Each step is a conditional variant of the same operation (e.g. "do X if flag is set, else do Y")
- The steps share no external tool requirements — only API calls or env-var writes
- The resulting script body would be straightforward to read without comments

A group should become a composite action instead when:
- The same sequence appears in two or more workflows or actions
- The group involves non-trivial inputs/outputs that callers need to vary
- The steps mix `actions/github-script` with other `uses:` steps that cannot be inlined

## Composite Action Placement

All local composite actions live under `.github/actions/<name>/action.yml`. Name the directory to describe what the action does (e.g. `read-file`, `approve-pr`, `find-pull-request`).

When the same step pattern appears in more than one workflow or composite action, extract it into a local composite action — do not copy-paste.

> **Any `uses:` step — whether a local action (`./.github/actions/...`) or a remote action (`actions/github-script`, etc.) — must only appear after `actions/checkout` has run in that job.** Keep all pre-checkout steps as inline `run:` bash steps. This includes secret-presence checks, workspace ownership fixes, and environment variable setup.

### Infrastructure steps that repeat across every job

Boilerplate setup blocks that appear at the top of nearly every job are high-value extraction targets even though they have no inputs or outputs — removing 2–3 duplicate lines from 30 jobs is still worth it. Examples:

- Secret presence check → inline `github-script` step or a thin local action that takes the token as an input and fails with a clear ❌ message if empty
- Workspace ownership fix (`sudo chown`) and active environment variable setup are pre-checkout steps — keep them as inline `run:` bash steps, not local actions

When extracting pure-infrastructure steps, the action may have no declared `outputs:` and take only optional inputs for minor variants (e.g. an `include-temp` flag for jobs that also need `TEMP`/`TMP` set).

### Unifying invocations that differ only in how a value is sourced

When the same third-party action is called in multiple places with identical parameters except for one value that is sourced differently (e.g. `pr-number` comes from an event payload in one caller and from a previous step output in another), extract it into a local action that always requires that value as an explicit input. Each caller then passes its own source:

```yaml
# pull_request trigger — PR is the current event
pr-number: ${{github.event.pull_request.number}}

# push trigger — PR was just created
pr-number: ${{steps.open-pr.outputs.pr_number}}
```

This makes all invocations consistent and removes the implicit context-dependent behaviour from the third-party action.

## Composite Action Inputs vs Environment Variables

Composite actions must never silently depend on environment variables set by the caller. Relying on ambient env vars creates invisible contracts — the action appears to work until a job that doesn't set those vars calls it and it fails with an obscure error.

### Prefer explicit inputs

Declare every required value as a named input with `required: true`. The caller passes it explicitly via `with:`, making the dependency visible at the call site:

```yaml
# action.yml
inputs:
  github-token:
    description: "GitHub token with write access"
    required: true
  pr-number:
    description: "Pull request number"
    required: true
```

```yaml
# caller
uses: ./.github/actions/my-action
with:
  github-token: ${{secrets.SOURCE_PUSH_TOKEN}}
  pr-number: ${{github.event.pull_request.number}}
```

### When env vars cannot be avoided

Some steps invoke external tools that read env vars by convention (e.g. `DOTNET_VERSION`, `NUGET_FEED`). If the action cannot express these as inputs, add a validation step as the **first step** in the action that checks each required env var and fails immediately with a clear ❌ message naming the missing variable:

```yaml
    - name: "Validate Environment"
      uses: actions/github-script@v9.0.0
      with:
        script: |
          const required = ['DOTNET_VERSION', 'NUGET_FEED'];
          const missing = required.filter(k => !process.env[k]);
          if (missing.length) {
            core.setFailed(`❌ Required environment variables are not set: ${missing.join(', ')}`);
          } else {
            core.info('✅ All required environment variables are set');
          }
```

**Rules:**

- Prefer `inputs:` over env var dependencies in all cases — explicit is always better than implicit.
- Never read an env var in an action step without either declaring it as an input or validating its presence first.
- Validation must be the first step so failures are reported before any side effects occur.
- The error message must name every missing variable — do not make the caller guess.
