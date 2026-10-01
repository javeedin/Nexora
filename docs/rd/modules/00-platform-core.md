# M0 — Platform core (all plans)

Purpose: everything every module needs. Built first (roadmap phase 0).

| ID | Requirement | Acceptance |
|---|---|---|
| PC-01 | Tenant lifecycle: sign-up, provisioning workflow, suspend, export, delete | New tenant usable < 10 min; export = ZIP of all tenant data + files |
| PC-02 | Pods: a tenant registers 1..n Fusion environments (name, base URL, credentials in Vault, read/write scopes, default pod per user) | "Test connection" calls a REST resource + the BIP runner; credentials never returned to the browser |
| PC-03 | Sites / warehouses, business units, inventory orgs, currencies as tenant reference data (imported from Fusion with a wizard) | Replaces legacy hard-coded BU 300000003234003, org GIC, currency MUR … |
| PC-04 | Identity: OIDC organisation per tenant, SSO, MFA, SCIM (Enterprise), invitations | |
| PC-05 | RBAC: roles per module (viewer / user / approver / admin) + scopes (site, BU, pod); custom roles (Enterprise) | Every API checks module entitlement + role + scope |
| PC-06 | Entitlements: plan + add-ons + overrides; UI hides modules not entitled; upgrade prompts | |
| PC-07 | Settings framework: typed, versioned settings per tenant / site / user with JSON schema; settings UI generated from schema | Replaces localStorage settings and WMS_*_SETTINGS tables |
| PC-08 | Audit log: every write, Fusion call, AI action, login, settings change; searchable, exportable, immutable | Legacy WMS_AI_AUDIT model (source, action, outcome, approval, target, model, tokens, cost) |
| PC-09 | Notifications: in-app, e-mail (tenant SMTP or ours), Teams / Slack webhooks (allow-listed hosts), per-user preferences | Legacy: AiControl.SendAlertAsync, SmtpVault |
| PC-10 | Files: object storage per tenant (PDFs, FBDI zips, exports), virus scan, signed URLs, retention | Replaces C:\fusion\… folders |
| PC-11 | Packs registry: install / upgrade / configure packs per tenant (fiscal, analytics, rules) | See 02 §2.8 |
| PC-12 | Usage metering + billing integration (Stripe / marketplace); budget alerts | |
| PC-13 | Admin console (tenant admin): users, roles, pods, sites, settings, packs, usage, audit, AI control | |
| PC-14 | Vendor console (our staff): tenants, plans, health, support access (consented, audited), feature flags | |
| PC-15 | Nexora Edge registration: pair an agent to a site, health, versions, remote config | |
| PC-16 | Activity analytics: page / action telemetry per tenant (task mining) | Legacy WMS_ACTIVITY_LOG + views |

Legacy references (`legacy/v12`): `classes/AiControl.cs` (audit, kill switch, roles), `apex_sql/75_ai_control.sql`,
`classes/Form1_AdminHandlers.cs` (release / modules), `Home/index.html` + `Home/trial-gate.js` (module tiles, trial),
`engine/FusionModel/Licensing/Licensing.cs` (signed licences), `classes/SmtpVault.cs`, `classes/FusionSqlService.cs`
(`FusionSqlStore` DPAPI secrets), login page `login.html`.
