# Database migrations (Liquibase, Oracle 23ai)

ADR 0007: schema changes only through these changelogs.

```sh
make db-migrate         # bootstrap + every module schema   (pnpm db:migrate)
make db-status          # pending changesets                (pnpm db:status)
make db-rollback-test   # migrate → roll back ALL → migrate (destructive; dev / CI only)
pnpm --filter @nexora/db lint   # convention checker (also runs in CI)
```

`make up` applies migrations automatically.

## Layout and security model
- `bootstrap/changelog.yaml` runs as the DB administrator (SYSTEM locally, ADMIN on Autonomous DB). It creates every
  module schema as an Oracle 23ai **schema-only account** — no password, nobody can log in as it — and lets only the
  migrator reach it by proxy.
- `<schema>/changelog.yaml` runs as `nexora_migrator[<schema>]` (proxy connection), so objects and the
  `DATABASECHANGELOG` table land in that schema. Order of schemas: `schemas.txt`.
- Runtime (application) users get DML-only grants plus VPD policies in P0-T07; they never own objects.

## Conventions (enforced by `tools/check-changelogs.mjs`)
- changeSet id `NNNN-kebab-case`, unique per file; author set; one logical change per changeSet.
- Every table has `tenant_id VARCHAR2(36) NOT NULL` (rule 1). The rare exception declares itself in the table remarks:
  `[tenant-exempt: <reason>]` — the reason then shows up as the table comment in the database.
- Raw `sql` changes carry an explicit `rollback`; bootstrap changeSets are idempotent (`onFail: MARK_RAN`).
- Never edit a changeSet that has reached `main`; add a new one.

## Adding a module schema
1. Add the schema name to `schemas.txt`.
2. Add a `0NNN-schema-<name>` changeSet to `bootstrap/changelog.yaml` (copy the platform one).
3. Create `<name>/changelog.yaml`.
