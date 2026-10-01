# Phase 6 — SCM Workbench + commercial launch

**Exit criteria:** three paying tenants on self-service plans; SCM Workbench used daily at the pilot; billing, trials
and support processes run without engineering help; SOC 2 readiness assessment passed.

| Task | What | Covers | Depends | Done when |
|---|---|---|---|---|
| P6-T01 | Shared SCM screen engine (grid → `q`, paging, KPIs, drawer, forms, LOV / typeahead, pod switch) on the design system | SCM-06 | P0 | |
| P6-T02 | Order Management workbench screens | SCM-01 | T01 | |
| P6-T03 | Purchasing screens | SCM-02 | T01 | |
| P6-T04 | Inventory screens | SCM-03 | T01 | |
| P6-T05 | Costing screens | SCM-04 | T01 | |
| P6-T06 | Setup & diagnostics screens | SCM-05 | T01 | |
| P6-T07 | Close legacy gaps (DELETE operations with approval, SOAP on-hand load, login history) | SCM-07 | T02–T05 | |
| P6-T08 | **Billing**: Stripe Billing (plans, metered prices from usage aggregates, invoices, dunning) + AWS / Azure Marketplace listing | PC-12 | P1-T04 | test-mode subscription lifecycle E2E |
| P6-T09 | **Self-service sign-up + trial** (provisioning workflow, sample-data tenant, onboarding wizard incl. first pod) | PC-01 | T08 | stranger signs up and runs a Fusion query in < 15 min |
| P6-T10 | Vendor console full (tenants, plans, health, consented support access, usage, revenue) | PC-14 | T08 | |
| P6-T11 | Second fiscal pack (e.g. India IRP or KSA ZATCA) to prove the pack interface | M10 | P2-T17 | |
| P6-T12 | Compliance: SOC 2 controls evidence, pen test, DPA / privacy, data residency regions | NFR compliance | — | readiness report |
| P6-T13 | Self-hosted edition (Helm chart + signed licences, port `Licensing.cs`) | 02 §2.7 | P0 | install in a clean cluster with a licence |
