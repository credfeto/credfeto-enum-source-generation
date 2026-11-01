# Learnings Instructions

[Back to Global Instructions Index](index.md)

Load this file whenever a memory file (`ai/global` or `ai/local`) is created or updated to record something learned during work.

## Learning Capture (Human Notes)

MANDATORY: Memory files are AI-rule text: terse, procedural, written for a model to follow. They are not written for people. Whenever one is created or updated to record something learned, also record a human-readable note summarising it for people in the knowledge-base MCP (the `kb_orchestrator` server). Never copy rule wording verbatim into that note.

- **P1.** Write the note in plain prose: what was learned, why it mattered, and what prompted it.
- **P2.** Explain it as you would to a person unfamiliar with the AI rule files; do not restate instruction-file rule syntax.
- **P3.** Add it with the knowledge-base MCP server's tool for adding a note, titled `learning: <description>`, after searching the knowledge base for an existing note on the same thing and updating that one instead if there is one; see [learnings.examples.md](learnings.examples.md) for the note template. If the server is not connected, put the note text in your hand-off or final report instead, so it is not lost.
- **P4.** Note the note's title in the commit message that updates the memory file.
- **P5.** This is additional to, not a replacement for, the memory-file update itself (see the Management Rules in `.ai-instructions`).
