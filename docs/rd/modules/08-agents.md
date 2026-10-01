# M8 — Nexora Agents (AI digital employee + governed agents)

Purpose: let people ask and *act* in plain language across WMS, Fusion and the tenant's data — safely: every acting step
is policy-checked, approved when needed, audited, costed and stoppable.

## 8.1 Chat agent ("digital employee")
| ID | Requirement | Legacy |
|---|---|---|
| AG-01 | Chat with attachments (xlsx / csv / pdf / docx / images), slash commands, pod switch, research panel per round, export (Excel / CSV / PDF), copy SQL, live "pipeline" diagram of what the agent is doing | `aianalysis/index.html` |
| AG-02 | Agent loop: tool calls with a round limit and "rounds left" nudges; runs in the AI platform (LangGraph), streaming steps; conversation stored per user | `ClaudeCliService.RunLoopAsync` (JSON-only action protocol, 8 rounds; CLI or API mode) |
| AG-03 | **Tools** (as MCP servers in the AI platform): `tenant_query` (read-only typed queries / approved SQL), `fusion_read` (REST GET), `fusion_write` (POST / PATCH / DELETE — policy), `module_api` (curated actions from the API catalog — policy), `report_save`, `db_change` (admin only, policy), `schedule_job`, `email`, `fiscal_submit` (pack, e.g. MRA), `device` (printers, print grid / order PDFs, download, file intake via Edge), `model` (Analytics read-only), `dll_inspect` (dev tool) | action table in the survey (`sql`, `fusion`, `ords`, `api_form`, `db_write`, `schedule_job`, `email`, `mra_interface`, `device`, `dll`, `model`) |
| AG-04 | Interactive UI results: selectable grid with up to 3 actions, editable confirm forms (catalog API / raw request / stored form), order-entry form | `grid`, `api_form`, `order-entry.js` |
| AG-05 | **Trained processes**: tenant-defined procedures (stages, data sources, validations incl. deterministic CHECK_SQL, interfaces, steps, lookups) the agent must follow; "train process" from a description | WMS_AI_PROCESSES |
| AG-06 | Business knowledge per tenant / pack (e.g. order creation routes, line-cancel rules, fiscal flow) as versioned **knowledge packs**, not hard-coded prompt text | prompt sections MRA_KNOWLEDGE etc. in `ClaudeCliService.cs` |

## 8.2 Safety and control plane
| ID | Requirement | Legacy |
|---|---|---|
| AG-10 | Policies per (user or role) × action × pod: AUTO / ASK / DENY + max batch; resolution most-specific first; default ASK; admin-only edit; OPA in the product | WMS_AI_POLICIES (scripts 40, 41, 74) |
| AG-11 | **Approval integrity**: the server registers a fingerprint of exactly what the card shows; a decision runs only if it matches an issued card, once, within 12 h, and the policy / kill switch still allow it | `Form1_AiGuard.cs` |
| AG-12 | **Kill switch** per tenant (+ global vendor switch): anyone pauses, admins resume; blocks chat actions, approvals, jobs, agents, the AI gateway | WMS_AI_CONTROL.AI_ENABLED |
| AG-13 | **Inbox**: approval requests decided from anywhere (web / mobile / Teams card), first decision wins, selection of items, requester reports DONE / FAILED, alerts, 48 h expiry, duplicate suppression | WMS_AI_INBOX |
| AG-14 | Audit + cost per turn and per action (model, tokens incl. cache, USD), per-tenant budgets | WMS_AI_AUDIT, price table |
| AG-15 | Redaction of secrets in prompts, tool I/O and logs; no credentials ever passed to the model | `DllInspector.Redact` |

## 8.3 Jobs, tasks, reports, forms
| ID | Requirement | Legacy |
|---|---|---|
| AG-20 | **Scheduled jobs** (ONCE / RECURRING / REPEAT_UNTIL_DONE with completion query, max runs, until date) of typed steps (query, REST to allow-listed hosts, print, download PDF, forEach, module action); cloud lane (Temporal) and **Edge lane** (local devices) with leases and recovery | WMS_AI_JOBS (DB lane DBMS_SCHEDULER, LOCAL lane polling), scripts 38, 56, 74 |
| AG-21 | **Daily tasks** board: tasks with assignee (person or AI), recurrence, status, events, results, issues; "work in chat" or deterministic run; task library | `aianalysis/tasks.js`, WMS_AI_TASKS* |
| AG-22 | Saved reports with parameters, re-run, export, show the exact request | WMS_AI_REPORTS |
| AG-23 | Curated **action catalog** (module write APIs with field schemas and pod injection), every run logged | `aianalysis/api-catalog.js`, WMS_AI_API_LOG |
| AG-24 | **Forms designer**: JSON-defined forms (header fields with cascades, detail grids with lookups / line rules / totals, rules, actions, wizard, mobile layout), WYSIWYG canvas, AI builds forms from text or screenshots | `formsdesigner/*`, WMS_AI_FORMS |
| AG-25 | **Agent flows**: visual flow designer (Start, AI agent, query, write with approval, HTTP, condition, set vars, approval pause, e-mail, end) → compiled to LangGraph graphs with checkpoints | `agentflow/*`, WMS_AI_FLOWS |

## 8.4 Specialist agents (LangGraph)
| ID | Agent | Behaviour |
|---|---|---|
| AG-30 | **Pipeline Doctor** | triage (rules) → diagnose (LLM, JSON) → verify ⏸ (test fix read-only on source) → propose → approval ⏸ → apply ⏸ (patch task, queue re-run) → report; checkpointed; only whitelisted task fields | `ai-hub/ai_hub/agents/pipeline_doctor.py` (tested) |
| AG-31 | **Shipping agent** | WMS background agent: trip summaries, print trip, line cancellations only via inbox approval | see M1 |
| AG-32 | **Fusion SQL agent** / **Analytics agent** | see M2, M7 |
| AG-33 | Backlog: Order exception agent, fiscal exception agent (MRA rejects), FBDI import doctor, period-close assistant | new |

## 8.5 Dropped / changed
- Claude Code CLI mode and its Windows installer (`Form1_AiCliInstall.cs`) → not needed server-side.
- Generic literal-SQL gateways (`ai/executequery`, `ai/executewrite`) → typed tools; ad-hoc SQL only read-only and audited.
- RAG desktop service (`rag/`) → AI platform RAG on Oracle 23ai vectors / pgvector.
- Internet Search module → **out of scope** (personal tool, not part of the product).
- Hard-coded admin `JAVEED`, bootstrap "everyone is admin when the list is empty" → tenant admin set at provisioning.
