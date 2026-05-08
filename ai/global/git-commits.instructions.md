# Git Commit Instructions

[Back to Global Instructions Index](index.md)

Load this file when about to commit or acting as the Committer agent. See [git.instructions.md](git.instructions.md) for the mandatory identity check and branch verification.

## Commit Rules

- **Never create an empty commit.** Verify `git diff --cached --name-only` lists at least one file before running `git commit`.
- Never amend an existing commit — always create a new one.
- Push to `origin` after every commit.

## Commit Message Format

- Use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) format.
- Include the user's original prompt verbatim in the commit body, prefixed with `Prompt:` followed by a space — not in the title.
