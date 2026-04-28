# Package Management Instructions

[Back to Global Instructions Index](index.md)

## Security

- When adding packages, always use a secure version — try to avoid known security vulnerabilities.
- See [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning) for the dependency scanning requirements that apply to the build and CI pipeline.

## Managed vs Native Libraries

- In managed languages (e.g. .NET, JVM, Python), prefer managed libraries over native ones.
- Only use a native library if it is clearly the best available option — i.e. the most actively maintained and stable choice for the task.

## Deprecated or Obsolete Packages

- Avoid using obsolete or deprecated packages or language features.
- If they are necessary for a specific reason, add a comment explaining:
  - Why they are being used.
  - When they can be removed.
