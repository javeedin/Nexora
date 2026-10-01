# CLAUDE.md — Nexora

## What this is
Nexora is a **multi-tenant SaaS product** for Oracle Fusion customers (WMS, Fusion SQL, data loading, orders, SCM
workbench, pipelines, analytics, governed AI agents, AI model gateway). It is a **rewrite** of a single-customer desktop
system (Gray's WMS v12) kept on the read-only branch **`legacy/v12`**.

## Read first (every session)
0. **`docs/plan/PROGRESS.md`** — current phase, next task, blockers, open questions, session log. Then the current
   phase file in `docs/plan/` and the rules in `docs/plan/README.md`.
1. `docs/rd/README.md` — the Requirements Document. Every feature has a stable ID (`WM-11`, `SQL-08`, `AG-11` …).
2. `docs/rd/03-architecture.md` — target stack and cross-cutting rules.
3. `docs/rd/90-legacy-map.md` — where each module's logic lives on `legacy/v12`, customer-specific values, security debts.
4. `docs/adr/` — decisions already taken. Propose a new ADR before changing one.

## Using the legacy branch
- Read with `git show legacy/v12:<path>` or `git worktree add ../legacy legacy/v12`. Its root `CLAUDE.md` describes the
  legacy system in depth (module by module).
- Use it to learn **business logic and edge cases** (statuses, rules, Fusion payloads, retries). Do **not** copy its
  architecture: no WebView IPC, no client-built SQL, no hard-coded URLs / ids, no `C:\fusion` paths, no plain-text secrets.
- Pure, tested legacy logic may be **ported with its tests**: `om/om-engine.js`, `dataload/fbdi-engine.js` +
  `fbdi-rules.js`, `fusionsql/watch-engine.js`, `fusionsql/knowledge-engine.js`, `engine/FusionModel/` (semantic engine,
  packs), `pipeline-server/pipeline_server/`, `ai-hub/ai_hub/`.
- Some legacy server logic exists only in the customer's APEX workspace; if `legacy/apex-export/` is missing, say so
  instead of guessing.
- Never merge `legacy/v12` into `main`.

## Non-negotiable rules
1. **Tenancy:** every table, query, cache key, file path, event, workflow and AI request carries `tenant_id`; no data access
   without the tenant filter (enforced in the data layer, covered by tests).
2. **No customer specifics in core code.** Customer / country values are tenant settings or **packs** (fiscal, analytics,
   rules, knowledge). Nothing named after a customer in `main`.
3. **Secrets** only in Vault (per-tenant keys). Never in tables, config files, browser storage, logs or prompts.
4. **Fusion:** read-only by default; writes only via official APIs with explicit scopes, idempotency keys and audit.
5. **AI actions** go through the Action Gateway: policy (OPA) → approval if needed (fingerprint-bound, single use,
   expiring) → execute → audit → cost. Kill switch honoured everywhere. Evals gate prompt / model changes.
6. **Every external call**: timeout, retry with back-off, circuit breaker, trace span.
7. **Typed APIs** (OpenAPI → generated TS client). Ad-hoc SQL only in Nexora SQL (Fusion, read-only, runner) and admin tools.
8. **Tests with every change**: unit (domain logic), integration (Testcontainers), E2E for user journeys, evals for AI.
9. Commit messages: Conventional Commits, reference requirement IDs (e.g. `feat(wms): ship confirm workflow (WM-11)`).

## Stack (see ADRs)
React 19 + TS + Vite + TanStack + Tailwind/shadcn · .NET 10 (LTS) modular monolith · Oracle 23ai + Liquibase · Temporal ·
Kafka · Python 3.12 AI platform (FastAPI, LangGraph, LangChain-core, MLflow) · Vault · OpenTelemetry · GitHub Actions ·
Docker / Kubernetes (OKE) / Terraform.

## How to work across sessions
The full scope is written down: 163 requirements (`docs/rd/modules/`) mapped to 140 ordered tasks in 7 phases + later
(`docs/plan/`). Sessions do not share memory — **the repo is the memory**.
1. Start: read `docs/plan/PROGRESS.md` → take the **first `todo` task of the current phase whose dependencies are done**
   (or the task the user names). If the user just says "continue", do exactly that.
2. Before coding: read the task's requirement IDs in the RD and the legacy pointers on `legacy/v12`.
3. Branch `feat/<task-id>-<short-name>`; commits reference task + requirement IDs; PR into `main` when the task's
   "Done when" and the Definition of done (`docs/plan/README.md`) are met.
4. Same PR: update `PROGRESS.md` (task status + PR, requirement statuses, "Current state") and tick the phase file.
5. Phase gates: do not start the next phase until the current phase's exit criteria are met and marked in `PROGRESS.md`,
   unless the user says so.
6. Unclear or wrong requirement → add to `PROGRESS.md › Open questions` and ask; never silently change the RD or an ADR
   (changes need the user's OK, then update RD / ADR first, then code).
7. End of session (or before the context runs out): add a row to `PROGRESS.md › Session log` (what was done, what is next,
   anything half-finished and where).

## Current phase
**Phase 0 — Foundation** — next task **P0-T02** (finish: PR + ruleset), then **P0-T03** (see `docs/plan/phase-0-foundation.md`). Keep this line in sync with
`PROGRESS.md`.
