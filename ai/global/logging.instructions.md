# Logging Instructions

> Load when: writing code that produces log output.

[Back to Global Instructions Index](index.md)

## Rules

- Use structured logging — key-value pairs or structured objects, never concatenated strings.
- Log at service/system boundaries (requests, outgoing calls, significant state transitions).
- Log enough context to diagnose a problem without reproducing it — include relevant identifiers and state.
- Never log PII (names, emails, phone numbers, IP addresses, or anything that identifies an individual).
- Never log secrets, credentials, tokens, or passwords — see [security.instructions.md](security.instructions.md#secrets-and-credentials).
- Avoid logging large payloads in full — summarise or truncate.

## Log Levels

| Level | Use when |
| --- | --- |
| Error | Unexpected failure; operation could not complete |
| Warning | Unexpected but recoverable; may need investigation |
| Information | Significant business/operational events (service started, job completed) |
| Debug | Detailed diagnostics for development; disabled in production by default |
| Trace | Very fine-grained; never in production |
