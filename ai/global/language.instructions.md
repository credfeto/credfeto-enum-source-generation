# Language Instructions

[Back to Global Instructions Index](index.md)

## English Variant

- All documentation, comments, and commit messages must be written in **UK English**.
- This includes prose in `README.md`, `CHANGELOG.md`, inline code comments, and AI instruction files.

## Code Identifiers

- Variable names, function names, class names, and other code identifiers should follow the **platform or library convention**, which is typically US English — do not force UK spellings onto them.
- For example: use `Color` not `Colour` when the platform or library defines it as `Color`; mixing conventions produces confusing and inconsistent code (e.g. `Color colour = Color.Red` is actively harmful).
- Where a library or platform uses a non-English language for its identifiers, use **English** for any variables or identifiers that interact with it.
