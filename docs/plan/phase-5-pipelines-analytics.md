# Phase 5 — Pipelines + Analytics

**Exit criteria:** the pilot's Fusion data syncs incrementally into its analytics store on schedules with monitoring;
the semantic model with Fusion packs answers dashboards, Power BI and AI questions per tenant.

| Task | What | Covers | Depends | Done when |
|---|---|---|---|---|
| P5-T01 | Decide Dagster vs Temporal-native runner (ADR); port legacy `pipeline-server` connectors / runner / tests | PL-02 | P0 | ADR accepted; ported tests green |
| P5-T02 | Connections (Oracle EZ / TNS / ADB wallet, SQL Server, MySQL, PostgreSQL, DuckDB / Parquet, REST) with Vault secrets + test | PL-01 | T01 | |
| P5-T03 | Pipelines + tasks + modes (APPEND / TRUNCATE_INSERT / MERGE / INCREMENTAL), column map, auto-create targets | PL-02 | T01 | |
| P5-T04 | Schedules (MANUAL / INTERVAL / CRON / CONTINUOUS), dependencies, on_error, timeouts | PL-03 | T03 | |
| P5-T05 | Runs: claim, cancel, kill, retry from failed task, watermark per page, resume after restart; monitoring UI (live runs, logs, throughput, engine controls) | PL-04, PL-05 | T04 | |
| P5-T06 | Workers in cloud regions and on Edge (private networks) | PL-07 | T05, P2-T02 | |
| P5-T07 | **Analytics engine**: port `engine/FusionModel` (storage → Parquet per tenant, versioned publish, sandboxed queries) with its xUnit tests | AN-01, AN-02, AN-11 | P0 | ported tests green |
| P5-T08 | Semantic layer (relationships, DAX-compatible measures, calendar, hierarchies, roles) + model studio UI | AN-03 | T07 | MeasureTests parity |
| P5-T09 | Fusion packs GL / AP / AR / PO / OM / INV via packs registry; check on pod | AN-04 | T08, P2-T01 | |
| P5-T10 | Reconciliation checks | AN-05 | T08 | |
| P5-T11 | Reports + dashboards (canvas, visuals, cross-filter, slicers, editor) | AN-06 | T08 | |
| P5-T12 | Ask the model (catalog, hybrid search, agent tools), verified examples, glossary | AN-07 | T08, P1-T03 | |
| P5-T13 | Dashboard copilot + quick dashboard | AN-08 | T11, T12 | |
| P5-T14 | REST + MCP access with tokens acting as users | AN-09 | T08 | |
| P5-T15 | Power BI (push datasets, embedded reports, report links, Desktop feed with hashed keys) | AN-10 | T08 | |
| P5-T16 | Save SQL result as dataset; send SQL to model; add SQL to pipeline | SQL-10, SQL-14 | T03, T07, P1-T08 | |
