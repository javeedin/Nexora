# Phase 4 — Agents (governed AI digital employee)

**Exit criteria:** pilot users ask and act through the Nexora agent; every acting step is policy-checked, approved where
required (fingerprint-bound), audited with cost and stoppable by the kill switch; the legacy AI Digital Employee page is
retired.

| Task | What | Covers | Depends | Done when |
|---|---|---|---|---|
| P4-T01 | **Action Gateway**: one service path for every acting AI step — OPA policy (AUTO / ASK / DENY, max batch, most-specific-first) → approval → execute → audit → cost | AG-10, AG-14 | P1-T03 | unit + integration tests for resolution order |
| P4-T02 | **Approval integrity**: server-issued fingerprint of exactly what the card shows; single use; 12 h expiry; re-check policy + kill switch at decision | AG-11 | T01 | tampered decision rejected in tests |
| P4-T03 | Policies admin UI (admins only), seed defaults per plan | AG-10 | T01 | |
| P4-T04 | **Tools as MCP servers** backed by Nexora APIs: tenant_query (typed / approved read SQL), fusion_read, fusion_write, module_api (action catalog), report_save, db_change (admin), schedule_job, email, fiscal_submit, device (via Edge), model (Analytics, when available) | AG-03, AG-23 | T01 | each tool has contract + permission tests |
| P4-T05 | **Chat agent** (LangGraph): loop with round limit, streaming steps, attachments (xlsx / csv / pdf / docx / images), pod switch, research panel, exports, grid / form / order-entry interactive results, conversation storage per user | AG-01, AG-02, AG-04 | T04 | eval set of pilot tasks |
| P4-T06 | **Inbox**: requests decided from web / mobile / Teams card, first decision wins, selections, DONE / FAILED reporting, alerts, expiry, duplicate suppression | AG-13 | T02, P1-T07 | (a minimal version is brought forward for P2-T18) |
| P4-T07 | **Trained processes** (definitions with stages, validations incl. CHECK_SQL, interfaces, steps, lookups; "train from description") | AG-05 | T05 | |
| P4-T08 | **Knowledge packs** (business knowledge per tenant / pack, versioned) replacing hard-coded prompt sections | AG-06 | T05, P2-T01 | MRA knowledge comes from the MRA pack |
| P4-T09 | Redaction of secrets in prompts / tool I/O / logs | AG-15 | T04 | |
| P4-T10 | **Scheduled jobs** (Temporal: ONCE / RECURRING / REPEAT_UNTIL_DONE; typed steps; cloud lane + Edge lane with leases / recovery) | AG-20 | T04, P2-T02 | |
| P4-T11 | **Daily tasks** board (people + AI, recurrence, events, results, library; work in chat or deterministic run) | AG-21 | T05, T10 | |
| P4-T12 | Saved reports with parameters | AG-22 | T04 | |
| P4-T13 | **Forms designer** (port JSON form engine; WYSIWYG; AI builds forms) | AG-24 | P0 | sample POS form renders |
| P4-T14 | **Agent flows** designer → compiled to LangGraph (nodes: AI agent, query, write w/ approval, HTTP, condition, vars, approval pause, e-mail) | AG-25 | T04 | |
| P4-T15 | **Pipeline Doctor** productised (from legacy `ai-hub` agent) | AG-30, PL-06 | P5-T05 or fake pipeline API | interrupts work end-to-end |
| P4-T16 | Pilot migration (policies, processes, reports, forms, flows, tasks) + switch | — | all | exit criteria checked |
