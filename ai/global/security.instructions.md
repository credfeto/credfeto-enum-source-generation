# Security Instructions

> Always loaded.

[Back to Global Instructions Index](index.md)

## Secrets and Credentials

- Never commit secrets, credentials, API keys, tokens, or passwords.
- If accidentally committed, treat it as compromised immediately — rotate and purge from history.
- Use environment variables, secrets managers, or platform vaults for all runtime secrets.
- Refer to local AI instructions for the project-specific secrets management approach.

## Input Validation

- Validate all external input (user input, API requests, file contents, env vars, message queues) at the entry boundary before use.
- Reject input that does not conform to the expected shape, type, range, or format — never silently fix or guess malformed input.

## Output Sanitisation

- Sanitise all output containing external data for its context (HTML encoding, parameterised queries, shell escaping).
- Never construct queries, commands, or markup by string-concatenating untrusted input.

## Threat Modelling

- For any new feature handling data, exposing an endpoint, or introducing a trust boundary: model threats before writing code (who can call it, what can they send, what can go wrong). Document significant threats and mitigations.

## Dependency Vulnerability Scanning

- Scan dependencies for known vulnerabilities in CI; assess and resolve promptly.
- Refer to local AI instructions for the project-specific scanning tool.
