# M2 — Nexora SQL (Fusion data workbench)

Purpose: answer any question from Fusion data in minutes, safely (read-only), and let the company's knowledge accumulate.

## Features
| ID | Requirement | Notes / acceptance |
|---|---|---|
| SQL-01 | SQL editor (Monaco) with Fusion schema autocomplete, run against a chosen pod through the **BI Publisher runner** (one deployed report runs base64 SQL via DBMS_XMLGEN; XML first, CSV fallback; row cap) | Only SELECT / WITH; inline PL/SQL rejected (legacy `FusionSqlService.ValidateStatement`) |
| SQL-02 | Runner deployment wizard per pod (create `/Custom/<tenant>/QueryRunner.xdo`), health check | Legacy deploys from the app |
| SQL-03 | Schema browser: tables, views, columns, PL/SQL source, dependencies; cached per pod | Legacy SQLite cache `FusionSqlExtras.cs` |
| SQL-04 | Result grid: sort, filter, group, pivot, export (Excel, CSV), copy as INSERT, charts | |
| SQL-05 | Saved queries with parameters (`{{P_X}}`), tags, sharing (private / team / tenant), versions | Legacy WMS_FUSION_SQL_QUERIES |
| SQL-06 | **Ask AI**: natural-language → SQL agent with read-only tools (search objects / columns, describe, source, dependencies, sample rows), streams steps, returns SQL + explanation | Legacy `FusionSqlAgent.cs`; uses AI platform task `fusion_sql` |
| SQL-07 | **Knowledge** ("it learns your Fusion"): facts TABLE / COLUMN (what ATTRIBUTEn / SEGMENTn hold) / VALUE / JOIN / RULE / TERM / EXAMPLE with status PROPOSED → APPROVED / REJECTED and confidence; learned from saved queries, AI proposals, 👍 Correct; injected into Ask AI (relevance selection with budget) | Legacy `fusionsql/knowledge*.js`, WMS_FUSION_KNOWLEDGE |
| SQL-08 | **Watchdogs**: a query on a schedule → one number; rules AUTO (median ± k·MAD by weekday/hour, learning period, sensitivity, direction), ABOVE / BELOW / CHANGE %; alerts with cool-down to Teams / e-mail; sparkline with normal band | Legacy `fusionsql/watch-engine.js` (pure, tested). **Runs server-side** (Temporal schedule), not in an open browser tab |
| SQL-09 | **Format results** → dashboard / report (KPIs, insights, charts, formatted table) with Excel / PDF / HTML / PNG export and share (Outlook draft, rich copy) | Legacy `fusionsql/report.js` |
| SQL-10 | **Save to dataset**: store a result as a tenant table, refreshable (REPLACE / APPEND) with its SQL + params | Legacy WMS_FUSION_SQL_DATASETS → becomes an Analytics dataset |
| SQL-11 | **Flows**: process traces for one document (Order-to-Cash, Procure-to-Pay, 65 standard processes incl. 25 period reconciliations), steps pass keys (`IN ({{KEY}})`), diagram shows rows per step and where it stops; flow report with headline figures; AI builds flows from the library | Legacy `fusionsql/flows*.js` |
| SQL-12 | **Setup checklist**: module-wise setup tasks with check SQL (COUNT ≥ min rows), results per pod, drill-down | Legacy `fusionsql/setups*.js`, WMS_FUSION_SETUP_* |
| SQL-13 | Call log / inspector: every runner call with timing, request (password redacted), response size | |
| SQL-14 | Send to Analytics model (table definition from a query, FULL / INCREMENTAL / WINDOW) and Add to pipeline | Legacy `fusionsql/tomodel.js`, "Add to pipeline" |

## Data
Tenant tables: saved_queries, knowledge_facts, watchdogs, watch_runs, flows, flow_steps, flow_runs, setup_tasks,
setup_results, datasets. All tenant-scoped; pod-scoped where results depend on a pod.

## Integrations
Fusion connector (BIP runner, REST for describe), AI platform (`fusion_sql`), notifications, Analytics, Pipelines.

## Product changes vs legacy
- Credentials per tenant pod in Vault (legacy: app credentials fetched from an APEX endpoint, DPAPI per PC).
- Watchdog scheduler and lease logic replaced by server-side schedules.
- Report folder `/Custom/GraysWMS/` → per-tenant folder name setting.

Legacy references: `fusionsql/*` (page + 16 scripts), `classes/FusionSqlService.cs`, `FusionSqlExtras.cs`,
`FusionSqlAgent.cs`, `Form1_FusionSqlHandlers.cs`, `apex_sql/63–65, 68, 80`, `docs/Fusion_SQL_Technical_RD.md`.
