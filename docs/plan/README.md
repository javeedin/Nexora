# Nexora — development plan

This folder turns the RD (`docs/rd/`) into ordered, checkable work. **Every Claude session works from here.**

| File | What |
|---|---|
| [`PROGRESS.md`](PROGRESS.md) | Status of every requirement ID and every plan task — the single source of "where are we" |
| [`phase-0-foundation.md`](phase-0-foundation.md) | Monorepo, CI/CD, dev stack, platform core (M0), Fusion connector v0, app shell |
| [`phase-1-sql-aihub.md`](phase-1-sql-aihub.md) | Nexora SQL (M2) + AI Hub gateway (M9) + notifications + usage metering |
| [`phase-2-wms-edge.md`](phase-2-wms-edge.md) | Nexora Edge, WMS (M1), MRA fiscal pack (M10), picker PWA |
| [`phase-3-orders-load.md`](phase-3-orders-load.md) | Orders (M4) + Load / FBDI / REST / FSM (M3) |
| [`phase-4-agents.md`](phase-4-agents.md) | Agents (M8): chat agent, action gateway, policies, approvals, inbox, jobs, tasks, forms, flows, Pipeline Doctor |
| [`phase-5-pipelines-analytics.md`](phase-5-pipelines-analytics.md) | Pipelines (M6) + Analytics (M7) |
| [`phase-6-scm-commercial.md`](phase-6-scm-commercial.md) | SCM Workbench (M5), billing, self-service sign-up, vendor console, compliance |
| [`phase-later.md`](phase-later.md) | ML models, document AI, receiving / locators, more packs and regions |

## How a session works (mandatory)
1. Read `CLAUDE.md`, then `PROGRESS.md`. Find the **current phase** and the **first task that is not done** whose
   dependencies are done. Do that task (or the one the user names).
2. Before coding a task, read its requirement IDs in `docs/rd/modules/*` and the legacy pointers
   (`docs/rd/90-legacy-map.md`, branch `legacy/v12`).
3. Work on a branch `feat/<task-id>-<short-name>` (e.g. `feat/P0-T07-tenancy`); small commits referencing the task and
   requirement IDs; open a PR into `main` when the task's "Done when" is met (CI green).
4. In the **same PR**, update `PROGRESS.md` (task + requirement statuses, PR link, notes) and tick the task in the phase file.
5. A phase is finished only when its **exit criteria** are met and checked in `PROGRESS.md`. Do not start the next phase's
   tasks before that unless the user says so.
6. If a requirement is unclear or the RD seems wrong: write the question in `PROGRESS.md › Open questions` and ask the user;
   do not silently change the RD or an ADR.
7. End every session by updating `PROGRESS.md › Session log` (date, what was done, what is next).

## Definition of done (every task)
- Code, tests (unit for domain logic, integration with Testcontainers where a DB / queue / workflow is involved, E2E for
  user-visible journeys, evals for AI behaviour) and docs updated; CI green (build, lint, tests, CodeQL, Trivy, gitleaks).
- Tenant isolation covered by a test (a second tenant cannot see / change the data).
- Permissions and entitlements enforced server-side; audit events written for writes and external calls.
- No customer-specific value in code; new settings have a schema, defaults and UI.
- Observability: traces / metrics / structured logs with `tenant_id` for new endpoints and workers.
- OpenAPI updated and the TS client regenerated; migrations added (Liquibase) and reversible where possible.
- Feature flag for anything user-visible that is not finished.

## Status values
`todo` · `in-progress` · `blocked` (say why) · `done` (with PR link) · `dropped` (with reason, needs user approval)
