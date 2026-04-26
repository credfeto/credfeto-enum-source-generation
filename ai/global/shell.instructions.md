# Shell Script Instructions

[Back to Global Instructions Index](index.md)

## Firewall Rules (`firewall-cmd`)

### Private Network Constants

Define these constants at the top of any script that manages firewall rules:

```bash
IPV4_PRIVATE_RANGES=(
    "10.0.0.0/8"
    "172.16.0.0/12"
    "192.168.0.0/16"
)

IPV6_PRIVATE_RANGES=(
    "fc00::/7"
    "fe80::/10"
)
```

### Helper Functions

Include these helpers in any script that opens ports:

```bash
allow_ipv4() {
    local subnet="$1"
    local port="$2"
    local protocol="$3"
    firewall-cmd --permanent \
        --add-rich-rule="rule family='ipv4' source address='${subnet}' port port='${port}' protocol='${protocol}' accept"
}

allow_ipv6() {
    local subnet="$1"
    local port="$2"
    local protocol="$3"
    firewall-cmd --permanent \
        --add-rich-rule="rule family='ipv6' source address='${subnet}' port port='${port}' protocol='${protocol}' accept"
}

open_port_for_private_networks() {
    local port="$1"
    local protocol="${2:-tcp}"

    for subnet in "${IPV4_PRIVATE_RANGES[@]}"; do
        allow_ipv4 "${subnet}" "${port}" "${protocol}"
    done

    for subnet in "${IPV6_PRIVATE_RANGES[@]}"; do
        allow_ipv6 "${subnet}" "${port}" "${protocol}"
    done

    firewall-cmd --reload
}
```

### Usage

To allow a port from all private networks (both IPv4 and IPv6):

```bash
open_port_for_private_networks 8080 tcp
open_port_for_private_networks 53 udp
```

To allow a single specific subnet:

```bash
allow_ipv4 "192.168.0.0/16" 1234 tcp
allow_ipv6 "fc00::/7" 1234 tcp
firewall-cmd --reload
```

### Rules

- Always use `--permanent` so rules survive reboots.
- Always call `firewall-cmd --reload` after adding rules — call it once after all rules for a given operation are added, not once per rule.
- Never open ports to `0.0.0.0/0` or `::/0` without an explicit security review.
- Use `open_port_for_private_networks` as the default when a service should be reachable from LAN/VPN; only open to the internet if the service is intentionally public-facing.
