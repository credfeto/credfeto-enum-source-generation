# Git Instructions

[Back to Global Instructions Index](index.md)

## Language/Runtime Prerequisites (MANDATORY before any work)

Verify all required languages and runtimes are installed. If any are missing, stop — do not scaffold code or make partial changes; ask the user to install them first.

## Environment Health (MANDATORY)

If the environment is too broken to work in without first fixing infrastructure or tooling, **stop** and demand it be fixed. Do not work around broken tooling.

## Pre-Work Baseline Check (MANDATORY before starting any work)

Before starting any work on an issue or PR, run the pre-commit hooks against all tracked files to verify the repo is clean:

```bash
pre-commit run --all-files
```

1. If hooks **auto-fix** files (e.g. trailing whitespace, end-of-file): commit those fixes separately before starting the original work.
2. If hooks **fail** with errors that require manual fixes: fix and commit them first, then proceed with the original work.
3. If hooks **still fail** after all fixing attempts:
   - For an issue: comment on the issue, label it `Blocked`, and do not start work.
   - For a PR: comment on the PR, label it `Blocked`, and do not continue work.

This ensures CI results are unambiguous — pre-existing failures are resolved before any new changes are introduced.

## Pre-Commit Hook Verification (MANDATORY before blocking)

Never block work based on inspecting config files and deducing that a tool might be missing. Always verify by actually running the hook:

1. Stage your changes.
2. Run the pre-commit hook directly: `<hooks-path>/pre-commit` (find the path with `git config --global core.hooksPath`).
3. Only block if the hook **actually fails** with a real error.

Inspecting `.pre-commit-config.yaml` and concluding a `language: system` tool is absent is not sufficient — the tool may be installed in a location not visible to `command -v` in the current shell context.

## Build and Test Verification (MANDATORY before any commit or push)

Build must pass and all tests must pass before committing or pushing. If they fail and cannot be resolved, stop and ask.

## Git Identity Check (MANDATORY before any commit)

Before any commit, verify identity and GPG signing are correct — see [git.examples.md](git.examples.md) for the check script.

**If either check fails: stop all work, do not commit, and report the misconfiguration.**

## Pre-Commit Branch Check

- Run `git branch --show-current` and confirm it is the expected working branch before staging or committing.
- Never commit if the current branch is `main`.
- If the branch has switched to `main` and the upstream no longer exists (merged and deleted), create a new branch before continuing.

## GitHub CLI Comment Bodies (MANDATORY)

When posting comment or PR bodies via the GitHub CLI (`gh issue comment`, `gh pr comment`, `gh pr create`, `gh pr edit`, etc.), always pass multi-line text using a HEREDOC so that real newline characters are embedded. **Never** use escaped `\n` sequences — GitHub renders them as literal characters, not line breaks:

```bash
gh issue comment <number> --repo <owner/repo> --body "$(cat <<'COMMENT'
First paragraph.

Second paragraph.
COMMENT
)"
```

This applies to any `--body` argument that contains or may contain newlines.

## GitHub Issues

- If `gh` is available, use it to manage issues for every piece of work.
- Before starting, either find a **100% matching** existing issue (confirm with user before linking) or create a new one with the original prompt and a clear description.
- For complex or multi-component tasks, see [task-workflow.instructions.md](task-workflow.instructions.md).
- Reference issue numbers in commit messages and branch names.
- If work on an issue is abandoned, comment with findings before closing — do not abandon silently.

### AI-Initiated Issues (MANDATORY)

When raising a GitHub issue autonomously (not directly requested by a human):

1. Search for existing issues (both **open** and **closed**) covering the same topic before creating — do not create duplicates.
2. Add the `Blocked` label immediately after creating the issue so it is held for human review before being acted upon.

**Exceptions — do not add `Blocked`:**

- A human explicitly asked you to raise the issue: ask for the priority label instead, then apply it.
- The issue is raised by the dependency security detection rule (e.g. flagged during `npm install` or from a Dependabot advisory): use only the labels specified by that rule.

## GitHub CLI (`gh`) Proxy Behavior

When `GH_HOST` is set to a value other than `github.com`, `gh` routes through a proxy:

- If a `gh` command fails, raise an issue on `credfeto/github-api-proxy` with the exact subcommand and flags, the API method (if visible), and the full error message.
- Commit and push operations are always rejected by the proxy — use `git` CLI directly for all commit and push operations.
- **`gh pr create` (MANDATORY when `GH_HOST` is set):** Always pass both `--repo <owner>/<repo>` and `--head <owner>:<branch>`. Without `--repo`, gh CLI performs a client-side check that a git remote URL has a hostname matching `GH_HOST` — since remotes use `github.com` but `GH_HOST` is the proxy host, no remote matches and `gh` refuses to proceed before any API request reaches the proxy. Without `--head`, gh may try to detect the branch from git remotes, leading to a blank head ref at the proxy GraphQL layer. With both flags supplied, `gh` relies solely on API calls and bypasses the remote-URL check entirely.

  ```bash
  gh pr create \
    --repo <owner>/<repo> \
    --head <owner>:<branch-name> \
    --base main \
    --draft \
    --title "..." \
    --body "..."
  ```

