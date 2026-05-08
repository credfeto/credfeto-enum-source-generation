# Firewall Script Examples

[Back to Global Instructions Index](index.md)

Reference implementations for the firewall helpers described in [shell.firewall.instructions.md](shell.firewall.instructions.md). Load this file when actively writing or modifying firewall scripts.

## Private Network Constants

Define these at the top of any script that manages firewall rules:

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

## Helper Functions

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

## Usage

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
