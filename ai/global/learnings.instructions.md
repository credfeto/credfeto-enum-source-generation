# Learnings Instructions

[Back to Global Instructions Index](index.md)

Load this file whenever a memory file (`ai/global` or `ai/local`) is created or updated to record something learned during work.

## Learning Capture (Human Notes)

MANDATORY: Memory files are AI-rule text — terse, procedural, written for a model to follow. They are not written for people. Whenever one is created or updated to record something learned, also file a human-readable issue in `credfeto/credfeto-notes` summarising it for people. Never copy rule wording verbatim into that issue.

1. Write the issue in plain prose: what was learned, why it mattered, and what prompted it.
2. Explain it as you would to a person unfamiliar with the AI rule files; do not restate instruction-file rule syntax.
3. Use `gh issue create --repo credfeto/credfeto-notes` with the `AI-Work` label; see [learnings.examples.md](learnings.examples.md) for the command template.
4. Note the resulting issue URL in the commit message that updates the memory file.
5. This is additional to, not a replacement for, the memory-file update itself (see the Management Rules in `.ai-instructions`).
