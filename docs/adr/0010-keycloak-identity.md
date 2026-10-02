# ADR 0010 — Keycloak (self-hosted) as the identity provider
Status: accepted (2026-10-02, user decision). Answers open question 2; narrows the Auth row of RD 03.
Context: the legacy system had its own username / password check (APEX table, plain-text passwords, credentials in the
URL). Nexora needs OIDC, tenants as organisations, MFA, invitations and later per-tenant SSO and SCIM. The customer has
no identity server and prefers not to buy one.
Decision: Keycloak, self-hosted next to the platform (same image as the dev stack), realm `nexora`.
- Tenant = Keycloak **organisation**; `tenant_id` = the organisation's id (UUID), carried in the signed access token
  (claim `tenants: {"<alias>": {"id": "<uuid>"}}`) — no per-request lookup. Users in several organisations pick one per
  request (`X-Nexora-Tenant`), validated against the token.
- Roles in the token (`realm_access.roles`); the API validates issuer, audience `nexora-api`, signature, lifetime.
- MFA by step-up: `acr` `pwd` (password) / `mfa` (password + OTP); admin capabilities require `mfa` (configurable).
- Users join by **invitation** from their tenant admin (organisation invite); no open self-registration. Per-tenant SSO
  (Entra ID, Okta, Google …) as organisation identity providers when a customer asks.
- Login page keeps the legacy feel (username or e-mail, password, remember me) via a `nexora` login theme; the work
  context (Fusion pod, business unit, inventory org) is chosen right after sign-in in the web app, and remembered.
Consequences: we operate Keycloak (HA, backups, upgrades) — no licence cost; the API depends only on standard OIDC /
JWT, so a hosted IdP stays possible later. Legacy users can be imported once (passwords reset on first login).
