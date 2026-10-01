# ADR 0005 — Temporal for long-running and human-in-the-loop workflows
Status: accepted.
Decision: print trip, shipping agent, fiscal batches, FBDI imports, provisioning, approvals and scheduled jobs are
Temporal workflows (durable, retryable, visible). LangGraph keeps agent reasoning state; Temporal owns business
side effects.
Consequences: no browser-tab or per-PC schedulers; one place to see every running process.
