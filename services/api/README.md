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

## Adding a module
1. `src/Modules/<Name>/Nexora.Modules.<Name>` (+ `.Contracts`), a sealed class implementing `IModule`.
2. Register it in `Program.cs`: `AddModules(..., new PlatformModule(), new <Name>Module())`.
3. Its schema: `db/migrations` (see the README there).
