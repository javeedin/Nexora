# ADR 0007 — Oracle Database as system of record, Liquibase migrations
Status: accepted.
Decision: Oracle 23ai for platform and module data (customers already run Oracle; JSON + vector search available);
schema changes only through Liquibase changelogs in `db/migrations`; analytics data in Parquet / DuckDB.
Consequences: no tables created by pages at runtime (legacy pattern); environments reproducible.
