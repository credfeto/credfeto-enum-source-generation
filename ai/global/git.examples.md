# Git Examples

[Back to Git Instructions](git.instructions.md) | [Back to Global Instructions Index](index.md)

## Git Identity Check Script

Run before any commit to verify identity and GPG signing:

```bash
#!/bin/sh
# Checks that git is configured with a valid identity and GPG signing.

die() {
    printf '\n\033[31m✗\033[0m %s\n' "$*"
    exit 1
}

info() {
    printf '\n\033[32m→\033[0m %s\n' "$*"
}

CURRENT_EMAIL=$(git config user.email)

[ -z "$CURRENT_EMAIL" ] && die "git user.email is not set — run: git config --global user.email \"you@example.com\""

[ "$CURRENT_EMAIL" = "andy@nanoclaw.ai" ] && die "git is configured with the wrong identity (${CURRENT_EMAIL}) — aborting"

[ "$(git config commit.gpgsign)" = "true" ] || die "GPG signing is not enabled — run: git config --global commit.gpgsign true"

command -v gpg > /dev/null 2>&1 || die "gpg is not installed — cannot verify signing key"

gpg --list-secret-keys "$CURRENT_EMAIL" > /dev/null 2>&1 \
    || die "no GPG secret key found for ${CURRENT_EMAIL} — run: gpg --gen-key"

SIGNING_KEY=$(git config user.signingkey)
[ -z "$SIGNING_KEY" ] && die "git user.signingkey is not set — run: git config --global user.signingkey <keyid>"

gpg --list-secret-keys "$SIGNING_KEY" > /dev/null 2>&1 \
    || die "git user.signingkey (${SIGNING_KEY}) not found in GPG keyring"

gpg --list-secret-keys "$SIGNING_KEY" 2>/dev/null | grep -qF "$CURRENT_EMAIL" \
    || die "git user.signingkey (${SIGNING_KEY}) is not associated with ${CURRENT_EMAIL}"

info "git identity check passed (${CURRENT_EMAIL}, key ${SIGNING_KEY})"
```

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
