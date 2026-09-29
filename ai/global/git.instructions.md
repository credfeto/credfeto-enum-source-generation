# Git Instructions

[Back to Global Instructions Index](index.md)

## Language/Runtime Prerequisites (MANDATORY before any work)

Verify all required languages and runtimes are installed. If any are missing, stop: do not scaffold code or make partial changes; ask the user to install them first.

## Environment Health (MANDATORY)

If the environment is too broken to work in without first fixing infrastructure or tooling, **stop** and demand it be fixed. Do not work around broken tooling.

## Pre-Work Baseline Check (MANDATORY before starting any work)

If already on the correct, existing work branch for this task (i.e. resuming work rather than branching fresh from `main`), bring it up to date **before** running the check below; see [When to Rebase](git-rebasing.instructions.md#when-to-rebase) for the fetch/check/rebase procedure. If that procedure performed a rebase, it already ran `pre-commit-check` as its final step — that satisfies this baseline gate too; do not run it again here.

Otherwise (no rebase was needed, or you're starting a fresh branch from `main`), run this now, before starting any work on an issue or PR, to verify the repo is clean:

```bash
pre-commit-check
```

Always run this check in the background, because it is more likely than not to take a while to run; it is not an exception case to spot and handle specially. Backgrounding it does not mean walking away from it: you MUST then poll for its own completion and WAIT for it to actually finish, in this same turn/session, before doing anything else, including ending your turn. This is not optional and does not depend on how the check is invoked: see the mandatory [Background Tasks and Monitor Tool](task-workflow.instructions.md#background-tasks-and-monitor-tool-mandatory) rules for the poll-loop shape and the 30-minute deadline. Do **not** end your turn on the assumption that you will be automatically resumed once the check finishes. A fresh, single-phase invocation that is never resumed starts with no memory of the wait, and the backgrounded check is killed when the turn ends, so the result is never seen. If your own session genuinely is interactive and resumable, confirm that explicitly before treating "come back to this later" as safe; absent that confirmation, assume it is not.

- **P1.** <a id="baseline-autofix-new-branch"></a>If the check **auto-fixes** files (e.g. trailing whitespace, end-of-file) and everything else passes: commit those fixes on a **new, dedicated branch and issue** (a clean base-point, kept separate from the branch/issue for the requested work), and mark the original work item `Blocked` until the base-fix branch is merged. Do not start the requested work on top of an unmerged, auto-mutated baseline.
- **P2.** <a id="baseline-manual-fix-then-proceed"></a>If the check **fails** with errors that require manual fixes: fix and commit them first, then proceed with the original work.
- **P3.** <a id="baseline-still-fails-escalate"></a>If the check **still fails** after all fixing attempts:
  - For an issue: comment on the issue, label it `Blocked`, and do not start work.
  - For a PR: comment on the PR, label it `Blocked`, and do not continue work.

This ensures CI results are unambiguous: pre-existing failures are resolved before any new changes are introduced.

When picking up a **new issue** (branching fresh from `main`, not resuming an existing branch): once the baseline hook passes cleanly, check whether `COVERAGE.md` exists at the repo root. **A missing `COVERAGE.md` means the coverage ratchet has never been applied to this repo, not that it can be skipped.** If you are making changes, or have already made changes, to a repo without one, you **MUST** create it and then keep it maintained — this is not optional and does not depend on whether the requested work touches code coverage at all.

- If it exists, nothing further is needed here: the AI Coverage phase reads it live from `origin/main` every time it runs (see [coverage-ratchet.instructions.md](coverage-ratchet.instructions.md)), so there is no per-branch capture step and nothing to refresh after a rebase.
- If it does **not** exist, collect it now while still on `main` (run the [per-language extraction](coverage-ratchet.instructions.md#per-language-overall-coverage-extraction) procedure for each orchestrated language present), then create the work branch as normal and commit the resulting `COVERAGE.md` as its **first commit**, before starting the requested work. No separate branch or issue is needed for this, unlike the auto-fix case above: only one branch/PR is allowed open per repo at a time, so there is no concurrent-bootstrap race to isolate against. The AI Coverage phase overwrites the file again with the branch's live numbers when it runs later in this same PR (see its [bootstrap rule](coverage-ratchet.instructions.md#committed-coverage-file-mandatory)), so `COVERAGE.md` ends up with two commits over the branch's lifetime — expected, not a conflict. Do not treat this as a nice-to-have or defer it to a later PR: bootstrapping `COVERAGE.md` is a precondition of the work being tracked by the ratchet at all, and skipping it leaves the repo permanently ungated.

## Pre-Commit Hook Verification (MANDATORY before blocking)

Never block work based on inspecting config files and deducing that a tool might be missing. Always verify empirically: stage your changes and run `git commit` as normal — the pre-commit hook runs automatically and aborts the commit cleanly if it fails, leaving your staged changes intact. Only block if that actually fails with a real error.

Inspecting `.pre-commit-config.yaml` and concluding a `language: system` tool is absent is not sufficient; the tool may be installed in a location not visible to `command -v` in the current shell context.

## Build and Test Verification (MANDATORY before any commit or push)

Build must pass and all tests must pass before committing or pushing. If they fail and cannot be resolved, stop and ask. For pre-commit-specific failures (including ones requiring a fix to pre-commit itself or a component tool), see [Fixing Pre-Commit Failures](code-quality.instructions.md#fixing-pre-commit-failures-mandatory) — new/unexpected failures there are your responsibility to fix, not a reason to stop.

## Pre-Commit Branch Check

- Run `git branch --show-current` and confirm it is the expected working branch before staging or committing.
- Never commit if the current branch is `main`.
- If the branch has switched to `main` and the upstream no longer exists (merged and deleted), create a new branch before continuing.

## GitHub CLI Comment Bodies (MANDATORY)

For the HEREDOC rule for any `gh` `--body` argument that contains or may contain newlines, see [github-cli.instructions.md](github-cli.instructions.md#comment-and-body-text-mandatory-heredoc-never-n).

## GitHub Issues

- If `gh` is available, use it to manage issues for every piece of work.
- Before starting, either find a **100% matching** existing issue (confirm with user before linking) or create a new one with the original prompt and a clear description.
- The search itself is a normal, automatic part of this workflow: do not ask permission before running it. Any question is about what a match means once found (confirming a candidate before linking, as required above), never about whether to run the search itself.
- For complex or multi-component tasks, see [task-workflow.instructions.md](task-workflow.instructions.md).
- Reference issue numbers in commit messages and branch names.
- If work on an issue is abandoned, comment with findings before closing; do not abandon silently.

### AI-Initiated Issues (MANDATORY)

When raising a GitHub issue autonomously (not directly requested by a human):

- **P1.** Search for existing issues (both **open** and **closed**) covering the same topic before creating; do not create duplicates.
- **P2.** Add the `Blocked` label immediately after creating the issue so it is held for human review before being acted upon.

**Exceptions: do not add `Blocked`:**

- A human explicitly asked you to raise the issue: ask for the priority label instead, then apply it.
- The issue is raised by the dependency security detection rule (e.g. flagged during `npm install` or from a Dependabot advisory): use only the labels specified by that rule.

## GitHub CLI (`gh`) Proxy Behavior

For full `GH_HOST` proxy behaviour and the required `gh pr create` flags, see [github-cli.instructions.md](github-cli.instructions.md#gh_host-proxy-behavior-mandatory-when-set). In short: commit and push operations are always rejected by the proxy; use `git` CLI directly for those.

## Running Git Commands in a Specific Directory

- Always use `git -C <dir> <command>`; never `cd <dir> && git <command>`.
- `git -C` runs the command in the specified directory without changing the shell's working directory, using a single invocation and avoiding leaving the shell in the wrong directory for subsequent commands.
- In Claude Code the `cd` form also triggers an unnecessary permission prompt for the directory change itself.
- This applies to all git subcommands: `git -C /path status`, `git -C /path add`, `git -C /path commit`, etc.

## Destructive Commands (MANDATORY)

Before any command that can discard uncommitted work (`git reset --hard`, `git checkout`/`restore` over tracked files, `git clean`), run `git status` first. If it shows uncommitted changes you did not just create and intend to discard, stash them (`git stash -u`, `-u` to include untracked files) or commit them before proceeding. Running the destructive command directly on the assumption the tree is clean, without checking, silently discards any uncommitted work that is there; the check costs one command and is never skippable "because it should be clean".

## Scratch Review Branches

A local branch created only to review a PR, named `pr<number>-review` or `pr-<number>-review`, may be deleted without asking once the review is done, because it is a throwaway copy of an existing PR head and holds no work of its own.

- Create it tracking the PR head: `git fetch origin +refs/pull/<number>/head:refs/remotes/origin/pr/<number>`, then `git branch --track pr<number>-review origin/pr/<number>`. `git branch -d` checks a branch against its upstream, so without one it checks against HEAD and refuses while the PR is still open, even though every commit is safe on the PR head.
- Delete it with the same fetch chained to `git branch -d`, never `-D`: `git fetch origin +refs/pull/<number>/head:refs/remotes/origin/pr/<number> && git branch -d pr<number>-review`. The fetch is repeated because `origin/pr/<number>` sits inside origin's default refspec with no matching branch on origin, so any plain `git fetch origin` or `git pull` with `fetch.prune` set deletes it, and `-d` then checks against HEAD and refuses even an unchanged branch; the ref cannot live outside `refs/remotes/origin/*` instead, because `git branch --track` only accepts a ref that a remote's refspec maps. `-d` refuses unless the branch is merged into its upstream (or into HEAD when it has none). A branch still equal to the PR head is deleted (exit 0; the warning that it is not yet merged to HEAD is expected); a branch left with local commits is refused as `not fully merged` (exit 1). If git refuses, keep the branch and report it rather than forcing the deletion, because those local commits would otherwise be lost.
- This covers local branches only. Deleting a remote branch, or any other local branch, still needs human approval.

## Avoid `git worktree`

- Do not use `git worktree` or the native `EnterWorktree` tool to create additional working trees for a repo.
- Switch branches in the existing working directory (`git -C <dir> checkout <branch>` / `git -C <dir> switch <branch>`) instead.

## Branching

- All new work must be in a branch; never commit directly to `main`.
- Ensure `main` is up-to-date with `origin` before starting.
- Continue in the same branch until the task changes.
- Before continuing work on an existing branch, check if `origin/main` has advanced; if so, rebase first. This is done as part of the [Pre-Work Baseline Check](#pre-work-baseline-check-mandatory-before-starting-any-work) above, before the baseline hook runs; see [git-rebasing.instructions.md](git-rebasing.instructions.md) for the rebase procedure and version-conflict resolution.
- **Before creating a new branch for an issue, check whether one already exists for it**: a previous session may have pushed work and then been interrupted before ever opening a PR. This is the branch-only counterpart to the PR check in [task-workflow.instructions.md's "Bot-Created PRs" section](task-workflow.instructions.md#bot-created-prs-mandatory-treat-as-your-own) ("Checking for existing work before branching"). The glob matches the `<type>/<issue-number>-<name>` convention below (see [Branch Naming](#branch-naming)):

  ```bash
  git ls-remote --heads origin "*/<issue-number>-*"
  ```

  - No match: branch fresh from `main` as normal.
  - Match found: fetch it and compare against `main`:

    ```bash
    git fetch origin <branch>
    git rev-list --count origin/main..origin/<branch>
    ```

    - `0` (not ahead of `main`): branch fresh from `main` as normal.
    - `>0`: check it out and continue from there instead of branching again, rebasing first per the bullet above if `origin/main` has advanced.

## Pushing Branches

- **Always push a new branch with `-u`** to set up tracking: `git -C <repodir> push -u origin <branch>`.
- Subsequent pushes on a tracked branch can use `git -C <repodir> push`.
- **Never push without `-u` on the first push**: without it the branch has no upstream and later `git push` and `git pull` commands will fail.

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

See [git-commits.instructions.md](git-commits.instructions.md); load when about to commit.

## Dependabot Vulnerability Warnings

After any push, if the remote reports vulnerabilities:

- Check for open Dependabot PRs covering them (`gh pr list --label dependencies`).
- If none exist, visit the repo's Dependabot page and for any manually fixable advisory create a GitHub issue labelled `Security` and `AI-Work`, naming the package, severity, and fix steps.

## Pre-Commit Hook Known Incompatibilities

- **dotenv-linter**: use `entry: dotenv-linter check`; v3.x requires the `check` subcommand before the filename.

## Template Rule Escalation (Non-Template Repos Only)

When working outside `credfeto/cs-template` and a gap in the global template rules is found:

- **P1.** Do not apply the change locally.
- **P2.** Create an issue in `credfeto/cs-template`; see [git.examples.md](git.examples.md) for the command template.
- **P3.** The issue must include: source repository, current behaviour/gap, proposed rule text, reason for template propagation.
- **P4.** Note the issue URL in any relevant commit or PR description.
- **P5.** Continue work without waiting for the template issue to be resolved.
