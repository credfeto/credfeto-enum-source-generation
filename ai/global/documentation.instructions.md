# Documentation Instructions

[Back to Global Instructions Index](index.md)

## README

- Keep `README.md` accurate and up to date; rewrite template placeholder content before starting work.
- Update the configuration section in the same commit whenever a config option (`appsettings.json`, options class, or env var) is added, removed, or renamed; include type, default, and description.
- For changelog format and tooling see [changelog.instructions.md](changelog.instructions.md).

### Required Sections (in this order)

1. **Title and one-line description**: state what the project does, not just what it is named.
2. **Badges**: see [Badge Guidelines](#badge-guidelines) below.
3. **Overview**: 2–3 sentences explaining the purpose and key features.
4. **Quick Start / Simple Example**: the minimal copy-paste snippet that gets something working; no preamble.
5. **Installation**: e.g. `dotnet add package <Name>` or the NuGet Package Manager command.
6. **Usage / Examples**: one or two brief, self-contained examples; link to `docs/` for more complex ones.
7. **Documentation**: link to the `docs/` folder and any generated API documentation (e.g. DocFX, Doxygen, GitHub Pages).
8. **Changelog**: link to `CHANGELOG.md`.
9. **Contributing**: link to `CONTRIBUTING.md`.
10. **Security**: link to `SECURITY.md`.
11. **Licence**: link to `LICENSE`.
12. **Contributors**: all-contributors section; see the [Contributors section](#contributors) below for when to include it.
13. **Reference links**: all image and URL references used by badges and links, collected at the very bottom of the file, sorted alphabetically by label.

Omit any section that does not apply (e.g. no Installation section for a library with no NuGet package), but never invent placeholder sections.

### Badge Guidelines

Place all badges near the top of the file, after the title but before the overview prose.

#### Link Format (MANDATORY)

All badges and hyperlinks must use **reference-style** markdown, never inline URLs. Use descriptive named labels; never use bare numbers (`[1]`, `[2]`). Collect all reference definitions at the very bottom of the file, sorted alphabetically by label.

```markdown
[![Alt text][image-ref]][link-ref]

<!-- bottom of file -->
[image-ref]: https://example.com/badge.svg
[link-ref]: https://example.com/
```

#### CI / Build Status

Use a table to separate `main`/pre-release from `release` builds:

```markdown
| Branch  | Status                                            |
|---------|---------------------------------------------------|
| main    | [![Build: Pre-Release][pre-release-img]][pre-release] |
| release | [![Build: Release][release-img]][release]         |

<!-- bottom of file -->
[pre-release-img]: https://github.com/OWNER/REPO/actions/workflows/build-and-publish-pre-release.yml/badge.svg
[pre-release]: https://github.com/OWNER/REPO/actions/workflows/build-and-publish-pre-release.yml
[release-img]: https://github.com/OWNER/REPO/actions/workflows/build-and-publish-release.yml/badge.svg
[release]: https://github.com/OWNER/REPO/actions/workflows/build-and-publish-release.yml
```

#### NuGet (when the project publishes a package)

```markdown
[![NuGet][nuget-img]][nuget]
[![NuGet Downloads][nuget-downloads-img]][nuget]

<!-- bottom of file -->
[nuget-img]: https://img.shields.io/nuget/v/PACKAGE_NAME.svg
[nuget-downloads-img]: https://img.shields.io/nuget/dt/PACKAGE_NAME.svg
[nuget]: https://www.nuget.org/packages/PACKAGE_NAME
```

#### Code Coverage

```markdown
[![codecov][codecov-img]][codecov]

<!-- bottom of file -->
[codecov-img]: https://codecov.io/gh/OWNER/REPO/branch/main/graph/badge.svg
[codecov]: https://codecov.io/gh/OWNER/REPO
```

#### Licence

```markdown
[![Licence][licence-img]][licence]

<!-- bottom of file -->
[licence-img]: https://img.shields.io/github/license/OWNER/REPO
[licence]: LICENSE
```

#### Open Issues / Bugs

```markdown
[![Bugs][bugs-img]][bugs]

<!-- bottom of file -->
[bugs-img]: https://img.shields.io/github/issues/OWNER/REPO/bug
[bugs]: https://github.com/OWNER/REPO/issues?q=is%3Aopen+is%3Aissue+label%3Abug
```

Only include badges that are actively maintained and reflect real state. Remove any badge whose target service is unavailable or whose data is stale.

## Additional Documentation

- All non-template repos must have a `docs/` folder; template repos must not.
- Place all docs and architecture diagrams (except `README.md`/`CHANGELOG.md`) in `docs/`; keep them current.
- Architecture diagrams: show folder structure only, omit individual files and package versions; prefer SVG over raster formats.

## Contributors

- Include a `## Contributors` section in `README.md` using the [all-contributors](https://allcontributors.org) spec **only when the repository has more than one known human contributor**. Omit the section entirely when there is only one.
- When the condition is met, preserve the auto-generated comment markers exactly; do not reformat, reorder, or remove them:

```markdown
## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
```

- Never manually edit the content between the `START` and `END` markers; the all-contributors bot manages that block.
