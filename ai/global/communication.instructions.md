# Communication Instructions

> Always loaded.

[Back to Global Instructions Index](index.md)

This file covers how to raise anything that needs the human, so it is never buried in a routine status update. For when a session counts as interactive, see [Waiting for Approval in an Interactive Session](agent-roles.instructions.md#waiting-for-approval-in-an-interactive-session); for the `Blocked` label itself, see [Blocked Label](agent-roles.instructions.md#blocked-label).

## NEEDS YOU Block (MANDATORY)

In an interactive session, a question, decision, approval or manual step that needs the human is easy to miss in the middle or at the end of a status update, and the work then waits on an answer nobody knows has been asked. Raise it so it cannot be missed:

- **P1.** Raise anything that needs the human (a question, a decision, an approval or a manual step) as soon as it is identified, not at the end of the current batch of work.
- **P2.** Put it at the **top** of the reply, in a highlighted block whose first line is a marker for how urgent it is, followed by one bullet per item:

  ```markdown
  > 🛑 **NEEDS YOU:**
  > - **Q12.** <yes/no question>
  ```

- **P3.** Use exactly one of these markers, keeping the emoji and the bold label together on the line so the status still reads correctly without colour. Write each marker as the emoji character, never a GitHub shortcode, because a terminal shows a shortcode as plain text, while the character displays the same in a terminal and on GitHub. The markers are an output format the user has explicitly asked for in these instructions, so a general instruction to use emojis only when the user asks for them does not stop them; without them an agent falls back to a shortcode or plain text and the urgency marker is lost:
  - `🛑 **NEEDS YOU:**`: the work is stopped until the human answers.
  - `⚠️ **NEEDS YOU:**`: a question is open, but the agent can carry on with other work in the meantime.
  - `✅ **NEEDS YOU:** nothing right now.`: nothing is outstanding.
- **P4.** Keep the block at the top of every reply while anything is still outstanding, so it never scrolls out of view. Once nothing is outstanding, either show `> ✅ **NEEDS YOU:** nothing right now.` or leave the block out.
- **P5.** Number questions in the block as [Numbering and Cross-Reference Conventions](task-workflow.instructions.md#numbering-and-cross-reference-conventions-mandatory) describes: a `Q` prefix, continuing from the highest number already used in the work item as [Numbers Are Never Reused](task-workflow.instructions.md#numbers-are-never-reused) requires. Phrase each as a yes/no question where possible, so it can be answered in one word.
- **P6.** Before asking, gather what a command can show, as [Check Live State Before Asking or Reporting](agent-roles.instructions.md#check-live-state-before-asking-or-reporting-mandatory) requires; ask only for what no command can show.
- **P7.** Only the main session talks to the human. A role working from a hand-off puts anything that needs the human in its hand-off report, and the main session raises it in the block.

## Ask in Chat First (MANDATORY)

- **P1.** In an interactive session, ask the question in chat, in the NEEDS YOU block, rather than posting it on the issue or PR and adding `Blocked`, because the human is present and answers within minutes, and a `Blocked` label they have already answered is noise someone then has to clear.
- **P2.** Post the question on the issue or PR and add `Blocked`, with `cfwf issue block` or `cfwf pr block` as [Blocked Label](agent-roles.instructions.md#blocked-label) describes, only when it is still unanswered when the session would otherwise stop, so the question survives the session.
- **P3.** An answer given in chat is still recorded on GitHub: mirror it as a comment as [Blocked Label](agent-roles.instructions.md#blocked-label) P4 describes, and record the prompt and its outcome as [Prompt Traceability](task-workflow.instructions.md#prompt-traceability-mandatory) describes, so the record survives even if the chat session is lost.
- **P4.** An unattended run is unchanged: post the question on the issue or PR and add `Blocked` straight away, because no human is watching the chat.
- **P5.** Plan approval is unchanged: [Issue Workflow: Plan First](agent-roles.instructions.md#issue-workflow-plan-first-new-issues-only) still posts the plan, adds `Blocked` and moves the board to **Planning**, because the plan is the record approval is judged against. In an interactive session, also raise the pending approval and any open `Q` questions in the NEEDS YOU block; a live-chat approval clears that `Blocked` as [Waiting for Approval in an Interactive Session](agent-roles.instructions.md#waiting-for-approval-in-an-interactive-session) P5 describes.
