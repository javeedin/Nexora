# M7 — Nexora Analytics (semantic model on Fusion data)

Purpose: one governed, fast dataset of Fusion + operational data with DAX-compatible measures, dashboards and AI Q&A.

| ID | Requirement | Notes |
|---|---|---|
| AN-01 | Model: modules (one storage unit each), tables from sources (Fusion SQL, tenant DB, files, BICC), load strategies FULL / INCREMENTAL (key + changed-since + overlap) / WINDOW (last N months), keyset paging, resumable loads, schema drift detection | Legacy `engine/FusionModel/` (C#, xUnit-tested) |
| AN-02 | Versioned publish (build new version, atomic manifest switch), readers on read-only snapshots, query sandbox (no external access, one SELECT) | |
| AN-03 | Semantic layer: relationships (active / inactive, single / both), measures in DAX-compatible text, calendar with fiscal year, hierarchies, roles (row filters applied everywhere incl. AI) | Legacy `Semantic/Parser.cs`, `Compiler.cs`, `QueryEngine.cs` — CALCULATE, FILTER, iterators with context transition, time intelligence |
| AN-04 | Fusion packs GL / AP / AR / PO / OM / INV: tables, column docs, relationships, 46 measures, glossary, reconciliation checks; "check on pod" probe | Legacy `Packs/fusion-packs.json` → becomes Packs in the registry |
| AN-05 | Reconciliation checks (left vs right measure per group, tolerance) with PASS / FAIL details | |
| AN-06 | Reports (values / rows / across / filters, pivot, charts, export) and **Dashboards** (24-column canvas, 15 visual types, cross-filter, slicers, phone layout, editor with undo / redo) | Legacy `fusionmodel/dashboards.js`, `dash-edit.js` |
| AN-07 | Ask the model (AI agent with read-only tools: search catalog, describe, evaluate, lookup values, run_sql), verified examples, glossary; hybrid search (BM25 + fuzzy + glossary + values + vectors) | Legacy `Ai/Catalog.cs`, `ModelTools.cs`, `ModelAskAgent.cs` |
| AN-08 | Dashboard copilot (AI designs dashboards, checked & repaired) and Quick dashboard without AI | Legacy `Ai/DashboardCopilot.cs` |
| AN-09 | Open access: REST + MCP server (`/mcp`) with tokens acting as users (roles applied) | Legacy `engine/FusionModel.Server`, `FusionModel.Mcp` |
| AN-10 | Power BI: push datasets and embedded reports, report links, Power BI Desktop feed (Power Query `WmsFeed`, DAX measures, relationships) with hashed feed keys | Legacy `powerbi/*`, `PowerBiService.cs`, `apex_sql/76, 77` |
| AN-11 | Storage moves to Parquet on object storage per tenant (DuckDB engine kept), refresh orchestrated by workflows instead of a "refresher PC" | |

Legacy references: `engine/FusionModel*` (engine, tests, server, MCP), `fusionmodel/*`, `classes/Form1_ModelHandlers.cs`,
`powerbi/*`.
