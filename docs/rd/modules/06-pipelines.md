# M6 — Nexora Pipelines

Purpose: move Fusion / database data to where it is needed, incrementally and observably.

| ID | Requirement | Notes |
|---|---|---|
| PL-01 | Connections: Oracle (EZ / TNS / ADB wallet), tenant DB, SQL Server, MySQL, PostgreSQL, DuckDB / Parquet, REST endpoint; secrets in Vault (legacy: RSA-OAEP encrypted in the page with the server's key) | |
| PL-02 | Pipelines with ordered tasks: source (Fusion SQL / tenant SQL / connection SQL with `{{P_X}}` / `{{WATERMARK}}`) → target object; modes APPEND / TRUNCATE_INSERT / MERGE (keys) / INCREMENTAL (watermark + overlap); column map (`"SRC": "TGT:TYPE" / "SKIP"`); target auto-create with inferred types | Legacy `pipeline-server/pipeline_server/connectors.py`, `runner.py` |
| PL-03 | Schedules MANUAL / INTERVAL / CRON (tenant time zone) / CONTINUOUS (until cancelled); ON/OFF; depends_on; on_error STOP / CONTINUE; timeouts | Legacy `engine.py` Supervisor |
| PL-04 | Runs: claim, run, cancel (graceful at next page), kill, retry from failed task; watermark saved after every written page; re-queue after restart | |
| PL-05 | Monitoring: live runs with rows / s, task steps, logs, history, throughput; engine Pause / Resume / Drain / Stop | Legacy NiceGUI console + app server bar |
| PL-06 | **Pipeline Doctor** (Agents): diagnose failed runs, test fix, approval, patch, re-run | See M8 |
| PL-07 | Runs where data is: cloud workers per region, or **Nexora Edge** workers inside the customer network | New |
| PL-08 | Later: BICC extracts as source, CDC, dbt transforms after load, data quality checks | |

Implementation: Dagster or Temporal workers (decide in ADR), evolved from `pipeline-server/` (Python, tested: fake Fusion
pod, DuckDB loads, scheduler, API). Legacy references: `pipeline-server/*`, `fusionsql/pipelines.js`,
`fusionsql/pipeline-setup.js`, `apex_sql/69, 81`.
