# M5 — Nexora SCM Workbench (fast Fusion screens)

Purpose: daily Fusion SCM work on screens faster than Fusion's own, through official REST (no DB writes).

| ID | Area | Capabilities (legacy parity) |
|---|---|---|
| SCM-01 | Order Management | Order Hub search / lines / analytics; order view with timeline; create / change / copy / return editor (charges, EFF, lots, reservations, credit check, branch PO); POS with 80 mm receipt; customers (BIP); price lists; shipment lines; pick release / confirm; ship confirm (`shippingTransactions`); AutoInvoice; AR invoices |
| SCM-02 | Purchasing | POs, draft POs, life cycle, expected receipts / receive, ASN, supplier returns, suppliers, supplier balance (SQL with REST fallback), PO loading |
| SCM-03 | Inventory | Item master, item loading with DFF / EFF, on-hand, subinventories, staged on-hand transactions, transfer orders, inventory transactions |
| SCM-04 | Costing | Item costs, receipt costs, cost management via ERP integrations with an ESS monitor |
| SCM-05 | Setup & diagnostics | Business units, legal entities, Browse Data service scan, 360° PO tracker, FSM ZIP explorer, COA segments, trial balance check, UAT diagnostics (BIP) |
| SCM-06 | Shared engine | Grid with filters → `q`, paging, sort, quick filter, KPIs, CSV; drawer; forms; LOV / typeahead; pod switch |
| SCM-07 | Gaps to close (legacy disabled) | DELETE operations (draft PO lines, sales credits, notes, attachments, charges, reservations) — allow through the connector with approval; SOAP on-hand loading; identity-domain login history |

Legacy references: `fscm/*` (46 files: `core.js` = FX engine, `fom-*.js`, `purchasing.js`, `inv-*.js`, `costing.js`,
`setup-*.js`). All calls went through host action `dataLoadFusionRest` (GET / POST / PATCH to
`*.oraclecloud.com/{fscm|hcm|crm}RestApi/resources/` only) — becomes the Fusion connector with write scopes.
