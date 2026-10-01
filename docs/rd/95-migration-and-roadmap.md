# 95. Migration and roadmap

Strategy: **build the platform, then move modules one by one; the legacy desktop app keeps running for the pilot tenant
until each module is live in Nexora** (strangler). Gray's = tenant #1 (pilot), data migrated per module.

| Phase | Scope | Exit criteria |
|---|---|---|
| **0 — Foundation** (≈ 6–8 weeks) | Monorepo, CI/CD (GitHub Actions: build, test, CodeQL, Trivy, gitleaks), dev stack (compose: Oracle Free, Temporal, Kafka, Redis, Keycloak), IaC skeleton (OKE), **tenancy + identity + RBAC + entitlements + settings + audit (M0)**, design system + app shell, generated API client, Fusion connector v0 (pods, Vault credentials, REST GET, BIP runner), observability, ADRs. Export the APEX workspace + BIP catalog into the legacy branch. | Sign up a tenant, invite a user, register a Fusion pod, run a read-only Fusion query from the web app; everything traced and audited |
| **1 — Nexora SQL + AI Hub** | M2 (editor, schema, saved queries, Ask AI, Knowledge, Watchdogs server-side, Flows, Setups, report formatting) + M9 (gateway, providers AWS-first, router, budgets, usage, evals) | Pilot users use Nexora SQL instead of the desktop Fusion SQL page |
| **2 — WMS core + Edge** | Nexora Edge (printers), M1 trips / pick / ship / print / monitor printing, shipping agent as workflow, fiscal pack MRA (M10), picker PWA v1 | Pilot dispatch runs a full day on Nexora; desktop app no longer needed for WMS |
| **3 — Orders + Load** | M4 order pad (engine ported with its tests), M3 FBDI prepare / check / generate + upload & import, REST loads, FSM tracking | First FBDI import end-to-end through Nexora |
| **4 — Agents** | M8 chat agent on the AI platform with policies (OPA), approvals, inbox, jobs, tasks, forms, flows; Pipeline Doctor | Every acting AI step governed; legacy AI page retired |
| **5 — Pipelines + Analytics** | M6 (from `pipeline-server`), M7 (semantic engine, packs, dashboards, Power BI feed, MCP) | Analytics on Parquet per tenant; refresh by workflows |
| **6 — SCM Workbench + commercial** | M5 screens; billing (Stripe / marketplace), self-service sign-up, second fiscal pack, second and third tenants, SOC 2 readiness | 3 paying tenants |
| **Later** | ML models (forecasting, anomalies, document AI), receiving / locators, mobile native if needed, more regions | |

Per-module migration recipe: (1) write the module's API + data model from this RD, (2) port pure logic with its tests
(`om-engine.js`, `fbdi-engine.js`, `watch-engine.js`, `knowledge-engine.js`, semantic engine, pipeline runner), (3) migrate
the pilot's data for the module (scripts in `db/migrations/data/`), (4) run in parallel with legacy for one week, (5) switch
users, (6) remove the legacy page from the pilot's menu.
