# 2. Subscription, tenancy and licensing

## 2.1 Tenancy model
| Term | Meaning |
|---|---|
| **Tenant** | One customer company. Owns users, settings, Fusion connections (pods), data, AI policies and usage. |
| **Pod / instance** | One Oracle Fusion environment of a tenant (PROD, TEST, DEV …). A tenant has 1..n pods. |
| **Site** | A warehouse / branch inside a tenant (printers, pickers, trips belong to a site). |
| **Business unit** | Fusion BU — mapped per tenant (legacy had them hard-coded). |

**Isolation tiers**
| Tier | Storage | For |
|---|---|---|
| Pooled | Shared database, every row carries `tenant_id`, Oracle VPD / row-level policies + app-level checks | Starter / Professional |
| Schema | One schema per tenant in a shared database | Professional with data-residency needs |
| Dedicated | Own database (and optionally own deployment / cloud account) | Enterprise, regulated |

Rules: `tenant_id` on every table, cache key, file path, queue message, log line, trace and AI prompt; no query without a
tenant filter (enforced in the data layer and by tests); per-tenant encryption keys for secrets; per-tenant rate limits
and AI budgets; tenant export and deletion (GDPR) built in.

## 2.2 Plans and entitlements
| Plan | Modules | Limits (defaults) |
|---|---|---|
| **Starter** | SQL (+ Knowledge, Watchdogs), AI Hub (gateway only) | 5 users, 1 pod, 1 M AI tokens / month |
| **Professional** | + WMS, Orders, Load, SCM Workbench, Pipelines (5) | 25 users, 2 pods, 5 M tokens, 50 M pipeline rows |
| **Enterprise** | + Agents, Analytics, unlimited pipelines, SSO/SCIM, dedicated tier option, self-hosted option, SLA | negotiated |
| **Add-ons** | extra users, pods, AI tokens, pipeline rows, country packs (e.g. MRA), premium support | — |

**Entitlement service**: per tenant `{modules: {...}, limits: {...}, features: {...}, packs: [...]}` resolved from plan +
add-ons + overrides; checked at the API gateway (module access), in services (limits) and in the UI (hide / upsell).
Feature flags (Unleash) for gradual rollouts are separate from entitlements.

## 2.3 Metering and billing
- Metered events: `ai.tokens` (by model + provider, cost), `fusion.query`, `pipeline.rows`, `load.records`, `docs.pages`,
  `users.active`. Emitted to a usage pipeline (OpenMeter or own table) → aggregated daily → **Stripe Billing**
  (subscriptions, metered prices, invoices, dunning) or **AWS / Azure Marketplace** private offers for enterprise.
- Budgets and hard caps per tenant (from the AI Hub budget concept); alerts at 80 / 100 %.
- Trials: 30 days, Starter features, sample data tenant.

## 2.4 Provisioning (self-service sign-up)
Temporal workflow: sign-up → verify email → create tenant → choose region → create schema / DB → seed defaults (roles,
settings, sample dashboards) → create admin user in the IdP organisation → connect first Fusion pod (wizard with test) →
welcome. Deprovisioning: suspend → export → delete after retention.

## 2.5 Identity and access
- **IdP**: Auth0 / Entra External ID / Keycloak, one *organisation* per tenant; SSO with the customer's Entra ID / Okta /
  Google; SCIM user provisioning (Enterprise).
- **RBAC + ABAC**: roles per module (viewer, user, approver, admin) + attributes (site, BU, pod). Policy engine **OPA**
  for AI actions and approvals (legacy: WMS_AI_POLICIES).
- Platform staff access to tenant data only through break-glass with reason + audit.

## 2.6 On-prem connector ("Nexora Edge")
A small signed agent (Windows service / Linux daemon) the customer installs where the cloud cannot reach:
local / network **printers**, local files and scanners, **private Fusion or database networks**, legacy systems.
Outbound-only (WebSocket / gRPC over TLS to the tenant's endpoint), auto-update, health in the admin console.
Replaces the legacy desktop app's local duties (printing, C:\fusion files, mobile listener).

## 2.7 Self-hosted licensing
Same container images deployed in the customer's cloud. Signed licence file: ECDSA P-256 over the canonical JSON of
`{customer, edition, modules, packs, max_users, pods, expiry}` (the legacy Fusion Model already implements this —
`engine/FusionModel/Licensing/Licensing.cs` on `legacy/v12`); vendor private key offline; 30-day grace on expiry; never
blocks reading of existing data.

## 2.8 Country / industry packs (plugins)
Customer- or country-specific logic is packaged, versioned and switched on per tenant:
- **Fiscal packs**: Mauritius MRA e-invoicing (legacy `classes/MRAProcessor.cs`), later others (e.g. India e-invoice, KSA ZATCA).
- **Fusion analytics packs**: GL, AP, AR, PO, OM, INV (legacy `engine/FusionModel/Packs/fusion-packs.json`).
- **Pricing / discount rule packs** (legacy order-pad discount engine `om/om-engine.js`).
- **Setup checklists, flow libraries, FBDI rules** per Fusion release.
A pack = manifest + config schema + optional code (a plugin implementing a published interface) + migrations + tests.
