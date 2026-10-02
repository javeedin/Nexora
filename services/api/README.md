# Nexora API — modular monolith (.NET 10)

```sh
dotnet run --project src/Nexora.Api          # http://localhost:5000 · API docs (Development): /docs
dotnet test                                  # integration (incl. Redis via Testcontainers) + architecture tests
```

With the dev stack (`make up`) export `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317` and
`ConnectionStrings__Redis=localhost:6379,password=<REDIS_PASSWORD>` to get traces in Grafana and Redis-backed idempotency.

## Layout
| Path | What |
|---|---|
| `src/Nexora.Api` | Host: cross-cutting pipeline, registers modules explicitly |
| `src/BuildingBlocks` | Shared kernel: `IModule`, validation, idempotency, problem details, rate limiting, health, telemetry |
| `src/Modules/<Name>/Nexora.Modules.<Name>` | A module: endpoints, application logic, persistence (own schema) |
| `src/Modules/<Name>/Nexora.Modules.<Name>.Contracts` | The only part other modules may reference (BCL types only) |
| `openapi/v1.json` | Generated at build — commit it; CI fails when it is stale. Source of the TS client |

## Conventions (enforced by tests)
- Endpoints live under `/api/v1/<module>/…` (`EndpointConventionTests`).
- Every write endpoint calls `.WithIdempotency()` — clients send `Idempotency-Key` — or `.WithoutIdempotency("why")`.
  Same key + same request replays the response (`Idempotent-Replayed: true`); different request → 422; in flight → 409;
  5xx responses are not stored.
- Requests are validated with FluentValidation: `.WithValidation<TRequest>()`; validators are picked up per module.
- Errors are RFC 9457 problem details with `traceId`.
- Modules reference other modules only through `*.Contracts`; contracts reference nothing but the BCL; building blocks
  know no module (`ModuleBoundaryTests`).
- Rate limit: token bucket per tenant (per client address until P0-T07), `RateLimiting:PermitsPerMinute` (default 600).
- `/health/live` (process) and `/health/ready` (dependencies, e.g. Redis) for Kubernetes probes.

## Sign-in and tenants (ADR 0010)
- Bearer tokens from Keycloak (realm `nexora`): issuer, audience `nexora-api`, signature and lifetime are validated.
  Everything requires sign-in unless marked `.AllowAnonymous()` (health, OpenAPI, `/platform/info`).
- Tenant = Keycloak organisation; its id comes from the token claim `tenants: {"<alias>": {"id": "<uuid>"}}`.
  Users in several tenants send `X-Nexora-Tenant: <alias or id>`; a tenant you don't belong to is 403, never a fallback.
- Policies: `tenant-member`, `tenant-admin`, `platform-admin` (`NexoraPolicies`). Every write endpoint names one
  (`EndpointConventionTests`). Admin policies need MFA (`acr = mfa`); without it the API answers 403 with the RFC 9470
  challenge `WWW-Authenticate: Bearer error="insufficient_user_authentication", acr_values="mfa"`.
  `Auth:RequireMfaForAdmins` is on by default and off in `appsettings.Development.json`.
- `GET /api/v1/platform/me` — who am I; `POST /api/v1/platform/invitations` — tenant admin invites by e-mail
  (Keycloak organisation invite; registration is invitation-only).
- Local: `Keycloak__ClientSecret` = `NEXORA_API_CLIENT_SECRET` from `infra/compose/.env` (Vault from P0-T13).

## Adding a module
1. `src/Modules/<Name>/Nexora.Modules.<Name>` (+ `.Contracts`), a sealed class implementing `IModule`.
2. Register it in `Program.cs`: `AddModules(..., new PlatformModule(), new <Name>Module())`.
3. Its schema: `db/migrations` (see the README there).
