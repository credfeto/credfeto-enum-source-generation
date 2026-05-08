# GitHub Workflows Examples

[Back to GitHub Workflows Instructions](github-workflows.instructions.md) | [Back to Global Instructions Index](index.md)

Load this file when creating or scaffolding a new local composite action.

## Minimal Composite Action Template

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

## Explicit Inputs Declaration

Declare every required value as a named input with `required: true`:

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

## Env-Var Validation Step

When env vars cannot be avoided, add this as the **first step** in the action:

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

## Step Output Formatting — github-script

```javascript
core.info(`✅ Branch ${branchName} already exists`);
core.info(`✅ Updated global.json to version ${version}`);
core.info(`❌ Branch ${branchName} not found — creating`);
core.setFailed(`❌ Found ${mergeCommits.length} merge commit(s). Please rebase.`);
core.warning(`⚠️ No releases found — nothing to do`);
```

## Step Output Formatting — bash

```bash
echo "✅ No merge conflict markers found."
echo "❌ Merge conflict markers found — resolve before merging."
echo "✅ src/global.json found — detected SDK version: $version"
echo "⚠️ src/global.json not found — using fallback version: 10.0.*"
```

## Surfacing Key Values (core.info + core.notice)

```javascript
// \u001b[38;5;6m = ANSI 256-colour cyan; colours the value in the step log
// core.notice surfaces it as a job annotation visible in the run summary
core.info(`Version: \u001b[38;5;6m${version}`);
core.notice(`Version: ${version}`);
```

## Unifying Invocations — Caller Pattern

```yaml
# pull_request trigger — PR is the current event
pr-number: ${{github.event.pull_request.number}}

# push trigger — PR was just created by a previous step
pr-number: ${{steps.open-pr.outputs.pr_number}}
```