## Running Git Commands in a Specific Directory

- Always use `git -C <dir> <command>` — never `cd <dir> && git <command>`.
- `git -C` runs the command in the specified directory without changing the shell's working directory, using a single invocation and avoiding leaving the shell in the wrong directory for subsequent commands.
- In Claude Code the `cd` form also triggers an unnecessary permission prompt for the directory change itself.
- This applies to all git subcommands: `git -C /path status`, `git -C /path add`, `git -C /path commit`, etc.

## Branching

- All new work must be in a branch — never commit directly to `main`.
- Ensure `main` is up-to-date with `origin` before starting.
- Continue in the same branch until the task changes.
- Before continuing work on an existing branch, check if `origin/main` has advanced — if so, rebase first.

## Resolving Version Conflicts When Merging or Rebasing

When a merge or rebase produces conflicting versions of the same package, action, or runtime (both branches changed the version), resolve each conflicting entry individually — never take a whole file wholesale from one side.

This applies to every version-bearing file, including:

- Dependency manifests: `.csproj`, `Directory.Packages.props`, `packages.config`, `package.json`, `requirements.txt`
- GitHub Actions `uses:` version pins in workflows and composite actions
- Runtime and tool versions: .NET SDK (`global.json`), `dotnet-tools.json`, Node.js (`.nvmrc`, `engines`, `setup-node` versions), Python (`.python-version`, `setup-python` versions), and similar

Rules:

1. Take the **latest** of the candidate versions.
2. **Security exception**: if the latest candidate is known to be less secure than another candidate (e.g. it has a published security advisory that the other does not), take the most recent candidate that is not affected.
3. Never resolve by downgrading below every candidate, and never invent a version that appears on neither side.
4. Lock files (`package-lock.json` and similar): do not hand-merge — resolve the manifest first, then regenerate the lock file with the package manager.
5. After the merge or rebase completes, run the build and tests. If the chosen version broke the build (API changes, removed features), fix the breakage on the same branch as part of the merge work — do not downgrade to avoid the fix.

## Pushing Branches

- **Always push a new branch with `-u`** to set up tracking: `git -C <repodir> push -u origin <branch>`.
- Subsequent pushes on a tracked branch can use `git -C <repodir> push`.
- **Never push without `-u` on the first push** — without it the branch has no upstream and later `git push` and `git pull` commands will fail.

## Command Failure Reporting (MANDATORY)

When any git command fails (push, rebase, fetch, etc.), you **must** quote the exact stdout and stderr output verbatim in any issue or PR comment before posting any explanation or diagnosis. Never substitute a narrative about why a command might have failed for the actual error output.

Capture the output into a variable and embed it in the comment body:

```bash
push_output=$(git -C /path push --force-with-lease 2>&1) || true
gh pr comment NUMBER --repo OWNER/REPO --body "$(cat <<COMMENT
git push failed with:

${push_output}
COMMENT
)"
```

This rule exists because AI-generated diagnoses of command failures are frequently wrong. The verbatim output is always correct; the explanation is not.

## Branch Naming

Format: `<type>/<name>` (mirroring Conventional Commits types):

- `feature/add-user-auth`
- `fix/null-pointer-on-login`
- `chore/update-dependencies`
- `refactor/simplify-payment-flow`

Include the issue number when applicable: `fix/123-null-pointer-on-login`.

For branches fixing multiple issues, reference each issue number in the individual commit message bodies.

## Commits

See [git-commits.instructions.md](git-commits.instructions.md) — load when about to commit.

## Dependabot Vulnerability Warnings

After any push, if the remote reports vulnerabilities:

- Check for open Dependabot PRs covering them (`gh pr list --label dependencies`).
- If none exist, visit the repo's Dependabot page and for any manually fixable advisory create a GitHub issue labelled `Security` and `AI-Work`, naming the package, severity, and fix steps.

## Pre-Commit Hook Known Incompatibilities

- **dotenv-linter**: use `entry: dotenv-linter check` — v3.x requires the `check` subcommand before the filename.

## Template Rule Escalation (Non-Template Repos Only)

When working outside `credfeto/cs-template` and a gap in the global template rules is found:

1. Do not apply the change locally.
2. Create an issue in `credfeto/cs-template` — see [git.examples.md](git.examples.md) for the command template.
3. The issue must include: source repository, current behaviour/gap, proposed rule text, reason for template propagation.
4. Note the issue URL in any relevant commit or PR description.
5. Continue work without waiting for the template issue to be resolved.
