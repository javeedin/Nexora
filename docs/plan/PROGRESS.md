# Progress tracker

> The single source of "where are we". Update it **in the same PR** as the work (rules: `docs/plan/README.md`).

## Current state

| Item | Value |
|---|---|
| Current phase | **Phase 0 — Foundation** (in progress: T01, T03–T06 done; T02 awaiting PR) |
| Next task | **P0-T07** Tenancy core (P0-T02 still needs: PR run + ruleset import) |
| Blockers | P0-T24 still needs the PL/SQL + table / view DDL of `WKSP_GRAYSAPP` and the BI Publisher catalog (ORDS REST export received 2026-10-01) — blocks phase 2 |

## Phase exit criteria

| Phase | Scope | Exit criteria | Status |
|---|---|---|---|
| 0 | Foundation | see [phase-0-foundation.md](phase-0-foundation.md) | todo |
| 1 | Nexora SQL + AI Hub | see [phase-1-sql-aihub.md](phase-1-sql-aihub.md) | todo |
| 2 | WMS + Edge + MRA pack | see [phase-2-wms-edge.md](phase-2-wms-edge.md) | todo |
| 3 | Orders + Load | see [phase-3-orders-load.md](phase-3-orders-load.md) | todo |
| 4 | Agents | see [phase-4-agents.md](phase-4-agents.md) | todo |
| 5 | Pipelines + Analytics | see [phase-5-pipelines-analytics.md](phase-5-pipelines-analytics.md) | todo |
| 6 | SCM Workbench + commercial | see [phase-6-scm-commercial.md](phase-6-scm-commercial.md) | todo |

## Tasks (140)

