# 1. Product vision

## 1.1 One line
**Nexora is the operations cloud for Oracle Fusion customers** — warehouse execution, Fusion data tools, data
pipelines and governed AI agents in one subscription, working *with* Fusion instead of replacing it.

## 1.2 The problem
Companies on Oracle Fusion Cloud ERP / SCM live with gaps Fusion does not close well:

| Gap | What people do today | Cost |
|---|---|---|
| Warehouse execution on the floor (trips, picking, label / document printing, drivers) | Spreadsheets, paper, custom APEX pages | Slow dispatch, wrong prints, no live view |
| Getting data out of Fusion | BI Publisher reports, OTBI, raising SRs, waiting on IT | Days per question |
| Getting data into Fusion | Hand-filled FBDI spreadsheets, trial and error on imports | Failed loads, week-long data migrations |
| Moving Fusion data elsewhere | BICC extracts + custom scripts | Brittle jobs nobody owns |
| Country rules (e.g. fiscal e-invoicing like Mauritius MRA) | Custom integrations per country | Expensive, risky |
| AI | Generic chatbots with no access to Fusion, or unsafe agents | No trust, no audit |

## 1.3 The product
One web platform, one login, modules switched on per subscription:

| Module | What it gives the customer |
|---|---|
| **Nexora WMS** | Trips, picking, picker monitor, printing to local printers, shipping agent, fiscal invoice packs (MRA first) |
| **Nexora SQL** | Read-only SQL workbench on Fusion (BI Publisher runner), saved queries, Ask AI, Knowledge that learns the company's Fusion, Watchdogs (anomaly alerts), Flows (order-to-cash and other process traces), setup checklist |
| **Nexora Load** | FBDI templates filled from real data with checks before upload, Fusion REST loads, FSM setup tracking and setup-data analysis |
| **Nexora Orders** | Order pad with the company's pricing and discount rules, approvals, Fusion order submission |
| **Nexora SCM Workbench** | Fusion Order Management, Purchasing, Inventory, Costing and Setup screens that are faster than Fusion's own for daily work |
| **Nexora Pipelines** | Fusion / APEX / database → warehouse (Oracle, DuckDB, …) pipelines: incremental, scheduled or continuous, monitored |
| **Nexora Analytics** | Shared semantic model (DAX-compatible measures) on Fusion data, Fusion packs (GL, AP, AR, PO, OM, INV), dashboards, Power BI feed |
| **Nexora Agents** | AI digital employee with approvals, policies, kill switch, audit and cost control; Pipeline Doctor and other LangGraph agents |
| **Nexora AI Hub** | Model gateway (Claude on AWS Bedrock / Claude Platform on AWS / direct, NVIDIA NIM, other Bedrock models): routing, data policies, budgets, evals on the customer's own questions |

## 1.4 Customers and personas
**Target customer:** mid-size companies (100–5,000 employees) running Oracle Fusion Cloud (ERP and/or SCM), especially
distribution, FMCG, retail and manufacturing with own warehouses and delivery fleets. First market: Indian Ocean / Africa /
Middle East / India (pilot: a distributor in Mauritius = the legacy system's owner, tenant #1).

| Persona | Needs | Main modules |
|---|---|---|
| Warehouse supervisor | Plan trips, assign pickers, print everything right, see progress | WMS |
| Picker / driver | Simple phone screens, scan, confirm | WMS (PWA) |
| Order desk | Enter orders fast with correct prices, get approvals | Orders |
| Fusion functional consultant / key user | Answer data questions, load data, check setups | SQL, Load, SCM Workbench |
| Finance / controller | Reconciliations, period checks, fiscal compliance | SQL (Flows, Watchdogs), Analytics, WMS fiscal pack |
| Data / BI team | Reliable Fusion data elsewhere, a semantic model | Pipelines, Analytics |
| IT / platform admin | Users, SSO, permissions, AI policies, cost, audit | Platform admin, AI Hub |
| Partner / implementer | Reuse across clients: templates, packs, migrations | Load, SQL, Analytics |

## 1.5 Principles
1. **Fusion stays the system of record.** Nexora reads freely (read-only by default), writes only through official
   Fusion APIs (REST, FBDI / ERP Integration, BIP) and only with permission + audit.
2. **Safe AI.** Every acting AI step is policy-checked, approved where needed, audited, costed and stoppable.
3. **Multi-tenant from day one.** Nothing is "Gray's" in the core; customer specifics are configuration or plugins (packs).
4. **Measured, not claimed.** Evals for AI, accuracy scoreboards for forecasts, SLOs for the platform.
5. **Works on the floor.** Phones, scanners, printers and bad networks are first-class.

## 1.6 Success measures (first 12 months)
- Pilot tenant fully migrated off the legacy desktop app; ≥ 3 paying tenants.
- Trip dispatch time −50 %, print errors ≈ 0 at the pilot.
- Time to answer a Fusion data question: days → minutes (Nexora SQL usage).
- FBDI first-time-right rate > 90 %.
- AI: every acting step audited; eval accuracy published per model; AI cost per tenant within budget.
