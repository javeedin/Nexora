# 4. Non-functional requirements

| Area | Requirement |
|---|---|
| **Availability** | 99.9 % monthly per region (Enterprise 99.95 %); planned maintenance announced 72 h ahead; no single-AZ dependency |
| **Performance** | Web p95 < 300 ms for API reads, < 1 s for list screens up to 10 k rows (server paging); Nexora SQL adds < 200 ms over the Fusion runner; print job dispatched to Edge < 2 s |
| **Scale (year 1)** | 50 tenants, 2,000 users, 5 k trips / day, 500 k pipeline rows / hour per tenant, 50 concurrent agent runs |
| **Security** | OWASP ASVS L2; OIDC + MFA; least-privilege RBAC; per-tenant encryption keys; TLS 1.2+ everywhere (no plain HTTP — legacy MRA gateway call must go through an HTTPS-terminating proxy or the vendor's HTTPS endpoint); secrets only in Vault; dependency + container scanning; yearly pen test |
| **Compliance** | SOC 2 Type II and ISO 27001-ready controls; GDPR (export, erasure, DPA); data residency per region; audit log immutable (WORM) and retained 7 years for fiscal events |
| **AI safety** | Kill switch per tenant and global; every acting step policy-checked (AUTO / ASK / DENY), approvals bound to exactly what was shown (fingerprint, single use, expiry); prompt and tool I/O redaction of secrets; per-tenant budgets; evals gate prompt / model changes; no customer data used for training |
| **Fusion safety** | Read-only by default; write scopes per pod; dry-run / TEST-pod-first for bulk loads; rate limits per pod; every Fusion write audited with request + response |
| **Reliability** | Workflows resumable (Temporal); at-least-once events with idempotent consumers; RPO 15 min, RTO 4 h (Enterprise RPO 5 min / RTO 1 h) |
| **Observability** | Traces, metrics, logs with tenant_id; SLO dashboards; alerting to on-call; per-tenant health page in admin console |
| **Usability** | Responsive (desktop, tablet, phone); WCAG 2.2 AA; keyboard-first for order desk and SQL; dark mode; English first, i18n-ready (French next) |
| **Offline** | Picker / driver PWA queues actions offline and syncs; Edge buffers print jobs during outages |
| **Supportability** | Feature flags; per-tenant debug logging switch; support impersonation with consent + audit; in-app status page |
| **Portability** | Containers only; Terraform modules for OCI first, Azure / AWS next; self-hosted Helm chart |
| **Testing** | ≥ 80 % unit coverage on domain logic (pricing, discounts, FBDI rules, semantic compiler, watchdog baselines); contract tests for Fusion connector; E2E for top 20 journeys; AI evals per agent |
