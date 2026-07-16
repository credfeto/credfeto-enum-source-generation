# Rootless Podman Under an Unprivileged Systemd Service Instructions

> Load when: configuring or debugging rootless podman run by a **system** systemd unit (`/etc/systemd/system/*.service`, `WantedBy=multi-user.target`) under an unprivileged `User=` account with **no real login session** — not a `systemctl --user` unit, not an interactively logged-in account. This is the shape you get when "run this as an unprivileged systemd service, not a user service" is the explicit requirement.

[Back to Global Instructions Index](index.md)

## D-Bus Session Bus Required for Networking (MANDATORY)

- Rootless podman's default network backend (pasta + netavark/aardvark-dns) registers itself with a D-Bus session bus at `/run/user/<uid>/bus`. That bus only exists once `systemd-logind` has started that user's `systemd --user` manager, which does not happen automatically for an account with no login session.
- Symptom in `journalctl` for the container-runner unit:

  ```text
  failed to move the rootless netns pasta process to the systemd user.slice:
  dbus: couldn't determine address of session bus
  ...
  aardvark-dns failed to start
  ```

- Fix: `loginctl enable-linger <user>` starts that user's manager (and its bus) at boot without requiring a login, independent of whether the container-runner unit stays a system unit. Point `XDG_RUNTIME_DIR` at the real `/run/user/<uid>` it provisions.
- Do **not** invent a bespoke `RuntimeDirectory=<user>` (`/run/<user>`) for this instead; it has no bus socket behind it and fails the same way.

## `%U` Is Not Reliable for the Service Account's UID (MANDATORY)

- `%U` in a systemd unit resolves to the **service manager's** UID (root, for a system unit), not the UID of the account in `User=`.
- Symptom: `Environment=XDG_RUNTIME_DIR=/run/user/%U` produces `Failed to obtain podman configuration: lstat /run/user/0: no such file or directory`.
- Fix: do not trust `%U` for this; compute the UID at runtime inside the script instead, which is always correct for whoever the process is actually running as, regardless of that specifier's behaviour on a given systemd version:

  ```sh
  : "${XDG_RUNTIME_DIR:=/run/user/$(id -u)}"
  export XDG_RUNTIME_DIR
  ```

- Shell-level `runuser -u <user> -- env XDG_RUNTIME_DIR=/run/user/$(id -u <user>) ...` invocations (e.g. from an install script) are **not** affected by this; only the systemd unit's own specifier expansion is.

## Cgroup Manager Must Be `cgroupfs`, Not `systemd` (MANDATORY)

- With lingering enabled, podman defaults to `--cgroup-manager=systemd` and asks the target user's own `systemd --user` instance (over its D-Bus session bus) to create a transient scope unit per container. A **system** unit's process reaching into that user session from *outside* it (not itself a descendant of that user session) gets this request rejected.
- Symptom:

  ```text
  runc create failed: unable to start container process: unable to apply
  cgroup configuration: unable to start unit "libpod-<id>.scope" (...):
  Permission denied: OCI permission denied
  ```

- Fix: force the `cgroupfs` cgroup manager so podman never needs to talk to the user session's systemd for cgroups at all:

  ```toml
  # ~<user>/.config/containers/containers.conf
  [engine]
  cgroup_manager = "cgroupfs"
  ```

- Trade-off (accepted for this topology): loses per-container `systemd-cgtop`/`systemctl` visibility into individual containers' resource usage.

## `KillMode=none` on the Container-Runner Unit (MANDATORY)

- Rootless podman is daemonless: `conmon`/container processes are not systemd-managed children the way `dockerd`-owned containers are. The default `KillMode=control-group` sends a cgroup-wide kill to running containers on the stop half of any `systemctl restart`.
- Fix: set `KillMode=none` on the container-runner unit. Confirmed on a real host that this avoids killing containers on `systemctl restart` — container IDs/uptimes survived a manual restart untouched, since `podman compose up -d` is idempotent and only recreates what actually changed.

## `ProtectHome=yes` Breaks `XDG_RUNTIME_DIR` (MANDATORY)

- `ProtectHome=yes` also masks `/run/user/*` on the unit, which breaks the D-Bus session bus fix above.
- Fix: use `ProtectSystem=strict` + `PrivateTmp=yes` + `NoNewPrivileges=yes` instead (all three confirmed compatible with rootless podman on a real host); drop `ProtectHome`.

## Renaming a Systemd Timer During a Migration Leaves the Old One Active (MANDATORY)

- A `.timer` unit defaults to targeting the identically-named `.service` when no `Unit=` override is given. If a migration renames `foo.timer`/`foo.service` to `foo-update.timer`/`foo-update.service` and also introduces a *new*, unrelated `foo.service`, any host upgrading in place still has the **old** `foo.timer` on disk, now firing against the **new** `foo.service` on its old schedule — a silent collision that only turns up as an unexpected `TriggeredBy:` in `systemctl status`.
- Fix: whenever a systemd unit is being renamed as part of a migration, have the install/upgrade script explicitly detect and remove the old timer unit file (`systemctl disable --now`, then `rm`) before installing the new units. Do not rely on copying the new files over being sufficient.

