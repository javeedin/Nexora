# Phase 1 — Nexora SQL + AI Hub

Goal: the first sellable module (Starter plan) and the governed AI gateway every later AI feature uses.
**Exit criteria:** pilot users run, save, share and schedule Fusion queries in Nexora instead of the legacy Fusion SQL
page; Ask AI answers through the gateway (Claude in Amazon Bedrock first) with cost in the usage ledger; a watchdog
alerts to Teams from the server; evals scoreboard runs on the pilot's verified questions.

| Task | What | Covers | Depends | Done when |
|---|---|---|---|---|
| P1-T01 | **AI platform service** (`services/ai-platform`, FastAPI): port legacy `ai-hub/ai_hub` (providers, gateway, usage, LangChain adapter) to multi-tenant: tenant context from a service token, provider credentials from Vault per tenant, usage written to platform metering events | AH-01, AH-02, AH-04, AH-07 | P0 | legacy pytest suite ported and green; tenant isolation test |
| P1-T02 | Router per tenant: tasks, candidates, data classes, budgets, route preview; admin UI (Providers, Router, Prices, Budget) in `apps/web` | AH-03 | T01 | budget stop and data-class skip covered by tests |
| P1-T03 | **Action / model gateway API** used by .NET modules (gRPC or HTTP): `chat`, `compare`, streaming; kill switch per tenant + vendor; audit of every call with model / tokens / cost | AH-03, AG-12 (switch), PC-08 | T01 | kill switch blocks calls; audit rows visible |
| P1-T04 | **Usage metering**: metering events (ai.tokens, fusion.query …) → daily aggregates per tenant; usage screen | PC-12 (metering part), AH-04 | T03 | usage page matches ledger in an integration test |
| P1-T05 | **Playground** UI | AH-05 | T02 | compare 3 models side by side |
| P1-T06 | **Tracing** of prompts / tool calls (Langfuse or OTel GenAI conventions) | AH-08 | T01 | each Ask AI run has a trace with spans per tool |
| P1-T07 | **Notifications** service: in-app, e-mail (tenant SMTP via Vault or platform), Teams / Slack webhooks with host allow-list, user preferences | PC-09 | P0 | watchdog alert reaches Teams in a test with a mock webhook |
| P1-T08 | **SQL editor** (Monaco, autocomplete from schema cache), run on pod via connector, result grid (sort / filter / group / export Excel / CSV / copy as INSERT), call inspector | SQL-01, SQL-04, SQL-13 | P0-T14 | legacy page parity checklist (see legacy `fusionsql/fusionsql.js`) |
| P1-T09 | **Runner deployment wizard** + health per pod | SQL-02 | P0-T14 | deploys report to tenant folder on a real TEST pod |
| P1-T10 | **Schema browser** (objects, columns, source, dependencies) with per-pod cache | SQL-03 | T08 | |
| P1-T11 | **Saved queries**: params `{{P_X}}`, tags, sharing (private / team / tenant), versions; migrate pilot's WMS_FUSION_SQL_QUERIES | SQL-05 | T08 | pilot queries imported |
| P1-T12 | **Ask AI** agent (LangGraph) with read-only Fusion dictionary tools; streams steps; returns SQL + explanation | SQL-06, AG-32 | T03, T10 | eval set of ≥ 20 verified questions ≥ legacy accuracy |
| P1-T13 | **Knowledge**: facts lifecycle, learn from saved queries (port `knowledge-engine.js` + tests), AI proposals, 👍 Correct → verified example, injection into Ask AI with budget; migrate pilot facts | SQL-07 | T11, T12 | engine tests ported; injection visible in trace |
| P1-T14 | **Watchdogs** server-side: Temporal schedules, metric SQL, rules AUTO / ABOVE / BELOW / CHANGE (port `watch-engine.js` + tests), alerts with cool-down, cards with sparkline / band, history | SQL-08 | T07, T08 | baseline tests ported; alert E2E |
| P1-T15 | **Format results** dashboard + exports (Excel, PDF, HTML, PNG) + share (Outlook draft link, rich copy) | SQL-09 | T08 | |
| P1-T16 | **Flows**: definitions, step chaining with `{{KEY}}`, run per document, diagram, report, library of 65 processes (port `flows-catalog.js`), AI builds a flow | SQL-11 | T08, T12 | Order-to-Cash flow runs on TEST pod |
| P1-T17 | **Setup checklist**: tasks with check SQL, results per pod, drill-down (port seed) | SQL-12 | T08 | |
| P1-T18 | **Evals**: tenant verified examples → models → result comparison on Fusion → scoreboard, history; CI eval gate for prompt changes | AH-06 | T02, T13 | scoreboard reproduces legacy AI Hub behaviour |
| P1-T19 | Pilot migration + parallel run (1 week) + switch | — | all | exit criteria checked in PROGRESS.md |
