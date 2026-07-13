# Docker Instructions

> Load when: any `Dockerfile`, `Containerfile`, `docker-compose*.yml`, `docker-compose*.yaml`, `compose.yml`, `compose.yaml`, or `.dockerignore` is present, or container work is needed.

[Back to Global Instructions Index](index.md)

## Container Runner Detection

- The container runner on a given machine may be **Docker or Podman**, never assume one is installed. If neither is installed, stop immediately, do not attempt to install one yourself, and ask the user to install Docker or Podman before continuing.
- Detect which is available and use that: prefer `docker` if present, otherwise fall back to `podman`. Check with `command -v docker` / `command -v podman` before running any container command.
- Do the same for the compose tool: `docker compose` and `podman compose` are subcommands of their respective CLIs, not standalone binaries, so detect them by running e.g. `docker compose version` / `podman compose version` rather than `command -v`; fall back to the legacy standalone `docker-compose` / `podman-compose` binaries only if the subcommand isn't available.
- Do not hardcode `docker` in scripts that need to run on either; resolve the runner once at the top of the script and use a variable.

## Dockerfile Authoring

- Prefer building the app outside the container (locally or in CI) and copying the pre-built artefacts into a minimal runtime image, rather than compiling inside the container. When the build must run inside the container, use multi-stage builds: a `build`/`sdk` stage for compiling/installing, and a minimal runtime stage that copies only the built artefacts across.
- Pin base image versions explicitly, never `latest`, unless explicitly required by local instructions. Prefer pinning by digest (e.g. `alpine:3.20@sha256:<digest>`) when the repo's `.github/dependabot.yml` has the `docker` ecosystem configured: Dependabot updates the digest (and the trailing tag) the same way it updates a tag-only pin, so digest pinning does not lose auto-update coverage. Fall back to a tag alone (e.g. `alpine:3.20`) when the `docker` ecosystem isn't configured. As with GitHub Actions pinning (see [github-workflows.instructions.md](github-workflows.instructions.md#version-pinning)), this is opportunistic: switch a pin to a digest when already touching that line, not a mandate to mass-migrate every Dockerfile. The version/digest shown is an example only and may be stale by the time you read this; check the image registry for the current stable release before pinning.
- Order layers from least to most frequently changing so the build cache is preserved (dependency manifests and restore steps before source copy).
- Run the container process as a non-root user; create a dedicated user in the image rather than running as `root`.
- Add a `.dockerignore` covering build output, `.git`, local secrets/env files, and IDE directories; never rely on the daemon to filter these out of the build context. This applies even when copying in pre-built artefacts rather than building inside the container: the whole build context is still sent to the daemon, not just the files referenced by `COPY`.
- Never `COPY`/`ADD` secrets, credentials, or `.env` files into an image layer: even if removed in a later layer, they remain in the image history. Pass secrets at build time via BuildKit `--secret` (or the Podman equivalent) or inject them at runtime.

## Compose Conventions

- Pin image tags/digests in `docker-compose.yml` / `compose.yml` the same way as Dockerfiles: no `latest`, unless explicitly required by local instructions.
- Define explicit named networks rather than relying on the default network when more than one service needs to communicate.
- Use external named volumes for persistent state; bind-mount only for local development conveniences (source code, config overrides).
- Keep environment-specific overrides in a separate override file (e.g. `compose.override.yml`) rather than branching logic inside the base file.
- Add `install` and `update` scripts to the repo: `install` performs first-time setup, creating any external volumes/networks, pulling or building images, and starting the containers; `update` pulls or rebuilds the latest images and restarts the containers without touching persistent volumes.
- A `reset` script should also be provided that tears down the existing containers and then calls `install`.

## Security Basics

- Prefer minimal base images (`-alpine`, `-slim`, or distroless) for runtime stages to reduce attack surface.
- Scan images for known vulnerabilities before publishing (e.g. `docker scout`, `trivy`, or the CI-configured scanner); see [security.instructions.md](security.instructions.md#dependency-vulnerability-scanning) for the general dependency-scanning policy.
- Never bake secrets, credentials, or tokens into an image layer; see the Dockerfile Authoring section above.
- Set explicit resource limits (memory/CPU) in compose/runtime configuration for anything other than local development; confirm the runner actually enforces them (e.g. `deploy.resources.limits` is a Swarm construct that some Compose V2 versions ignore outside Swarm mode) rather than assuming the field alone provides protection.
- Run containers under least privilege: remove `sudo`/`doas` and other setuid privilege-escalation binaries from the final image, run as a non-root user, drop all capabilities by default and add back only the specific ones a service genuinely needs (e.g. `cap_drop: [ALL]` plus an explicit `cap_add` allowlist), and set `security_opt: [no-new-privileges:true]` (or the Podman equivalent) so the container can never acquire additional privileges at runtime.
- Ensure that containers are defined with read-only filesystems (`read_only: true`) to prevent accidental or malicious modifications.
