# M3 — Nexora Load (data into Fusion + implementation tracking)

Purpose: get data into Fusion right the first time (FBDI and REST), and know the real state of the Fusion setup.

| ID | Requirement | Notes / acceptance |
|---|---|---|
| LD-01 | FBDI template catalog per Fusion release (sheets = interface tables, columns in CSV order, required flags, import process, UCM account); download official .xlsm (zip-signature check), detect newer quarterly releases | Legacy `dataload/fbdi-catalog.js` is **generated** from Oracle's 26C .xlsm files — keep the generator |
| LD-02 | Template specs per template (types, lengths, required, help, sample row, CSV names, END column flag), loaded on demand; 45 supported, 4 known unsupported | Legacy `dataload/fbdi-specs.js`, `dataload/specs/*.js` |
| LD-03 | **Prepare & Load** wizard: Source (Excel / CSV / paste / tenant SQL / Fusion SQL) → Map (one expression per column: constants, `{Col|date|num|dr|cr|map:…}`, `{#doc}`, `{#line}`, `{#sum:Col}`, `{#load}`) → Check (types, lengths, required, rules, live Fusion lookups) → Generate (CSV exactly like Oracle's macro: no header, END column, CRLF, UTF-8 no BOM, YYYY/MM/DD; zipped) | Legacy engine `dataload/fbdi-engine.js` (pure) + rules `fbdi-rules.js` (hand-written for Journals, AP Invoices, Inventory Transactions; auto rules for the rest) |
| LD-04 | Assist: browse source tables and auto-map, download a flat **input template** (one sheet, document key, hidden mapping sheet recognised on upload), "Prepare FBDI" fixes required gaps | Legacy `dataload/prepare-assist.js` |
| LD-05 | **Upload + import** (new): UCM upload + `ErpIntegrationService.importBulkData`, ESS job monitoring, load-request results, error rows back into the load with fixes | Legacy roadmap "next phase" |
| LD-06 | History: every check / generate / import with files kept; rebuild any ZIP | Legacy WMS_FBDI_* tables |
| LD-07 | **Fusion REST loads**: catalog of ~50 resources by module × Setup / Masters / Transactions / Integration with FBDI alternative; probe availability; fields from `/describe`; sample records with drill-down links; map rows (same expressions), group child collections, validate, POST / PATCH (parallel 1–4, stop after N errors, retry rejected) | Legacy `dataload/fapi*.js`; writes only GET / POST / PATCH to `{fscm|hcm|crm}RestApi/resources` |
| LD-08 | **FSM setup projects**: discover ASM_ objects, map project / task tables, track task status, status events and burn-down snapshots per project | Legacy `dataload/fsm.js`, WMS_FSM_* |
| LD-09 | **Setup data analysis**: start FSM CSV exports via REST (offering / functional area / task), poll, download, count rows per business object (configured vs empty), match to tasks ("completed but no rows"), compare two exports | Legacy `dataload/fsm-export.js` |
| LD-10 | Safety: TEST pod first for bulk loads (policy), dry-run checks mandatory before import, approvals for PROD imports above a threshold | New |

Legacy references: `dataload/*` (58 files), `classes/Form1_DataLoadHandlers.cs`, `apex_sql/70–72`.
