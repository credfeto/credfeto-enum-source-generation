# Git Rebasing Instructions

[Back to Global Instructions Index](index.md)

## When to Rebase

If already on the correct, existing work branch for this task (i.e. resuming work rather than branching fresh from `main`), bring it up to date **before** running the [Pre-Work Baseline Check](git.instructions.md#pre-work-baseline-check-mandatory-before-starting-any-work), as three distinct, ordered steps:

- **P1.** **Fetch**: `git -C <repodir> fetch origin main`; always fetch first, regardless of whether a rebase turns out to be needed.
- **P2.** **Check**: `git -C <repodir> rev-list --count HEAD..origin/main`; a non-zero count means `origin/main` has advanced and a rebase is needed.
- **P3.** **Rebase**: only if P2 found new commits, rebase onto `origin/main` now, following [Resolving Version Conflicts When Merging or Rebasing](#resolving-version-conflicts-when-merging-or-rebasing) below and, when the branch has already been pushed, [Force-Pushing a Rebased Branch](#force-pushing-a-rebased-branch-mandatory), then run the [After Every Rebase](#after-every-rebase-mandatory) check.

A branch just created fresh from an up-to-date `main` doesn't need this; it starts current by construction.

## After Every Rebase (MANDATORY)

A rebase pulls in unknown content from `origin/main` (other people's commits, plus any conflict resolutions), so every rebase, of any form and for any reason, leaves the branch unverified until this check passes. It applies to every rebase in a session, not only the first one. The [Rebase Agent](agent-roles.instructions.md#rebase-agent) does not run it itself, so the Orchestrator runs it as the Post-Rebase Check once the rebase is complete, or the role that performed the rebase directly runs it itself. The Orchestrator never implements directly, so it hands every fix, commit and push to the [review-fix route](task-workflow.instructions.md#review-fix-route) (Code Fixer, Code Tester, Committer, PR Submitter, CI Monitor) rather than editing files itself.

- **P1.** Run the build and tests, then run `pre-commit-check` against all tracked files, in the background and waited on until it finishes and judged by its exit code before continuing, per [Background Tasks and Monitor Tool](task-workflow.instructions.md#background-tasks-and-monitor-tool-mandatory).
- **P2.** Fix every issue it reports, as the Pre-Work Baseline Check's [P2](git.instructions.md#baseline-manual-fix-then-proceed) does, including issues that were already present before the rebase, because it covers the whole repository. Re-run `pre-commit-check` after each round of fixes and repeat until it reports no issues. Do not continue with any other work while issues remain, and do not suppress or weaken a check to make it pass.
- **P3.** Commit each fix as its own commit on the current branch, separate from the rebase and from fixes to other constructs, per [git-commits.instructions.md](git-commits.instructions.md). If the check only auto-fixes files (for example trailing whitespace) with everything else passing, commit those fixes on the current branch in their own commit, as the Pre-Work Baseline Check's [P1](git.instructions.md#baseline-autofix-own-commit) does.
- **P4.** Only for an issue that meets the [stop condition](code-quality.instructions.md#pre-commit-stop-condition) in Fixing Pre-Commit Failures: comment on the issue/PR with the verbatim output and label it `Blocked`, as the Pre-Work Baseline Check's [P3](git.instructions.md#baseline-still-fails-escalate) does. Difficulty is not a reason to escalate.
- **P5.** No coverage re-baseline step is needed: the AI Coverage phase always reads `COVERAGE.md` live from `origin/main`, so a rebase alone cannot make it stale. If the rebase itself produces a conflict in `COVERAGE.md`, do not hand-merge the numbers; see [Committed Coverage File](coverage-ratchet.instructions.md#committed-coverage-file-mandatory) in [coverage-ratchet.instructions.md](coverage-ratchet.instructions.md).

## Force-Pushing a Rebased Branch (MANDATORY)

Rebasing a pushed branch rewrites its history, so pushing it needs a force-push. That is allowed only for a pure rebase that passes every check below, because a force-push to a shared branch, or over commits someone else pushed, destroys their work. This applies to every role that rebases a pushed branch, including the [Rebase Agent](agent-roles.instructions.md#rebase-agent).

- **P1.** Before rebasing, refuse `main`, `master`, `develop`, `release/*` and any other shared branch: only a personal feature or topic branch may be rebased and force-pushed.
- **P2.** Before rebasing, fetch the branch (`git -C <repodir> fetch origin <branch>`) and confirm that local HEAD equals remote HEAD: `git -C <repodir> rev-parse refs/heads/<branch>` must print the same SHA as `git -C <repodir> rev-parse refs/remotes/origin/<branch>`, because a difference means the local branch is not what was pushed, so the force-push would not be a pure rebase of the remote branch and could drop commits pushed from elsewhere.
- **P3.** Push with `git -C <repodir> push --force-with-lease origin <branch>` only. Never use plain `--force` (or `-f`), because only the lease refuses to overwrite commits pushed since the fetch.
- **P4.** If any check above fails, or the push is rejected, stop and ask the human, quoting the output verbatim as [Command Failure Reporting](git.instructions.md#command-failure-reporting-mandatory) requires. Do not retry the push and do not fall back to plain `--force`.

## Resolving Version Conflicts When Merging or Rebasing

When a merge or rebase produces conflicting versions of the same package, action, or runtime (both branches changed the version), resolve each conflicting entry individually; never take a whole file wholesale from one side.

This applies to every version-bearing file, including:

- Dependency manifests: `.csproj`, `Directory.Packages.props`, `packages.config`, `package.json`, `requirements.txt`
- GitHub Actions `uses:` version pins in workflows and composite actions
- Runtime and tool versions: .NET SDK (`global.json`), `dotnet-tools.json`, Node.js (`.nvmrc`, `engines`, `setup-node` versions), Python (`.python-version`, `setup-python` versions), and similar

Rules:

- **P1.** <a id="version-conflict-rules"></a>Take the **latest** of the candidate versions.
- **P2.** **Stable-over-pre-release exception**: if one candidate is a stable (release) version and the other is a pre-release (alpha/beta/rc/preview/dev build, etc.), take the stable candidate even if the pre-release has a nominally higher version number. Only take a pre-release if every candidate is a pre-release, in which case take the latest of them.
- **P3.** **Security exception**: if the latest candidate is known to be less secure than another candidate (e.g. it has a published security advisory that the other does not), take the most recent candidate that is not affected.
- **P4.** Never resolve by downgrading below every candidate, and never invent a version that appears on neither side.
- **P5.** Lock files (`package-lock.json` and similar): do not hand-merge; resolve the manifest first, then regenerate the lock file with the package manager.
- **P6.** After the merge or rebase completes, run the build and tests. If the chosen version broke the build (API changes, removed features), fix the breakage on the same branch as part of the merge work; do not downgrade to avoid the fix.

### No Confirmation Needed When the Algorithm Resolves the Conflict

The [rules above](#version-conflict-rules) (P1-P5) are a complete, deterministic algorithm: for every conflicting entry there is exactly one correct resolution (the latest candidate, the stable candidate, or the security-exception candidate). Apply it and continue; do not stop a merge or rebase to ask for confirmation on a conflict this algorithm resolves unambiguously, and do not post a PR/issue comment asking someone to confirm the choice.

Only stop and ask when a conflict genuinely falls outside the algorithm, for example:

- The same package is bumped to two different, unrelated versions on both sides and there is no clear "latest" (e.g. divergent major versions).
- A security trade-off with no candidate that is both latest and unaffected.
