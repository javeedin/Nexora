# Nexora

**The operations cloud for Oracle Fusion customers** — warehouse execution, Fusion data tools, data pipelines and
governed AI agents in one multi-tenant subscription.

- Requirements: [`docs/rd/`](docs/rd/README.md) · Decisions: [`docs/adr/`](docs/adr/) · Working rules: [`CLAUDE.md`](CLAUDE.md)
- Legacy reference: branch **`legacy/v12`** (the desktop system this product grows from — read-only)

| Path | What |
|---|---|
| `apps/web` | React + TypeScript web app |
| `apps/picker-pwa` | Picker / driver PWA |
| `apps/print-agent` | Nexora Edge (on-prem agent) |
| `services/api` | .NET 9 core platform + modules |
| `services/fusion-connector` | Oracle Fusion connector |
| `services/workflows` | Temporal workers |
| `services/ai-platform` | Python AI platform (gateway, agents, evals, ML) |
| `packages/*` | design system, generated API client, shared config |
| `db/migrations` | Liquibase changelogs |
| `infra/*` | Terraform, Kubernetes, local compose stack |

Status: phase 0 (foundation) — see [roadmap](docs/rd/95-migration-and-roadmap.md).
