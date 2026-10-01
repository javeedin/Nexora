# ADR 0002 — Multi-tenant SaaS with isolation tiers
Status: accepted.
Context: the product serves many Oracle Fusion customers; the legacy system was single-customer with hard-coded values.
Decision: one platform, tenants isolated by `tenant_id` + row-level policies (pooled), schema-per-tenant or dedicated DB
by plan; customer / country logic as packs; self-hosted option with signed licences.
Consequences: tenant context everywhere from day one; tests for tenant filters; per-tenant keys, limits and budgets.
