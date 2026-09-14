# Package Management Instructions

[Back to Global Instructions Index](index.md)

- Use only secure package versions; see [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning).
- When a merge or rebase produces conflicting versions of the same package, take the latest secure candidate and fix any resulting build issues; see [git-rebasing.instructions.md](git-rebasing.instructions.md#resolving-version-conflicts-when-merging-or-rebasing).
- In managed languages (.NET, JVM, Python), prefer managed libraries over native; only use native if it is the most actively maintained and stable option.
- Avoid deprecated or obsolete packages and language features; if unavoidable, add a comment explaining why and when it can be removed.
- Prefer the standard library; where insufficient, use well-known actively-maintained third-party libraries.
- If you find hand-rolled code duplicating standard-library or trusted-third-party functionality, raise a GitHub issue; do not modify it inline.

## Third-Party Packages Require Human Approval (MANDATORY)

Adding any package **not** published by `credfeto` or `funfair-tech` (i.e. not a `Credfeto.*`/`FunFair.*` package — see [dotnet-owned-packages.instructions.md](dotnet-owned-packages.instructions.md) — or the equivalent recognised first-party namespace in another ecosystem) is prohibited without explicit human approval. This applies regardless of how small, trivial, or transitive the package seems, and regardless of how urgently it's needed. Exception: a package change a pre-commit component tool's own output demands as the specific fix for its failure; see [Conflict Resolution](#conflict-resolution-pre-commitcomponent-tool-mandated-package-changes-mandatory) below.

Before requesting approval, carry out a full security review of the candidate package and version:

- **Provenance**: the source repository, publisher/maintainer identity, and that the registry listing genuinely matches the claimed upstream project (guard against typosquatting and dependency confusion).
- **Known vulnerabilities**: check the exact proposed version and its transitive dependencies for published advisories/CVEs; see [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning).
- **Maintenance health**: last release date, responsiveness to reported security issues, whether the project is archived, deprecated, or effectively unmaintained.
- **Licence**: confirm it's compatible with this project.
- **Footprint**: what it pulls in transitively, and whether that's proportionate to the problem being solved.

Then present the human with, and wait for their explicit sign-off before touching any manifest, lockfile, or import:

1. Package name, proposed version, and links to its source repository and registry listing.
2. The findings of the security review above.
3. Why it's needed — what it does that the standard library, an already-owned Credfeto/FunFair package, or an existing dependency cannot.
4. Alternatives considered and why they were rejected.

If working from a GitHub issue or PR, follow the [Blocked Label](agent-roles.instructions.md#blocked-label) workflow: post the review as a comment, add `Blocked`, and do not proceed until an explicit human approval comment exists and `Blocked` is removed. Otherwise, ask the human directly and wait for an unambiguous go-ahead (`approved` / `go ahead` / `looks good` / `lgtm`).

## Conflict Resolution: Pre-Commit/Component-Tool-Mandated Package Changes (MANDATORY)

Pre-commit and its component tools (see [Fixing Pre-Commit Failures](code-quality.instructions.md#fixing-pre-commit-failures-mandatory)) are configured by humans, so a tool-reported error demanding a specific package change is itself a human-authorised instruction, not a discretionary choice by the agent. When a tool's own output pins down the exact remediation, adding a package reference, changing an existing reference's metadata or version, or removing one, apply the fix and proceed without pausing for a fresh approval round-trip, even when it introduces a package not previously referenced anywhere in the repo.

This does not remove the security review, only the wait:

- Still carry out the full security review above for any package this newly introduces to the repo.
- If the review finds a genuine blocker (a known vulnerability advisory, a provenance/typosquat mismatch, an incompatible licence, or a maintenance status so poor the package cannot be trusted), this exception does not apply: fall back to the full approval-and-wait process above, since the tool's output cannot have authorised a fix its own security review flags as unsafe.
- If the review finds no blocker: post the findings for visibility and proceed with the fix and the current work without waiting for sign-off. If working from a GitHub issue or PR, post them as a normal comment; do not follow the [Blocked Label](agent-roles.instructions.md#blocked-label) workflow for this case. If not (no issue or PR), share them with the human directly (e.g. in chat).

This exception applies only when the tool's output pins down the exact remediation with no choice among alternatives left to the agent (for example, several packages could resolve the same advisory, or the fix could be a version bump or a package swap): any such choice remains a discretionary package decision, not a tool mandate, and the full approval-and-wait process above still applies.
