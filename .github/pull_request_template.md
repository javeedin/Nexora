## What and why

<!-- Task and requirement IDs, e.g. P0-T07 · PC-01. One or two sentences on the change. -->

## Definition of done (`docs/plan/README.md`)

- [ ] Tests: unit for domain logic; integration (Testcontainers) for DB / queue / workflow; E2E for user journeys; evals for AI
- [ ] Tenant isolation covered by a test (tenant B cannot see / change tenant A data)
- [ ] Permissions + entitlements enforced server-side; audit events for writes and external calls
- [ ] No customer-specific values in code; new settings have schema, defaults and UI
- [ ] Traces / metrics / logs carry `tenant_id`
- [ ] OpenAPI updated + TS client regenerated; Liquibase migrations added (reversible where possible)
- [ ] Unfinished user-visible work behind a feature flag
- [ ] `docs/plan/PROGRESS.md` + phase file updated in this PR

## Notes for reviewers

<!-- Risky parts, follow-ups, screenshots. -->
