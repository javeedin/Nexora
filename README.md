# Nexora

**The operations cloud for Oracle Fusion customers** — warehouse execution, Fusion data tools, data pipelines and
governed AI agents in one multi-tenant subscription.

- Requirements: [`docs/rd/`](docs/rd/README.md) · Plan & progress: [`docs/plan/`](docs/plan/PROGRESS.md) · Decisions: [`docs/adr/`](docs/adr/) · Working rules: [`CLAUDE.md`](CLAUDE.md)
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

## Getting started

Prerequisites: Node 22 (`.nvmrc`) with pnpm 10 (`corepack enable`), .NET SDK 9+ (`global.json` rolls forward), [uv](https://docs.astral.sh/uv/) (installs Python 3.12 itself).

```sh
pnpm install          # TS workspaces + tooling
pnpm hooks:install    # pre-commit + commit-msg hooks (format, lint, secrets, Conventional Commits + requirement IDs)
pnpm build            # every workspace: TS, services/api (dotnet build), services/ai-platform (uv sync)
pnpm test             # node --test, dotnet test, pytest
pnpm lint             # ESLint, Prettier, dotnet format, Ruff
pnpm typecheck        # tsc, mypy --strict
pnpm affected         # build + test + lint only what changed vs main (used by CI)
```

Each service still works on its own: `dotnet build services/api/Nexora.sln`, `cd services/ai-platform && uv run pytest`.
Commit messages: `feat(wms): ship confirm workflow (WM-11)` — `feat` / `fix` / `perf` / `refactor` must cite a requirement or plan task ID.

Status: phase 0 (foundation) — see [roadmap](docs/rd/95-migration-and-roadmap.md).
