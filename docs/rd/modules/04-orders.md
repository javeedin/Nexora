# M4 — Nexora Orders (order pad)

Purpose: fast, correct order entry with the company's own pricing and discount rules, approvals and Fusion submission.

| ID | Requirement | Notes / acceptance |
|---|---|---|
| OR-01 | Order pad: customer (F7), items (F6 / scan / `12*code` / scanner words ORD-RET-ADJ±), lines grid, live totals | Legacy `om/pad.js` |
| OR-02 | **Line maths engine** (pure, tested): sign by line type, disc % = customer + marketing + additional, tax by tax code (rates are tenant config), deposit = consignment × qty, crates = ⌊qty / min⌋ × default × crate price, NET | Legacy `om/om-engine.js` — port to a shared TS/.NET library with the same tests |
| OR-03 | **Discount engine**: rules targeting ITEM / GROUP / SUB_CATEGORY / CATEGORY / BRAND / SUPPLIER / PROFIT_CENTER / ALL or level; best CUSTOMER + best MARKETING rule; customer by number → category → everyone; date window, qty band, exclusions; manual % wins when higher; simulator shows winning rules | Legacy WMS_OM_DISCOUNTS (replaced a SQL Server view) |
| OR-04 | Checks → verdict ready / approval / blocked (customer, order type, warehouse, qty, return reasons, max discount, duplicates, required DO/PO, credit, stock, OM period, customer PO reuse, reference order) | Legacy `omChecks` |
| OR-05 | Smart paste: parse pasted text / WhatsApp orders — rules first, then AI (candidate items only) | Legacy `omAiParse` |
| OR-06 | "Usual items" per customer (and, later, ML demand forecast) | Legacy JSON_TABLE over earlier orders |
| OR-07 | Approvals: content-hash-bound (any change after approval needs a new one), approver lists, requester cannot approve own | |
| OR-08 | Fusion submission via `salesOrdersForOrderHub`: draft / submit, price modes FROZEN / MPA / FUSION, consignment line N.1 and crate line N.2, header / line extras with placeholders (flexfields are tenant config) | Legacy `omFusionPayload` |
| OR-09 | Orders board / list with timeline, copy, credit note (RET at original price), Fusion status sync, open any Fusion order, print (BIP layouts), e-mail draft, fiscal submission (pack) | Legacy `om/orders.js` |
| OR-10 | Lookup sources: BIP report + params / Fusion SQL / tenant SQL with `{{BU_ID}}` placeholders; test shows columns | Legacy Setup › Lookup sources |
| OR-11 | Not done in legacy (backlog): Fusion holds, PDA confirm, bulk e-mail, analytics (→ Analytics OM pack) | |

Tenant config replacing legacy hard-coding: business units, tax codes / rates (GROT1.4 15 %, GRTESTOUT17 17 %),
order types (TAX=NO), crate items, deposit items, layouts (BIP paths), order prefixes.

Legacy references: `om/*`, `classes/Form1_OrderMgmtHandlers.cs`, `apex_sql/78_order_management.sql`,
`aianalysis/order-entry.js` (AI order entry form, BOGO companion lines, submit checks).
