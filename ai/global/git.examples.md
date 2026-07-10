# Git Examples

[Back to Git Instructions](git.instructions.md) | [Back to Global Instructions Index](index.md)

## Template Rule Escalation — Issue Command

```bash
gh issue create --repo credfeto/cs-template \
  --title "<short description of the rule change>" \
  --label "AI-Work" \
  --body "**Source repository**: <repo where need was discovered>

**Current behaviour / gap**: <what is missing or inconsistent>

**Proposed rule text**: <concrete rule update or new instruction text>

**Reason for template propagation**: <why this should apply across all repos>"
```
