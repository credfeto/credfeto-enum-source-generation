# GitHub Workflows Instructions

[Back to Global Instructions Index](index.md)

## Third-Party Action Policy

When adding or reviewing a `uses:` reference in a workflow or composite action, categorise it as follows:

- **Always allowed**: `actions/*` and `github/*` (official GitHub-owned actions)
- **Always allowed**: `credfeto/*` (project-owned actions)
- **Convert to github-script**: actions that perform simple GitHub REST or GraphQL API operations — see [Converting to github-script](#converting-to-github-script)
- **Acceptable as-is**: actions that require specialised external tooling that cannot be expressed via the GitHub API — see [Cannot Convert](#actions-that-cannot-be-converted)

Do not add new third-party actions without first determining which category they fall into.

## Converting to github-script

Use `actions/github-script` to replace third-party actions that perform any of the following:

- Reading a file from the workspace
- Creating, finding, approving, or merging a pull request
- Deleting a branch
- Adding or syncing labels on a PR or issue
- Assigning a user to a PR or issue
- Enabling auto-merge on a PR (via GraphQL)
- Checking PR commits for merge commits

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

## Actions That Cannot Be Converted

Do not attempt to replace these with github-script — they require specialised tooling that the GitHub API does not provide:

- **Docker toolchain**: `docker/build-push-action`, `docker/login-action`, `docker/setup-buildx-action`, `docker/setup-qemu-action`
- **AWS credential management**: `aws-actions/configure-aws-credentials`
- **Git operations** (rebase, auto-commit, conflict detection): `stefanzweifel/git-auto-commit-action`, `bbeesley/gha-auto-dependabot-rebase`, `olivernybroe/action-conflict-finder`
- **Security scanning**: `trufflesecurity/trufflehog`
- **Multi-language linting**: `super-linter/super-linter`
- **Complex config-driven label sync**: `crazy-max/ghaction-github-labeler`

## Version Pinning

All `uses:` references must be pinned to a specific released version. Never use `@latest`, `@main`, or `@master`.

Correct: `uses: actions/github-script@v9.0.0`
Wrong:   `uses: actions/github-script@latest`

## Bash Steps

Prefer `actions/github-script` over inline bash (`run: |`) for any step that is non-trivial — i.e. anything beyond a single-line command or a straightforward secret/env check. In particular, use `actions/github-script` for:

- Any step that calls the GitHub REST or GraphQL API
- Any step that processes git log output to make API decisions (e.g. creating branches, detecting missing releases)
- Any step that reads or writes files as part of a workflow (not as part of a build tool)
- Any step that manipulates PR metadata (labels, assignees, draft state)

If a bash step performing non-trivial logic appears in more than one workflow or composite action after the repository checkout step, extract it into a local composite action at `.github/actions/<name>/action.yml` rather than duplicating it.

Simple, single-purpose steps that do not call external APIs and are short enough to be self-evident (e.g. `sudo chown -R "$USER:$USER" "$GITHUB_WORKSPACE"`) may remain as bash.

## Dead Steps

If a step's output is never referenced by any subsequent step or job output, remove the step entirely. Do not leave unreferenced steps in workflows or composite actions.

## Permissions

- Declare `permissions:` at workflow level with the minimum required (default to `contents: read`).
- Override at job level with narrower or broader write permissions as needed.
- Never rely on the default `GITHUB_TOKEN` write permissions without an explicit `permissions:` declaration.

## Workflow Structure

- Use `concurrency:` with a meaningful `group:` key and `cancel-in-progress: true` on workflows triggered by pushes to feature branches (to avoid wasted runs on rapid pushes).
- Use `cancel-in-progress: false` on workflows where in-progress runs must not be interrupted (e.g. release workflows, dependency merge).
- Check required secrets with an explicit `run:` step before performing any operation that depends on them. Fail fast with a clear error message.

## Composite Action Placement

All local composite actions live under `.github/actions/<name>/action.yml`. Name the directory to describe what the action does (e.g. `read-file`, `approve-pr`, `find-pull-request`).

When the same step pattern appears in more than one workflow or composite action, extract it into a local composite action — do not copy-paste.