## Bind-Mounted Secrets Need `other`-Read, Not Just `group`-Read (MANDATORY)

- Rootless podman checks a bind-mounted host file's permissions against the **host-mapped UID** of whatever UID the containerized process claims to be internally, via the service account's subuid range (e.g. `200000-265535`). Tightening a secrets file from world-readable (`644`) to owner+group-only (`640`) can silently break a container whose process runs as some non-root UID inside its image (common for e.g. Node-based images), which maps to a host UID that is neither the service account nor its group.
- Symptom is unusually hard to diagnose: the container crash-loops (`podman compose ps` showing "Up Less than a second" despite being created many minutes earlier) with **zero output** from `podman logs`, since the crash happens before anything is logged.
- A sibling file bind-mounted with the same `640` permissions into a *different* container working fine is not evidence the general problem doesn't apply; that is almost certainly a coincidence of whatever UID that particular container's process happens to map to. Do not treat one working case as proof `640` is safe for bind-mounted secrets under rootless podman in general.
- Fix/trade-off: bind-mounted secrets under rootless podman generally need to stay world-readable (`644`) unless the image's internal UID is specifically known and kept in sync, which is fragile and breaks silently if the image changes it. This is a genuine trade-off (any local user on the host can read the file), not a free hardening win the way it would be for a normal host-only file.

## Firewalld: `docker` Zone Can Pre-Empt Other Zones (MANDATORY)

- A host that ever ran docker can have a standing firewalld `docker` zone with `target: ACCEPT` and a broad `sources:` range (e.g. `172.16.0.0/12`, docker's default bridge supernet). firewalld resolves zones **by source address before interface**, so any traffic whose source IP falls in that range gets blanket-accepted by the `docker` zone regardless of destination port, silently bypassing whatever port-specific rules exist in `public` (or any other zone) for that same traffic. `docker0` being link-down/unused does not disable this; the zone config persists independently of whether docker itself is still running anything.
- Check `firewall-cmd --get-active-zones` (not just `--list-all` on the zone you assume matters) before concluding a specific port-open rule is or isn't the cause of connectivity behaviour on a host that has ever run docker.

## Docker Bypasses Firewalld for Published Ports; Podman's `pasta` Does Not (MANDATORY)

- Docker manipulates iptables directly for published ports, which commonly bypasses firewalld's normal zone/port filtering entirely for that traffic — a port can "just work" over the LAN under docker with **zero explicit firewalld rule** for it. Rootless podman's `pasta` network backend does not do that same iptables insertion trick.
- Once a service migrates from docker to podman, ports that previously worked via this implicit bypass need an **explicit** firewalld rule or they will start silently failing (connection refused/timeout, e.g. a reverse proxy on another host returning 502) even though nothing about the port publishing itself changed.

## Prune Dangling Images After Every Pull (MANDATORY)

- A service that repeatedly pulls the same `:latest` tag (e.g. on a timer) leaves the previous image dangling (untagged) each time a new one lands and containers are recreated to use it; nothing reclaims that disk space on its own, and it accumulates indefinitely on a long-running host.
- Run `podman image prune --force` (or `docker image prune -f`) after `compose up -d` on every such cycle; `image prune` only removes images no container references, so anything still in use by a running container is left alone regardless of when in the sequence it runs.
- Make it non-fatal (`|| true`): by the time pruning runs, the containers are already up — a transient prune failure (empty store, a lock, etc.) must never fail the whole deploy/update run over what is purely best-effort disk cleanup.

## `podman-compose` Has No Standalone `rm` Subcommand

Unlike `docker compose`, `podman-compose` (confirmed on v1.6.0) does not support `rm` as a standalone subcommand:

```text
podman-compose: error: argument command: invalid choice: 'rm'
(choose from 'help', 'version', 'wait', 'systemd', 'pull', 'push', 'build',
'up', 'down', 'ps', 'run', 'exec', 'start', 'stop', 'restart', 'logs',
'config', 'port', 'pause', 'unpause', 'kill', 'stats', 'images')
```

Use `podman compose down` instead (stop + remove in one step) rather than a `stop` + `rm -f` pair.

## Source

Findings captured from real-host debugging during the `credfeto-notification-bot-docker` docker-to-rootless-podman migration; see [credfeto/cs-template#978](https://github.com/credfeto/cs-template/issues/978) and credfeto/credfeto-notification-bot-docker#12 / credfeto/credfeto-notification-bot-docker#14 for the full narrative and exact commands used to reproduce/diagnose each one.