| Phase | Task | What | Status | PR / notes |
|---|---|---|---|---|
| 0 | P0-T01 | Monorepo tooling: pnpm workspaces + Turborepo for TS; .NET solution `Nexora.sln` under … | done | branch `claude/nifty-edison-j36zte` (PR pending) |
| 0 | P0-T02 | CI: GitHub Actions — build + test per workspace (affected only), CodeQL, Trivy (deps + … | in-progress | workflows + Dependabot + ruleset on branch `claude/nifty-edison-j36zte`; \"Done when\" needs a PR run and the ruleset imported (repo admin) |
| 0 | P0-T03 | Local dev stack `infra/compose`: Oracle Free 23ai, Keycloak (dev IdP), Temporal + UI, … | done | branch `claude/nifty-edison-j36zte` (PR pending): `make up` → 10 services healthy + seeded in < 1 min; `make smoke` 9 end-to-end checks; S3 = SeaweedFS (see Q9) |
| 0 | P0-T04 | DB migrations: Liquibase project in `db/migrations` (changelog per module schema), run … | done | branch `claude/nifty-edison-j36zte` (PR pending): Liquibase 5 + ojdbc11 image; schema-only `platform` account reached by proxy; full rollback test in CI on Oracle Free; changelog checker enforces tenant_id NOT NULL |
| 0 | P0-T05 | API skeleton (.NET 10): modular-monolith host, module registration, OpenAPI, … | done | branch `claude/nifty-edison-j36zte` (PR pending): modules + contracts, OpenAPI 3.1 at build, problem details, validation, idempotency (Redis / in-memory), rate limiting, health, OTel; `/health` trace verified in Tempo; 29 tests incl. NetArchTest |
| 0 | P0-T06 | Identity: OIDC (Keycloak dev, Auth0 / Entra External ID prod) with organisation = … | done | branch `claude/nifty-edison-j36zte` (PR pending): Keycloak (ADR 0010) — tenant id + roles in the token, MFA step-up, invitation-only sign-up, legacy-style login theme; API auth + `/me` + invitations; 50 tests incl. real Keycloak. Web-app sign-in itself arrives with P0-T18; profile sync to DB moves to P0-T07 (needs the data layer) |
| 0 | P0-T07 | Tenancy core: `tenant` entity + lifecycle (create, suspend, delete); tenant context … | todo | |
| 0 | P0-T08 | RBAC + scopes: roles per module (viewer / user / approver / admin), scopes (site, BU, … | todo | |
| 0 | P0-T09 | Entitlements: plans, add-ons, overrides → resolved entitlements per tenant; gateway / … | todo | |
| 0 | P0-T10 | Settings framework: typed settings with JSON schema at tenant / site / user level, … | todo | |
| 0 | P0-T11 | Reference data: sites / warehouses, business units, inventory orgs, currencies, pods … | todo | |
| 0 | P0-T12 | Audit log: append-only audit table (+ WORM export), middleware for writes, API to … | todo | |
| 0 | P0-T13 | Secrets: Vault integration (per-tenant key, envelope encryption), secret references in … | todo | |
| 0 | P0-T14 | Fusion connector v0 (`services/fusion-connector`): pods (base URL, credentials in … | todo | |
| 0 | P0-T15 | Files: object storage per tenant (MinIO / OCI), upload / download with signed URLs, AV … | todo | |
| 0 | P0-T16 | Workflows base: Temporal worker project (`services/workflows`), tenant-aware activity … | todo | |
| 0 | P0-T17 | Events base: outbox table + publisher to Kafka, consumer base with idempotency, event … | todo | |
| 0 | P0-T18 | Web app shell (`apps/web`): React 19 + Vite + TanStack Router / Query, auth, tenant + … | todo | |
| 0 | P0-T19 | Tenant admin console v1: users + invitations, roles, sites / BUs, pods (with test), … | todo | |
| 0 | P0-T20 | Vendor console v0: create / suspend tenants, assign plans, feature flags (Unleash) | todo | |
| 0 | P0-T21 | Read-only Fusion query in the web app (thin slice of SQL-01): pick pod, run SELECT, grid | todo | |
| 0 | P0-T22 | IaC + dev environment: Terraform for OKE (cluster, registry, Vault, object storage, … | todo | |
| 0 | P0-T23 | Observability: dashboards (API latency, errors, Fusion calls, workflow failures) with … | todo | |
| 0 | P0-T24 | Legacy APEX export (needs the user): ORDS modules + PL/SQL of `WKSP_GRAYSAPP` and the … | in-progress | ORDS export received (71 modules, 414 handlers); PL/SQL + DDL + BIP catalog outstanding — see open question 7 |
| 1 | P1-T01 | AI platform service (`services/ai-platform`, FastAPI): port legacy `ai-hub/ai_hub` … | todo | |
| 1 | P1-T02 | Router per tenant: tasks, candidates, data classes, budgets, route preview; admin UI … | todo | |
| 1 | P1-T03 | Action / model gateway API used by .NET modules (gRPC or HTTP): `chat`, `compare`, … | todo | |
| 1 | P1-T04 | Usage metering: metering events (ai.tokens, fusion.query …) → daily aggregates per … | todo | |
| 1 | P1-T05 | Playground UI | todo | |
| 1 | P1-T06 | Tracing of prompts / tool calls (Langfuse or OTel GenAI conventions) | todo | |
| 1 | P1-T07 | Notifications service: in-app, e-mail (tenant SMTP via Vault or platform), Teams / … | todo | |
| 1 | P1-T08 | SQL editor (Monaco, autocomplete from schema cache), run on pod via connector, result … | todo | |
| 1 | P1-T09 | Runner deployment wizard + health per pod | todo | |
| 1 | P1-T10 | Schema browser (objects, columns, source, dependencies) with per-pod cache | todo | |
| 1 | P1-T11 | Saved queries: params `{{P_X}}`, tags, sharing (private / team / tenant), versions; … | todo | |
| 1 | P1-T12 | Ask AI agent (LangGraph) with read-only Fusion dictionary tools; streams steps; returns … | todo | |
| 1 | P1-T13 | Knowledge: facts lifecycle, learn from saved queries (port `knowledge-engine.js` + … | todo | |
| 1 | P1-T14 | Watchdogs server-side: Temporal schedules, metric SQL, rules AUTO / ABOVE / BELOW / … | todo | |
| 1 | P1-T15 | Format results dashboard + exports (Excel, PDF, HTML, PNG) + share (Outlook draft link, … | todo | |
| 1 | P1-T16 | Flows: definitions, step chaining with `{{KEY}}`, run per document, diagram, report, … | todo | |
| 1 | P1-T17 | Setup checklist: tasks with check SQL, results per pod, drill-down (port seed) | todo | |
| 1 | P1-T18 | Evals: tenant verified examples → models → result comparison on Fusion → scoreboard, … | todo | |
| 1 | P1-T19 | Pilot migration + parallel run (1 week) + switch | todo | |
| 2 | P2-T01 | Packs registry: install / upgrade / configure packs per tenant (manifest, config … | todo | |
| 2 | P2-T02 | Nexora Edge agent (`apps/print-agent`, .NET worker or Go): pairing to a site, outbound … | todo | |
| 2 | P2-T03 | WMS data model + APIs from the APEX export: trips, trip lines, pickers, vehicles, … | todo | |
| 2 | P2-T04 | Trip list / board UI (filters, tabs, KPIs, volume grids, export, multi-window) | todo | |
| 2 | P2-T05 | Create trip, add orders (pending orders / paste / pending shipment lines), remove, move … | todo | |
| 2 | P2-T06 | Assign picker | todo | |
| 2 | P2-T07 | Pick release workflow (Temporal): pick wave → open picks → lots per order; release all … | todo | |
| 2 | P2-T08 | Allocate lots / S2V; store transactions view with cancel line / lot, audited set-data … | todo | |
| 2 | P2-T09 | Order transactions view (pick release, SO lines, lots, shipment lines, confirm … | todo | |
| 2 | P2-T10 | Cancel lines (scheduled / selected / not picked) with child lines + promo map (tenant … | todo | |
| 2 | P2-T11 | Pick confirm + ship confirm workflows (step results, stop on failure, idempotent) | todo | |
| 2 | P2-T12 | Shipment lines per trip: actual ship date update, bulk cancel | todo | |
| 2 | P2-T13 | Trip analytics | todo | |
| 2 | P2-T14 | Document types (tenant config: report path + parameter + order-type rules) and print … | todo | |
| 2 | P2-T15 | Auto-print per trip + skip zero-line orders | todo | |
| 2 | P2-T16 | Monitor printing UI + print job grid | todo | |
| 2 | P2-T17 | MRA fiscal pack (pack interface + MRA implementation, config for seller block / tax … | todo | |
| 2 | P2-T18 | Shipping agent as Temporal workflow: agents, trips, tasks 1–3, approvals via inbox … | todo | |
| 2 | P2-T19 | Auto SO processing (real, not simulated) | todo | |
| 2 | P2-T20 | Auto inventory (S2V) processing incl. staged-transaction error clean-up, verify vs … | todo | |
| 2 | P2-T21 | Pending shipment lines grid + bulk volume | todo | |
| 2 | P2-T22 | Picker monitor, picking time monitor (score tunable) | todo | |
| 2 | P2-T23 | Picker / driver PWA (my picks, scan confirm, exceptions, location pings, offline queue) … | todo | |
| 2 | P2-T24 | Daily history / task mining (activity telemetry, journeys, friction, feedback) | todo | |
| 2 | P2-T25 | Inventory read screens (on-hand, lots, transactions, reservations) | todo | |
| 2 | P2-T26 | Pilot migration + parallel run + switch | todo | |
| 3 | P3-T01 | Pricing / discount / checks engine as a shared library: port `om/om-engine.js` with all … | todo | |
| 3 | P3-T02 | Orders data model + APIs (orders, lines, events, approvals, discounts, back-orders, … | todo | |
| 3 | P3-T03 | Order pad UI (keyboard-first: F7 customer, F6 items, scan, `12*code`, scanner words), … | todo | |
| 3 | P3-T04 | Lookup sources (BIP / Fusion SQL / tenant SQL with placeholders) + test | todo | |
| 3 | P3-T05 | Smart paste (rules → AI gateway, candidate items only) | todo | |
| 3 | P3-T06 | Usual items | todo | |
| 3 | P3-T07 | Approvals (content-hash bound) | todo | |
| 3 | P3-T08 | Fusion submission (draft / submit, price modes, consignment / crate lines, extras with … | todo | |
| 3 | P3-T09 | Orders board / list, timeline, copy, credit note, status sync, open Fusion order, … | todo | |
| 3 | P3-T10 | FBDI catalog + specs: port generator + generated catalog / specs per release; download … | todo | |
| 3 | P3-T11 | FBDI engine + rules: port `fbdi-engine.js`, `fbdi-rules.js` with tests | todo | |
| 3 | P3-T12 | Prepare & Load wizard (Source → Map → Check → Generate) + assist (browse tables, input … | todo | |
| 3 | P3-T13 | Upload + import: UCM upload, `importBulkData`, ESS monitoring, results and error rows … | todo | |
| 3 | P3-T14 | History of loads / runs / files, rebuild ZIP | todo | |
| 3 | P3-T15 | Fusion REST loads (catalog, probe, describe, sample + drill-down, map, group children, … | todo | |
| 3 | P3-T16 | FSM setup projects tracking | todo | |
| 3 | P3-T17 | FSM setup data analysis (exports, counts, match to tasks, compare) | todo | |
| 3 | P3-T18 | Load safety policies (TEST first, mandatory checks, approvals for PROD above threshold) | todo | |
| 4 | P4-T01 | Action Gateway: one service path for every acting AI step — OPA policy (AUTO / ASK / … | todo | |
| 4 | P4-T02 | Approval integrity: server-issued fingerprint of exactly what the card shows; single … | todo | |
| 4 | P4-T03 | Policies admin UI (admins only), seed defaults per plan | todo | |
| 4 | P4-T04 | Tools as MCP servers backed by Nexora APIs: tenant_query (typed / approved read SQL), … | todo | |
| 4 | P4-T05 | Chat agent (LangGraph): loop with round limit, streaming steps, attachments (xlsx / csv … | todo | |
| 4 | P4-T06 | Inbox: requests decided from web / mobile / Teams card, first decision wins, … | todo | |
| 4 | P4-T07 | Trained processes (definitions with stages, validations incl. CHECK_SQL, interfaces, … | todo | |
| 4 | P4-T08 | Knowledge packs (business knowledge per tenant / pack, versioned) replacing hard-coded … | todo | |
| 4 | P4-T09 | Redaction of secrets in prompts / tool I/O / logs | todo | |
| 4 | P4-T10 | Scheduled jobs (Temporal: ONCE / RECURRING / REPEAT_UNTIL_DONE; typed steps; cloud lane … | todo | |
| 4 | P4-T11 | Daily tasks board (people + AI, recurrence, events, results, library; work in chat or … | todo | |
| 4 | P4-T12 | Saved reports with parameters | todo | |
| 4 | P4-T13 | Forms designer (port JSON form engine; WYSIWYG; AI builds forms) | todo | |
| 4 | P4-T14 | Agent flows designer → compiled to LangGraph (nodes: AI agent, query, write w/ … | todo | |
| 4 | P4-T15 | Pipeline Doctor productised (from legacy `ai-hub` agent) | todo | |
| 4 | P4-T16 | Pilot migration (policies, processes, reports, forms, flows, tasks) + switch | todo | |
| 5 | P5-T01 | Decide Dagster vs Temporal-native runner (ADR); port legacy `pipeline-server` … | todo | |
| 5 | P5-T02 | Connections (Oracle EZ / TNS / ADB wallet, SQL Server, MySQL, PostgreSQL, DuckDB / … | todo | |
| 5 | P5-T03 | Pipelines + tasks + modes (APPEND / TRUNCATE_INSERT / MERGE / INCREMENTAL), column map, … | todo | |
| 5 | P5-T04 | Schedules (MANUAL / INTERVAL / CRON / CONTINUOUS), dependencies, on_error, timeouts | todo | |
| 5 | P5-T05 | Runs: claim, cancel, kill, retry from failed task, watermark per page, resume after … | todo | |
| 5 | P5-T06 | Workers in cloud regions and on Edge (private networks) | todo | |
| 5 | P5-T07 | Analytics engine: port `engine/FusionModel` (storage → Parquet per tenant, versioned … | todo | |
| 5 | P5-T08 | Semantic layer (relationships, DAX-compatible measures, calendar, hierarchies, roles) + … | todo | |
| 5 | P5-T09 | Fusion packs GL / AP / AR / PO / OM / INV via packs registry; check on pod | todo | |
| 5 | P5-T10 | Reconciliation checks | todo | |
| 5 | P5-T11 | Reports + dashboards (canvas, visuals, cross-filter, slicers, editor) | todo | |
| 5 | P5-T12 | Ask the model (catalog, hybrid search, agent tools), verified examples, glossary | todo | |
| 5 | P5-T13 | Dashboard copilot + quick dashboard | todo | |
| 5 | P5-T14 | REST + MCP access with tokens acting as users | todo | |
| 5 | P5-T15 | Power BI (push datasets, embedded reports, report links, Desktop feed with hashed keys) | todo | |
| 5 | P5-T16 | Save SQL result as dataset; send SQL to model; add SQL to pipeline | todo | |
| 6 | P6-T01 | Shared SCM screen engine (grid → `q`, paging, KPIs, drawer, forms, LOV / typeahead, pod … | todo | |
| 6 | P6-T02 | Order Management workbench screens | todo | |
| 6 | P6-T03 | Purchasing screens | todo | |
| 6 | P6-T04 | Inventory screens | todo | |
| 6 | P6-T05 | Costing screens | todo | |
| 6 | P6-T06 | Setup & diagnostics screens | todo | |
| 6 | P6-T07 | Close legacy gaps (DELETE operations with approval, SOAP on-hand load, login history) | todo | |
| 6 | P6-T08 | Billing: Stripe Billing (plans, metered prices from usage aggregates, invoices, … | todo | |
| 6 | P6-T09 | Self-service sign-up + trial (provisioning workflow, sample-data tenant, onboarding … | todo | |
| 6 | P6-T10 | Vendor console full (tenants, plans, health, consented support access, usage, revenue) | todo | |
| 6 | P6-T11 | Second fiscal pack (e.g. India IRP or KSA ZATCA) to prove the pack interface | todo | |
| 6 | P6-T12 | Compliance: SOC 2 controls evidence, pen test, DPA / privacy, data residency regions | todo | |
| 6 | P6-T13 | Self-hosted edition (Helm chart + signed licences, port `Licensing.cs`) | todo | |
| later | L-T01 | ML forecasting (order-pad quantities, item demand), trip / picking time prediction, … | todo | |
| later | L-T02 | Document AI: scanned invoices / delivery notes → rows → FBDI | todo | |
| later | L-T03 | Receiving (PO, duty-free, returns), locators, putaway, guided picking | todo | |
| later | L-T04 | Orders backlog: Fusion holds, PDA confirm, bulk e-mail, analytics pivots | todo | |
| later | L-T05 | Pipelines: BICC sources, CDC, dbt transforms, data quality | todo | |
| later | L-T06 | More agents: order exception, fiscal exception, FBDI import doctor, period-close assistant | todo | |
| later | L-T07 | More providers: Azure AI Foundry, Google Vertex AI, OCI Generative AI, self-hosted NIM | todo | |
| later | L-T08 | Native mobile app if hardware scanners require it; more regions; French UI | todo | |

## Requirements (163)

A requirement is **done** when all its tasks are done and its acceptance (RD) is met.

| ID | Module | Requirement | Planned in | Status |
|---|---|---|---|---|
| PC-01 | M0 | Tenant lifecycle: sign-up, provisioning workflow, suspend, export, delete | P0-T07, P0-T16, P6-T09 | todo |
| PC-02 | M0 | Pods: a tenant registers 1..n Fusion environments (name, base URL, … | P0-T14 | todo |
| PC-03 | M0 | Sites / warehouses, business units, inventory orgs, currencies as tenant … | P0-T11 | todo |
| PC-04 | M0 | Identity: OIDC organisation per tenant, SSO, MFA, SCIM (Enterprise), invitations | P0-T06 | in-progress — OIDC organisations, MFA step-up, invitations done (P0-T06); per-tenant SSO + SCIM later |
| PC-05 | M0 | RBAC: roles per module (viewer / user / approver / admin) + scopes (site, BU, … | P0-T08 | todo |
| PC-06 | M0 | Entitlements: plan + add-ons + overrides; UI hides modules not entitled; … | P0-T09 | todo |
| PC-07 | M0 | Settings framework: typed, versioned settings per tenant / site / user with … | P0-T10 | todo |
| PC-08 | M0 | Audit log: every write, Fusion call, AI action, login, settings change; … | P0-T12, P1-T03 | todo |
| PC-09 | M0 | Notifications: in-app, e-mail (tenant SMTP or ours), Teams / Slack webhooks … | P1-T07 | todo |
| PC-10 | M0 | Files: object storage per tenant (PDFs, FBDI zips, exports), virus scan, … | P0-T15 | todo |
| PC-11 | M0 | Packs registry: install / upgrade / configure packs per tenant (fiscal, … | P2-T01 | todo |
| PC-12 | M0 | Usage metering + billing integration (Stripe / marketplace); budget alerts | P1-T04, P6-T08 | todo |
| PC-13 | M0 | Admin console (tenant admin): users, roles, pods, sites, settings, packs, … | P0-T19 | todo |
| PC-14 | M0 | Vendor console (our staff): tenants, plans, health, support access … | P0-T20, P6-T10 | todo |
| PC-15 | M0 | Nexora Edge registration: pair an agent to a site, health, versions, remote … | P2-T02 | todo |
| PC-16 | M0 | Activity analytics: page / action telemetry per tenant (task mining) | P2-T24 | todo |
| WM-01 | M1 | Trip list by date range and pod with filters (lorry, picker, status), grid / … | P2-T03, P2-T04 | todo |
| WM-02 | M1 | Create trip; add orders from pending orders, pasted order numbers or pending … | P2-T05 | todo |
| WM-03 | M1 | Remove order from trip; move order to another trip (delete + add, roll back … | P2-T05 | todo |
| WM-04 | M1 | Trip header: lorry (required), priority, loading bay; status kept; profit … | P2-T03 | todo |
| WM-05 | M1 | Assign picker to a trip or selected orders with assignment date | P2-T06 | todo |
| WM-06 | M1 | Pick release: single order; release all = per order (1) release pick wave … | P2-T07 | todo |
| WM-07 | M1 | Allocate lots / store-to-van processing per order | P2-T08 | todo |
| WM-08 | M1 | Order transactions view: pick-release details, SO lines, lots, Fusion … | P2-T09 | todo |
| WM-09 | M1 | Cancel lines in Fusion (scheduled / selected / not picked) via … | P2-T10 | todo |
| WM-10 | M1 | Pick confirm by line / all via Fusion `pickTransactions`; on success update … | P2-T11 | todo |
| WM-11 | M1 | Ship confirm: get shipment number → Fusion `shippingTransactions` CONFIRM → … | P2-T11 | todo |
| WM-12 | M1 | Shipment lines per trip: update actual ship date … | P2-T12 | todo |
| WM-13 | M1 | Store transactions (S2V / V2S): details, allocated lots, QOH, cancel line / … | P2-T08 | todo |
| WM-14 | M1 | Trip analytics: order trends (day / week / month), status distribution, by … | P2-T13 | todo |
| WM-15 | M1 | Open each page in its own window / tab (multi-monitor dispatch desks) | P2-T04 | todo |
| WM-20 | M1 | Printer registry per site through Edge: discover local / network / Bluetooth … | P2-T02 | todo |
| WM-21 | M1 | Document types per tenant: report path + parameter for each document (sales … | P2-T14 | todo |
| WM-22 | M1 | Print job lifecycle: download (Pending → Downloading → Completed / Failed) … | P2-T14 | todo |
| WM-23 | M1 | Auto-print per trip: download all order documents and print in sequence; skip … | P2-T15 | todo |
| WM-24 | M1 | Monitor printing per trip: download / preview / print / print all, per-order … | P2-T16 | todo |
| WM-25 | M1 | Print job grid: filters, stats, preview, open, export, retry all failed | P2-T16 | todo |
| WM-26 | M1 | PDF storage in tenant object storage (not C:\fusion), cached on Edge for … | P2-T14 | todo |
| WM-30 | M1 | Agents: create (name, pod, capabilities), assign trips, start / pause / stop … | P2-T18 | todo |
| WM-31 | M1 | Task 1 — order status from Fusion shipment lines (Released to WH, Staged, … | P2-T18 | todo |
| WM-32 | M1 | Task 2 — lines stuck SCHEDULED / MANUAL RESERVATION (+ child lines): never … | P2-T18 | todo |
| WM-33 | M1 | Task 3 — auto-print orders Interfaced / Shipped without a document; trip done … | P2-T18 | todo |
| WM-34 | M1 | Print trip with fiscal step: if the tenant's fiscal pack is on for the pod, … | P2-T18 | todo |
| WM-35 | M1 | Activity log, notifications, performance, verify PDFs, backordered lines views | P2-T18 | todo |
| WM-36 | M1 | Runs server-side as a Temporal workflow per agent (legacy ran in a browser … | P2-T18 | todo |
| WM-40 | M1 | Auto sales-order processing: trip → order → line tree with status (CANCELLED … | P2-T19 | todo |
| WM-41 | M1 | Auto inventory (S2V) processing: pending transactions by trip / order / LID, … | P2-T20 | todo |
| WM-42 | M1 | Pending store transactions grid (by source org) → add to trip, assign picker, … | P2-T08 | todo |
| WM-43 | M1 | Pending shipment lines grid with bulk order-volume calculation (cancellable) | P2-T21 | todo |
| WM-50 | M1 | Vehicles master + dashboard (trips per lorry) — legacy add / edit / delete … | P2-T03 | todo |
| WM-51 | M1 | Pickers master — legacy add / edit / delete were stubs | P2-T03 | todo |
| WM-52 | M1 | Picker monitor: per-day assignments (cards / grid), export, AI summary … | P2-T22 | todo |
| WM-53 | M1 | Picking time monitor: by order / picker / lorry / priority / bay, top … | P2-T22 | todo |
| WM-54 | M1 | Picker / driver PWA (new): my picks, scan to confirm, exceptions, location … | P2-T23 | todo |
| WM-55 | M1 | Route tracker: live picker routes on a warehouse zone map | P2-T23 | todo |
| WM-56 | M1 | Daily history / task mining: timeline, journey replay, repetitive work, … | P2-T24 | todo |
| WM-60 | M1 | On-hand query (org, subinventory, item, lots), organizations, subinventories, … | P2-T25 | todo |
| WM-61 | M1 | Transactions: completed transactions, transaction types, stock movement … | P2-T25 | todo |
| WM-62 | M1 | Receiving (designed, not built): PO receiving, duty-free receiving, customer … | L-T03 | todo |
| WM-63 | M1 | Locators / putaway / guided picking (designed, not built): Fusion locator … | L-T03 | todo |
| SQL-01 | M2 | SQL editor (Monaco) with Fusion schema autocomplete, run against a chosen pod … | P0-T14, P0-T21, P1-T08 | todo |
| SQL-02 | M2 | Runner deployment wizard per pod (create `/Custom/<tenant>/QueryRunner.xdo`), … | P0-T14, P1-T09 | todo |
| SQL-03 | M2 | Schema browser: tables, views, columns, PL/SQL source, dependencies; cached … | P1-T10 | todo |
| SQL-04 | M2 | Result grid: sort, filter, group, pivot, export (Excel, CSV), copy as INSERT, … | P1-T08 | todo |
| SQL-05 | M2 | Saved queries with parameters (`{{P_X}}`), tags, sharing (private / team / … | P1-T11 | todo |
| SQL-06 | M2 | Ask AI: natural-language → SQL agent with read-only tools (search objects / … | P1-T12 | todo |
| SQL-07 | M2 | Knowledge ("it learns your Fusion"): facts TABLE / COLUMN (what ATTRIBUTEn / … | P1-T13 | todo |
| SQL-08 | M2 | Watchdogs: a query on a schedule → one number; rules AUTO (median ± k·MAD by … | P1-T14 | todo |
| SQL-09 | M2 | Format results → dashboard / report (KPIs, insights, charts, formatted table) … | P1-T15 | todo |
| SQL-10 | M2 | Save to dataset: store a result as a tenant table, refreshable (REPLACE / … | P5-T16 | todo |
| SQL-11 | M2 | Flows: process traces for one document (Order-to-Cash, Procure-to-Pay, 65 … | P1-T16 | todo |
| SQL-12 | M2 | Setup checklist: module-wise setup tasks with check SQL (COUNT ≥ min rows), … | P1-T17 | todo |
| SQL-13 | M2 | Call log / inspector: every runner call with timing, request (password … | P1-T08 | todo |
| SQL-14 | M2 | Send to Analytics model (table definition from a query, FULL / INCREMENTAL / … | P5-T16 | todo |
| LD-01 | M3 | FBDI template catalog per Fusion release (sheets = interface tables, columns … | P3-T10 | todo |
| LD-02 | M3 | Template specs per template (types, lengths, required, help, sample row, CSV … | P3-T10 | todo |
| LD-03 | M3 | Prepare & Load wizard: Source (Excel / CSV / paste / tenant SQL / Fusion SQL) … | P3-T11, P3-T12 | todo |
| LD-04 | M3 | Assist: browse source tables and auto-map, download a flat input template … | P3-T12 | todo |
| LD-05 | M3 | Upload + import (new): UCM upload + `ErpIntegrationService.importBulkData`, … | P3-T13 | todo |
| LD-06 | M3 | History: every check / generate / import with files kept; rebuild any ZIP | P3-T14 | todo |
| LD-07 | M3 | Fusion REST loads: catalog of ~50 resources by module × Setup / Masters / … | P3-T15 | todo |
| LD-08 | M3 | FSM setup projects: discover ASM_ objects, map project / task tables, track … | P3-T16 | todo |
| LD-09 | M3 | Setup data analysis: start FSM CSV exports via REST (offering / functional … | P3-T17 | todo |
| LD-10 | M3 | Safety: TEST pod first for bulk loads (policy), dry-run checks mandatory … | P3-T18 | todo |
| OR-01 | M4 | Order pad: customer (F7), items (F6 / scan / `12*code` / scanner words … | P3-T03 | todo |
| OR-02 | M4 | Line maths engine (pure, tested): sign by line type, disc % = customer + … | P3-T01 | todo |
| OR-03 | M4 | Discount engine: rules targeting ITEM / GROUP / SUB_CATEGORY / CATEGORY / … | P3-T01 | todo |
| OR-04 | M4 | Checks → verdict ready / approval / blocked (customer, order type, warehouse, … | P3-T01 | todo |
| OR-05 | M4 | Smart paste: parse pasted text / WhatsApp orders — rules first, then AI … | P3-T05 | todo |
| OR-06 | M4 | "Usual items" per customer (and, later, ML demand forecast) | P3-T06 | todo |
| OR-07 | M4 | Approvals: content-hash-bound (any change after approval needs a new one), … | P3-T02, P3-T07 | todo |
| OR-08 | M4 | Fusion submission via `salesOrdersForOrderHub`: draft / submit, price modes … | P3-T08 | todo |
| OR-09 | M4 | Orders board / list with timeline, copy, credit note (RET at original price), … | P3-T09 | todo |
| OR-10 | M4 | Lookup sources: BIP report + params / Fusion SQL / tenant SQL with … | P3-T04 | todo |
| OR-11 | M4 | Not done in legacy (backlog): Fusion holds, PDA confirm, bulk e-mail, … | L-T04 | todo |
| SCM-01 | M5 | Order Management | P6-T02 | todo |
| SCM-02 | M5 | Purchasing | P6-T03 | todo |
| SCM-03 | M5 | Inventory | P6-T04 | todo |
| SCM-04 | M5 | Costing | P6-T05 | todo |
| SCM-05 | M5 | Setup & diagnostics | P6-T06 | todo |
| SCM-06 | M5 | Shared engine | P6-T01 | todo |
| SCM-07 | M5 | Gaps to close (legacy disabled) | P6-T07 | todo |
| PL-01 | M6 | Connections: Oracle (EZ / TNS / ADB wallet), tenant DB, SQL Server, MySQL, … | P5-T02 | todo |
| PL-02 | M6 | Pipelines with ordered tasks: source (Fusion SQL / tenant SQL / connection … | P5-T01, P5-T03 | todo |
| PL-03 | M6 | Schedules MANUAL / INTERVAL / CRON (tenant time zone) / CONTINUOUS (until … | P5-T04 | todo |
| PL-04 | M6 | Runs: claim, run, cancel (graceful at next page), kill, retry from failed … | P5-T05 | todo |
| PL-05 | M6 | Monitoring: live runs with rows / s, task steps, logs, history, throughput; … | P5-T05 | todo |
| PL-06 | M6 | Pipeline Doctor (Agents): diagnose failed runs, test fix, approval, patch, … | P4-T15 | todo |
| PL-07 | M6 | Runs where data is: cloud workers per region, or Nexora Edge workers inside … | P5-T06 | todo |
| PL-08 | M6 | Later: BICC extracts as source, CDC, dbt transforms after load, data quality … | L-T05 | todo |
| AN-01 | M7 | Model: modules (one storage unit each), tables from sources (Fusion SQL, … | P5-T07 | todo |
| AN-02 | M7 | Versioned publish (build new version, atomic manifest switch), readers on … | P5-T07 | todo |
| AN-03 | M7 | Semantic layer: relationships (active / inactive, single / both), measures in … | P5-T08 | todo |
| AN-04 | M7 | Fusion packs GL / AP / AR / PO / OM / INV: tables, column docs, … | P5-T09 | todo |
| AN-05 | M7 | Reconciliation checks (left vs right measure per group, tolerance) with PASS … | P5-T10 | todo |
| AN-06 | M7 | Reports (values / rows / across / filters, pivot, charts, export) and … | P5-T11 | todo |
| AN-07 | M7 | Ask the model (AI agent with read-only tools: search catalog, describe, … | P5-T12 | todo |
| AN-08 | M7 | Dashboard copilot (AI designs dashboards, checked & repaired) and Quick … | P5-T13 | todo |
| AN-09 | M7 | Open access: REST + MCP server (`/mcp`) with tokens acting as users (roles … | P5-T14 | todo |
| AN-10 | M7 | Power BI: push datasets and embedded reports, report links, Power BI Desktop … | P5-T15 | todo |
| AN-11 | M7 | Storage moves to Parquet on object storage per tenant (DuckDB engine kept), … | P5-T07 | todo |
| AG-01 | M8 | Chat with attachments (xlsx / csv / pdf / docx / images), slash commands, pod … | P4-T05 | todo |
| AG-02 | M8 | Agent loop: tool calls with a round limit and "rounds left" nudges; runs in … | P4-T05 | todo |
| AG-03 | M8 | Tools (as MCP servers in the AI platform): `tenant_query` (read-only typed … | P4-T04 | todo |
| AG-04 | M8 | Interactive UI results: selectable grid with up to 3 actions, editable … | P4-T05 | todo |
| AG-05 | M8 | Trained processes: tenant-defined procedures (stages, data sources, … | P4-T07 | todo |
| AG-06 | M8 | Business knowledge per tenant / pack (e.g. order creation routes, line-cancel … | P4-T08 | todo |
| AG-10 | M8 | Policies per (user or role) × action × pod: AUTO / ASK / DENY + max batch; … | P4-T01, P4-T03 | todo |
| AG-11 | M8 | Approval integrity: the server registers a fingerprint of exactly what the … | P4-T02 | todo |
| AG-12 | M8 | Kill switch per tenant (+ global vendor switch): anyone pauses, admins … | P1-T03 | todo |
| AG-13 | M8 | Inbox: approval requests decided from anywhere (web / mobile / Teams card), … | P4-T06 | todo |
| AG-14 | M8 | Audit + cost per turn and per action (model, tokens incl. cache, USD), … | P4-T01 | todo |
| AG-15 | M8 | Redaction of secrets in prompts, tool I/O and logs; no credentials ever … | P4-T09 | todo |
| AG-20 | M8 | Scheduled jobs (ONCE / RECURRING / REPEAT_UNTIL_DONE with completion query, … | P4-T10 | todo |
| AG-21 | M8 | Daily tasks board: tasks with assignee (person or AI), recurrence, status, … | P4-T11 | todo |
| AG-22 | M8 | Saved reports with parameters, re-run, export, show the exact request | P4-T12 | todo |
| AG-23 | M8 | Curated action catalog (module write APIs with field schemas and pod … | P4-T04 | todo |
| AG-24 | M8 | Forms designer: JSON-defined forms (header fields with cascades, detail grids … | P4-T13 | todo |
| AG-25 | M8 | Agent flows: visual flow designer (Start, AI agent, query, write with … | P4-T14 | todo |
| AG-30 | M8 | Pipeline Doctor | P4-T15 | todo |
| AG-31 | M8 | Shipping agent | P2-T18 | todo |
| AG-32 | M8 | Fusion SQL agent / Analytics agent | P1-T12 | todo |
| AG-33 | M8 | Backlog: Order exception agent, fiscal exception agent (MRA rejects), FBDI … | L-T06 | todo |
| AH-01 | M9 | Providers: Claude in Amazon Bedrock (`AnthropicBedrockMantle`, … | P1-T01, L-T07 | todo |
| AH-02 | M9 | Credentials per tenant: SigV4 keys / bearer API key / workload identity; in … | P1-T01 | todo |
| AH-03 | M9 | Router: per task ordered candidates with fallback on error or refusal; data … | P1-T02, P1-T03 | todo |
| AH-04 | M9 | Usage ledger: per attempt task, provider, model, ok, latency, tokens, cost, … | P1-T01, P1-T04 | todo |
| AH-05 | M9 | Playground: one prompt → up to 6 models side by side with speed / tokens / cost | P1-T05 | todo |
| AH-06 | M9 | Evals: tenant's verified questions (Knowledge EXAMPLES) → each model writes … | P1-T18 | todo |
| AH-07 | M9 | LangChain adapter (`GatewayChatModel`) so any LangChain / LangGraph code uses … | P1-T01 | todo |
| AH-08 | M9 | Tracing (Langfuse / OTel) of every prompt, tool call and cost per tenant | P1-T06 | todo |
| AH-09 | M9 | ML models (new): forecasting (order-pad usual items with quantities, item … | L-T01 | todo |
| AH-10 | M9 | Document AI (new): scanned invoices / delivery notes → structured rows → Load … | L-T02 | todo |
| FP-01 | M10 | Already-sent check (legacy BIP `MRA_TRX_NO_CHECK_BIP`) → ALREADY | P2-T17 | todo |
| FP-02 | M10 | Fetch order summary + details in parallel (legacy BIPs … | P2-T17 | todo |
| FP-03 | M10 | Order-type rule table (interface Y/N) → NOT_REQUIRED | P2-T17 | todo |
| FP-04 | M10 | Line statuses must be CLOSED / AWAIT_BILLING / BILLED / SHIPPED / CANCELED | P2-T17 | todo |
| FP-05 | M10 | Build invoice: STD; CRN for returns (+ reference); PRF pro-forma; currency, … | P2-T17 | todo |
| FP-06 | M10 | Submit to the gateway with 60 s limit; success only with IRN; TIMEOUT vs … | P2-T17 | todo |
| FP-07 | M10 | Write IRN back to the Fusion order header EFF (legacy … | P2-T17 | todo |
| FP-08 | M10 | On / off per pod with mandatory reason and change log; flag shown on trips | P2-T17 | todo |
| FP-09 | M10 | Batch submission (trip, selection, AI action) with live status and retry | P2-T17 | todo |
| FP-10 | M10 | Gateway over HTTPS only (legacy `http://mra.busi.in/MRAInvoice.php` — require … | P2-T17 | todo |

## Open questions

| # | Question | Raised | Answer |
|---|---|---|---|
| 1 | Plan prices and limits for Starter / Professional / Enterprise | RD v0.1 | |
| 2 | Production identity provider: Auth0, Entra External ID or Keycloak | RD v0.1 | **Keycloak, self-hosted** (ADR 0010, 2026-10-02). Login keeps the legacy feel; pod / BU / org chosen right after sign-in; legacy users imported once later |
| 3 | First production region(s): OCI Frankfurt + India / Middle East? | RD v0.1 | |
| 4 | Pipelines engine: Dagster or Temporal-native (decided in P5-T01) | RD v0.1 | |
| 5 | Edge agent language: .NET worker or Go | RD v0.1 | |
| 6 | **.NET 9 support ends 2026-11-10** (STS); .NET 10 is the current LTS. Move ADR 0003 to .NET 10 before P0-T05? Repo is ready: one line (`NexoraTargetFramework` in `Directory.Build.props`) | P0-T01 | **Yes** — ADR 0009, moved to .NET 10 on 2026-10-01 |
| 7 | P0-T24: ORDS export covers the REST layer only. Handlers call PL/SQL not in it (`wms_*` procedures, `P_FUSION_OM_INSERT_LINE`, `RR_SYNC_JOBS_PKG`, `RR_GL_PKG`, `XXAP_*_PKG` …) and ~228 tables / views. Need: APEX › SQL Workshop › Generate DDL (all object types) + BIP catalog archive. Security debts were found in the export; details were given to the user directly and go into `90-legacy-map.md` only once legacy material lives in a private repo (see Q8) | P0-T01 session | |
| 8 | **Urgent — the repository is public.** Branch `legacy/v12` exposes customer names, production / test hostnames (database and Fusion pods) and customer documents; combined with the legacy security debts (Q7) this is an open door to production data. Proposal: move `legacy/v12` to a separate **private** repo (and purge it here), or make this repo private (Actions minutes + CodeQL then need a paid plan); lock down the ORDS endpoints regardless. Do **not** commit the ORDS export here while public | P0-T02 session | |
| 9 | Dev-stack substitutions (decided in P0-T03, please confirm): **SeaweedFS instead of MinIO** — MinIO stopped publishing community images; Nexora uses only the S3 API, prod stays OCI Object Storage. **Redis 8 is AGPL** upstream: fine locally; for production use the cloud's managed cache (OCI Cache) or Valkey — decide before P0-T22 | P0-T03 | |
| 10 | **MediatR is commercially licensed since v13** (RD 03 lists it). The API skeleton does not use it: minimal-API handlers call module services directly (less indirection). Proposal: drop MediatR from the RD stack; FluentValidation (Apache-2.0) stays | P0-T05 | |

## Session log

| Date | Summary | Next |
|---|---|---|
| 2026-10-01 | Repo created: RD v0.1, ADRs 0001–0008, plan for phases 0–6 + later (140 tasks, 163 requirements), `legacy/v12` imported with full history (one password scrubbed) | P0-T01 |
| 2026-10-01 | P0-T01 done: pnpm + Turborepo orchestrating TS, .NET and Python workspaces (`pnpm build/test/lint/typecheck`, `turbo --affected` ready for CI); `@nexora/config` (ESLint, Prettier, tsconfig, commitlint with requirement / task ID rule); `services/api/Nexora.sln` (analyzers as errors, central packages, lock files, xUnit v3 smoke test); `services/ai-platform` uv project (Ruff, mypy strict, pytest); local pre-commit hooks. Received ORDS export for P0-T24 (not committed yet — redaction + OK to write to `legacy/v12` pending) | P0-T02; answer open questions 6, 7 |
| 2026-10-01 | P0-T02 (in progress): `ci.yml` (Turborepo `--affected` build / test / lint / typecheck for TS + .NET 9 + Python, commitlint on commits + PR title, pre-commit hygiene, API image build → Trivy gate + SARIF + CycloneDX SBOM, `CI ok` gate); `security.yml` (CodeQL ×4 languages, gitleaks, Trivy fs vuln / secret / IaC + SBOM, dependency review with copyleft licence deny-list, weekly run, `Security ok` gate); all actions pinned by SHA; Dependabot (actions, npm, NuGet, uv, Docker) with cooldown; `services/api/Dockerfile` (chiseled, non-root, digest-pinned); `.trivyignore.yaml` with expiring accepts; actionlint + zizmor hooks; ruleset JSON for `main`; PR template = Definition of done. Raised Q8 (public repo exposes legacy prod details) | Open PR (user OK) + import ruleset → P0-T02 done; P0-T03; answer Q6–Q8 |
| 2026-10-01 | Moved to **.NET 10 LTS** (user OK; ADR 0009 supersedes the version in ADR 0003; RD, CLAUDE.md, README updated): `net10.0`, `global.json` 10.0 (`latestFeature`), Mvc.Testing 10.0.12, SDK / chiseled runtime images 10.0 pinned by digest, CI uses `global.json`; lock files refreshed; build, tests, image smoke run and Trivy scans pass | Open PR (user OK) + import ruleset → P0-T02 done; P0-T03; Q7, Q8 |
| 2026-10-02 | P0-T03 done: `infra/compose` (Oracle 23ai Free, Keycloak 26 realm with organisations = tenants, Temporal dev server + UI, Redpanda + console, Redis, SeaweedFS S3, Vault dev, Grafana LGTM, Mailpit), all on 127.0.0.1, random per-machine creds in gitignored `.env`; idempotent seed (Vault transit key per tenant + least-privilege policy, S3 bucket); `make up / smoke / check / down / reset` + `pnpm stack:*`; `Dev stack` workflow + Dependabot for compose images. Raised Q9 | P0-T04; still: PR + ruleset (P0-T02), Q7–Q9 |
| 2026-10-02 | P0-T04 done: `db/migrations` — bootstrap (as admin, idempotent) creates module schemas as Oracle 23ai schema-only accounts (no password) reachable only via `nexora_migrator[<schema>]`; `platform/changelog.yaml` with `tenant` registry (slug / status checks); `migrate.sh` update / status / rollback-test (rolls back everything, verifies, re-applies); `make db-*`, `make up` migrates; CI job on a throwaway Oracle with per-run random passwords; `@nexora/db` convention checker (rule 1: every table has `tenant_id VARCHAR2(36) NOT NULL` unless `[tenant-exempt: why]`). Compose Oracle app user renamed to `nexora_migrator` (run `make reset` once) | P0-T05; still: PR + ruleset (P0-T02), Q7–Q9 |
| 2026-10-02 | P0-T05 done: `services/api` modular monolith — `IModule` + explicit registration, `/api/v1/<module>` groups, Platform module + Contracts; BuildingBlocks: FluentValidation filter, RFC 9457 problem details with traceId, idempotency middleware (tenant-scoped keys, replay / 422 / 409, 5xx not stored; Redis `SET NX` store or in-memory), token-bucket rate limiting with 429 problem, `/health/live` + `/health/ready` (Redis), OpenTelemetry traces / metrics / logs over OTLP; OpenAPI 3.1 generated at build into `openapi/v1.json` (CI drift check), Scalar docs at `/docs` in Development. Tests: 29 (integration via a test module, Redis store contract on Testcontainers, endpoint-convention tests, NetArchTest module boundaries — mutation-checked). `/health/ready` trace found in Tempo. Raised Q10 (MediatR licence) | P0-T06; still: PR + ruleset (P0-T02), Q7–Q10 |
| 2026-10-02 | P0-T06 done (identity). User chose Keycloak as production IdP (ADR 0010; Q2 answered). Realm: `sub` / `acr` scopes (sub was missing since P0-T03 — fixed), audience `nexora-api`, `tenants` claim with organisation **id** = tenant_id, fixed issuer `localhost:8180`, `nexora-api` service account, brute-force lockout, password policy, SMTP → Mailpit, MFA step-up (LoA pwd / mfa, OTP enrolment on first use), invitation-only registration, `nexora` login theme (legacy fields). API: JWT validation, tenant from token + `X-Nexora-Tenant` for multi-tenant users (non-member → 403), policies tenant-member / tenant-admin / platform-admin, RFC 9470 step-up challenge, tenant + user on traces / logs, rate limits and idempotency per tenant, `GET /me`, `POST /invitations` via resilient Keycloak client. 50 tests (signed test tokens, real Keycloak on Testcontainers, convention: every write names a policy). Verified live: login page, step-up → OTP setup, invite e-mail in Mailpit, foreign tenant 403. Not yet verified: lockout. Deferred: user profile sync (P0-T07), legacy user import (needs user export), pod / BU / org picker (P0-T18) | P0-T07; still: PR + ruleset (P0-T02), Q7–Q10 |
