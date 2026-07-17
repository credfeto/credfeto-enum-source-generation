# Package Management Instructions

[Back to Global Instructions Index](index.md)

- Use only secure package versions; see [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning).
- When a merge or rebase produces conflicting versions of the same package, take the latest secure candidate and fix any resulting build issues; see [git-rebasing.instructions.md](git-rebasing.instructions.md#resolving-version-conflicts-when-merging-or-rebasing).
- In managed languages (.NET, JVM, Python), prefer managed libraries over native; only use native if it is the most actively maintained and stable option.
- Avoid deprecated or obsolete packages and language features; if unavoidable, add a comment explaining why and when it can be removed.
- Prefer the standard library; where insufficient, use well-known actively-maintained third-party libraries.
- If you find hand-rolled code duplicating standard-library or trusted-third-party functionality, raise a GitHub issue; do not modify it inline.

## Third-Party Packages Require Human Approval (MANDATORY)

Adding any package **not** published by `credfeto` or `funfair-tech` (i.e. not a `Credfeto.*`/`FunFair.*` package — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) — or the equivalent recognised first-party namespace in another ecosystem) is prohibited without explicit human approval. This applies regardless of how small, trivial, or transitive the package seems, and regardless of how urgently it's needed.

Before requesting approval, carry out a full security review of the candidate package and version:

- **Provenance**: the source repository, publisher/maintainer identity, and that the registry listing genuinely matches the claimed upstream project (guard against typosquatting and dependency confusion).
- **Known vulnerabilities**: check the exact proposed version and its transitive dependencies for published advisories/CVEs; see [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning).
- **Maintenance health**: last release date, responsiveness to reported security issues, whether the project is archived, deprecated, or effectively unmaintained.
- **License**: confirm it's compatible with this project.
- **Footprint**: what it pulls in transitively, and whether that's proportionate to the problem being solved.

Then present the human with, and wait for their explicit sign-off before touching any manifest, lockfile, or import:

1. Package name, proposed version, and links to its source repository and registry listing.
2. The findings of the security review above.
3. Why it's needed — what it does that the standard library, an already-owned Credfeto/FunFair package, or an existing dependency cannot.
4. Alternatives considered and why they were rejected.

If working from a GitHub issue or PR, follow the [Blocked Label](agent-roles.instructions.md#blocked-label) workflow: post the review as a comment, add `Blocked`, and do not proceed until an explicit human approval comment exists and `Blocked` is removed. Otherwise, ask the human directly and wait for an unambiguous go-ahead (`approved` / `go ahead` / `looks good` / `lgtm`).
