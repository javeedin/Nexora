# 3. Target architecture and technology

## 3.1 Overview
```
 Web app (React)   Picker / driver PWA   Admin console        Nexora Edge (on-prem agent: printers, files, private networks)
        │                  │                   │                         │ outbound TLS (WebSocket / gRPC)
        └──────── HTTPS + OIDC (per-tenant org) ────────────────┐       │
                                                                 ▼       ▼
                         API gateway / BFF  — authn, tenant resolution, entitlements, rate limits, audit
                                                                 │
   ┌──────────────── Core platform (.NET 10 modular monolith → split when needed) ───────────────────────┐
   │ Tenancy · Identity/RBAC · Entitlements · Settings · Audit · Notifications · Files · Packs registry  │
   │ Modules: WMS · SQL · Load · Orders · SCM Workbench · Pipelines (control) · Analytics (API) · Agents │
   └──────┬─────────────────────┬───────────────────────┬──────────────────────┬────────────────────────┘
          │ outbox → events     │ workflows             │ Fusion calls          │ AI calls
     Kafka / OCI Streaming   Temporal cluster      Fusion Connector svc     AI Platform (Python)
                                                    REST · BIP runner ·     gateway (routing, budget, data policy)
                                                    FBDI/ERP Integration ·  LangGraph agents · evals · RAG · ML models
                                                    BICC · SOAP             (Bedrock / Claude / NIM / …)
          │
   Oracle Database (system of record, per-tenant isolation) · Object storage (files, PDFs, FBDI zips)
   Analytics store: Parquet / DuckDB (+ dbt) · Redis (cache, rate limits) · Vault (secrets, per-tenant keys)
```

## 3.2 Technology choices
| Layer | Choice | Notes |
|---|---|---|
| Web front end | React 19 + TypeScript, Vite, TanStack Router + Query, Tailwind + shadcn/ui, AG Grid (heavy grids), Monaco (SQL editor), ECharts / Chart.js, i18next | One design system in `packages/ui`, Storybook |
| Mobile | PWA (offline queue, camera scanning) first; React Native only if hardware scanners need it | Pickers, drivers |
| Desktop presence | None required; **Nexora Edge** (Go or .NET worker service) for local devices | Replaces the WinForms + WebView2 host |
| Core backend | .NET 10 (LTS, ADR 0009), ASP.NET Core minimal APIs, modular monolith (one module = one folder + own schema), MediatR, FluentValidation, EF Core + Dapper, Polly | OpenAPI → generated TS client (`packages/api-client`) |
| Workflows | **Temporal** (.NET + Python SDKs) | Print trip, MRA batch, FBDI import, provisioning, approvals (signals), agent long waits |
| Events | Kafka (OCI Streaming / MSK / Event Hubs), transactional outbox | Order / trip / print / pipeline events |
| Fusion connector | .NET service: REST (resource index + describe), BIP SOAP runner (DBMS_XMLGEN report), ERP Integration (importBulkData, ESS), UCM, BICC; credentials from Vault per tenant + pod | Read-only by default; write scopes explicit |
| AI platform | Python 3.12, FastAPI, LangGraph (+ checkpoints in Postgres/Oracle), LangChain-core adapters, MCP servers for tools, Langfuse tracing, promptfoo / custom evals, MLflow for ML models (scikit-learn / LightGBM / TensorFlow → ONNX) | Gateway = evolution of `ai-hub/` |
| Pipelines | Dagster (or Temporal-based runner first, evolved from `pipeline-server/`), dbt for transforms | Incremental watermarks, merge, CDC later |
| Analytics | Parquet on object storage + DuckDB engine (evolved from `engine/FusionModel/` semantic engine), dbt, Power BI / Superset; semantic layer API | DAX-compatible measures kept |
| Database | Oracle 23ai (system of record; JSON, vector search available), Liquibase migrations, per-tenant VPD | Postgres possible for platform metadata if preferred |
| Auth | OIDC with **Keycloak** (self-hosted, organisations = tenants, ADR 0010), SCIM, OPA for policies | |
| Secrets | HashiCorp Vault / OCI Vault / Azure Key Vault, per-tenant keys (envelope encryption) | No credentials in tables or browser storage |
| Observability | OpenTelemetry → Grafana (Tempo, Loki, Prometheus, Mimir) or Datadog; Sentry (front end) | Trace from click to Fusion call |
| Delivery | GitHub Actions (build, test, CodeQL, Trivy, gitleaks, SBOM), container images, Helm, Argo CD, Terraform | Environments: dev, test, staging, prod per region |
| Runtime | Kubernetes (OKE first — Oracle customers, near Fusion pods; AKS/EKS possible) | Regions: EU (Frankfurt), ME, IN, US |
| Quality | ESLint + Prettier, Roslyn analyzers + .editorconfig, Ruff + mypy, SonarQube, pre-commit, Conventional Commits | |
| Testing | xUnit + Testcontainers (Oracle Free, Kafka, Temporal), Vitest + Testing Library, Playwright E2E, Pact (contract), k6 (load), AI evals in CI | |

## 3.3 Monorepo layout
```
apps/web            React web app (all modules, lazy-loaded per entitlement)
apps/picker-pwa     picker / driver PWA
apps/print-agent    Nexora Edge (printers, local files, private networks)
services/api        .NET core platform + modules (modular monolith)
services/fusion-connector
services/workflows  Temporal workers (.NET) — business workflows
services/ai-platform Python: gateway, agents, evals, ML
packages/ui         design system   packages/api-client  generated client   packages/config  shared lint / tsconfig
db/migrations       Liquibase changelogs (per module schema)
infra/              terraform, k8s (helm), compose (local dev stack)
docs/               rd (this document), adr, runbooks
```

## 3.4 Cross-cutting rules
- **Tenant context** is resolved once (token → tenant, user, roles, entitlements) and flows through every call, event,
  workflow and AI request; tests fail if a repository query lacks the tenant filter.
- **No literal SQL from clients.** The legacy pattern of pages sending SQL text to `ai/executequery|executewrite` is
  replaced by typed APIs; ad-hoc SQL exists only in Nexora SQL (Fusion, read-only, through the runner) and in admin tools.
- **Idempotency** keys on every write API and workflow activity (prints, Fusion writes, MRA posts).
- **Outbox** for every event; consumers idempotent.
- **AI actions** go through one Action Gateway: policy (OPA) → approval (if needed) → execute → audit → cost.
- **Every external call** has timeout, retry with back-off, circuit breaker and a trace span.
