# Analyzer Conflict Instructions

[Back to Global Instructions Index](index.md)

## Conflicting Diagnostics (MANDATORY)

Two analyzers can disagree, so that fixing one diagnostic raises another. The table below records the pre-approved resolution for each known pair, so a pair that has already been decided is resolved the same way every time without asking again.

An entry in this table is the repo owner's explicit written permission for the suppression it names, for that diagnostic pair and that affected code only, in every repo that uses these global instructions (see [Warning Suppression and Errors](dotnet.instructions.md#warning-suppression-and-errors)). It grants nothing wider: any other suppression of either diagnostic still needs its own permission.

### Resolution Table

| Diagnostic A | Diagnostic B | Affected code | Resolution | Justification |
| --- | --- | --- | --- | --- |
| `IDE0028` (Collection initialization can be simplified) | `MA0002` (IEqualityComparer/IComparer is missing) | A collection constructed with an explicit `IEqualityComparer` or `IComparer` (for example `new Dictionary<string, int>(StringComparer.Ordinal)`) | Suppress `IDE0028` with `[SuppressMessage]` and the justification in the last column; keep the comparer | A collection expression cannot pass a comparer to the collection's constructor, so simplifying would silently drop it (for example `Ordinal`) and change lookup semantics |

The `IDE0028` entry is applied as:

```csharp
[SuppressMessage(
    category: "Style",
    checkId: "IDE0028: Collection initialization can be simplified",
    Justification = "A collection expression cannot pass a comparer to the collection's constructor, so simplifying would silently drop it (for example Ordinal) and change lookup semantics"
)]
```

### Unlisted Pairs

When two diagnostics conflict and the pair, for that kind of code, is not in the table:

- **P1.** Stop. Do not guess a resolution or suppress either diagnostic, because only the repo owner can grant permission to suppress.
- **P2.** Raise an issue in `credfeto/cs-template` describing the conflict: both diagnostic IDs, the affected code, and the candidate resolutions. Give it the content that [Template Rule Escalation](git.instructions.md#template-rule-escalation-non-template-repos-only) asks for, using the command in [git.examples.md](git.examples.md).
- **P3.** Add `Blocked` to the affected issue or PR as described in [Blocked Label](agent-roles.instructions.md#blocked-label) and wait for the repo owner's decision. Unlike Template Rule Escalation, work on the affected code does not carry on meanwhile, because the code cannot build while one of the two diagnostics stands.

### Adding an Entry

Once the repo owner has chosen a resolution, add the pair to the table, in the same PR or a follow-up, with every column filled in: both diagnostic IDs, the affected code, the resolution and the justification text. Every entry needs the repo owner's decision first, because an entry is itself the permission to suppress.
