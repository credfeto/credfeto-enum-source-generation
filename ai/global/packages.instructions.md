# Package Management Instructions

[Back to Global Instructions Index](index.md)

- Use only secure package versions; see [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning).
- When a merge or rebase produces conflicting versions of the same package, take the latest secure candidate and fix any resulting build issues; see [git.instructions.md](git.instructions.md#resolving-version-conflicts-when-merging-or-rebasing).
- In managed languages (.NET, JVM, Python), prefer managed libraries over native; only use native if it is the most actively maintained and stable option.
- Avoid deprecated or obsolete packages and language features; if unavoidable, add a comment explaining why and when it can be removed.
- Prefer the standard library; where insufficient, use well-known actively-maintained third-party libraries.
- If you find hand-rolled code duplicating standard-library or trusted-third-party functionality, raise a GitHub issue; do not modify it inline.
