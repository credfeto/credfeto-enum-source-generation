# Shell Firewall Instructions

[Back to Global Instructions Index](index.md)

## Firewall Rules (`firewall-cmd`)

Use three standard helpers for all firewall rule management. See [shell.firewall.examples.md](shell.firewall.examples.md) for implementations of `allow_ipv4`, `allow_ipv6`, and `open_port_for_private_networks`, including the `IPV4_PRIVATE_RANGES` and `IPV6_PRIVATE_RANGES` constants.

### Rules

- Always use `--permanent` so rules survive reboots.
- Always call `firewall-cmd --reload` after adding rules — call it once after all rules for a given operation are added, not once per rule.
- Never open ports to `0.0.0.0/0` or `::/0` without an explicit security review.
- Use `open_port_for_private_networks` as the default when a service should be reachable from LAN/VPN; only open to the internet if the service is intentionally public-facing.
