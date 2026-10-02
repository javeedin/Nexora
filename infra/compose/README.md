# Local dev stack

One command starts every backing service Nexora needs, waits until each is healthy, seeds it and prints where to find it:

```sh
make up          # or: pnpm stack:up   (Windows without make)
make smoke       # end-to-end round trip through every service
make check       # quick status + URLs
make logs s=keycloak
make down        # stop, keep data
make reset       # stop and delete all data
```

Needs Docker (Desktop) with ~8 GB RAM and Node 22. `make smoke` also needs [uv](https://docs.astral.sh/uv/).
First start pulls ~5 GB of images; later starts take under a minute.

| Service | URL / address | Notes |
|---|---|---|
| Oracle 23ai Free | `localhost:1521/FREEPDB1` | `nexora_migrator`; module schemas (`platform` …) are password-less, see `db/migrations` |
| Keycloak | http://localhost:8180 | realm `nexora`; organisations `acme`, `globex` = demo tenants |
| Temporal | gRPC `localhost:7233`, UI http://localhost:8233 | namespace `nexora` |
| Redpanda (Kafka API) | `localhost:19092`, schema registry http://localhost:18081 | console http://localhost:8088 |
| Redis | `localhost:6379` | password protected |
| S3 (SeaweedFS) | http://localhost:8333 | bucket `nexora-files`, keys `tenants/<tenant_id>/…` |
| Vault (dev mode) | http://localhost:8200 | transit key per tenant `tenant-<id>`, policy `nexora-api` |
| Grafana + Tempo + Loki + Prometheus | http://localhost:3000 | OTLP `localhost:4317` (gRPC) / `:4318` (HTTP) |
| Mailpit | http://localhost:8025 | SMTP `localhost:1025` |

**Credentials** are random per machine: `make up` writes them to `infra/compose/.env` (gitignored, never committed).
Demo users `admin@acme.test` (tenant admin), `user@globex.test` (user), `vendor@nexora.test` (platform admin) share
`NEXORA_DEV_USER_PASSWORD`. The `nexora-dev-cli` client allows the password grant for tests — local only.

**Sign-in** (ADR 0010): login page http://localhost:8180/realms/nexora/account (themed, username or e-mail, remember
me). Tokens: issuer `http://localhost:8180/realms/nexora`, audience `nexora-api`, tenant in `tenants`. Asking for
`acr_values=mfa` adds the OTP step (set up on first use). Self-registration is off; invitations from
`POST /api/v1/platform/invitations` arrive in Mailpit. Five wrong passwords lock the account for a while.

**Notes**
- Every port binds to `127.0.0.1` only.
- Vault runs in dev mode (in memory): `make up` re-seeds keys after a restart; secrets written to it do not survive.
- S3 is SeaweedFS because MinIO no longer publishes community images; Nexora only uses the S3 API, so production on OCI
  Object Storage is unaffected.
- Redis 8 is AGPL-licensed upstream; fine for local dev. Production uses the managed cache of the cloud (open question 9).
