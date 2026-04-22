# Error Handling Instructions

[Back to Global Instructions Index](index.md)

## General Principles

- All errors must be handled explicitly — never swallow exceptions or silently ignore error states.
- Unhandled errors must not be allowed to crash a service or leave the system in an inconsistent state.

## Catching Errors

- Only catch errors you can meaningfully handle at that point — do not catch broadly just to suppress errors.
- If an error cannot be handled locally, let it propagate to a level that can handle it, or wrap it in a more contextual error and rethrow.
- Never catch an error, log it, and then rethrow it — this leads to duplicate log entries; either handle it or rethrow it.

## Propagation

- Errors should carry enough context to be diagnosable — include relevant identifiers, inputs, or state when wrapping errors.
- Do not lose the original cause when wrapping errors; always chain or preserve the root cause.

## Surfacing

- Errors that are exposed to external callers (e.g. API responses) must be sanitised — do not leak internal implementation details, stack traces, or sensitive data in error responses.
- Use appropriate error codes or status indicators for the protocol in use (e.g. HTTP status codes for REST APIs).
