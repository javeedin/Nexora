# M10 — Fiscal / e-invoicing packs (first: Mauritius MRA)

Purpose: country fiscal rules as installable packs, so the core never contains one country's logic.

## Pack interface
`validate(order)` → issues; `build(order)` → fiscal document; `submit(doc)` → {status, fiscal_id (IRN), qr, raw};
`writeBack(order, result)` → Fusion; statuses DONE / ALREADY / NOT_REQUIRED / FAILED / NOT_SENT (safe to retry) /
TIMEOUT (sent, may exist — check before retry); per-step timings; circuit breaker (stop after N gateway problems).

## MRA pack (parity with legacy)
| ID | Requirement |
|---|---|
| FP-01 | Already-sent check (legacy BIP `MRA_TRX_NO_CHECK_BIP`) → ALREADY |
| FP-02 | Fetch order summary + details in parallel (legacy BIPs `ORDER_SUMMARY_4_ORDER_NUMBER_BIP`, `ORDER_DETAILS_MRA_BIP`) |
| FP-03 | Order-type rule table (interface Y/N) → NOT_REQUIRED |
| FP-04 | Line statuses must be CLOSED / AWAIT_BILLING / BILLED / SHIPPED / CANCELED |
| FP-05 | Build invoice: STD; CRN for returns (+ reference); PRF pro-forma; currency, person type VATR, TAN / BRN / customer category; seller block and tax-code mapping (legacy 6004 → TC01, 23006 → TC03; 'SGG' service rule) **as pack config** |
| FP-06 | Submit to the gateway with 60 s limit; success only with IRN; TIMEOUT vs UNREACHABLE distinction |
| FP-07 | Write IRN back to the Fusion order header EFF (legacy `HeaderEffBGRAYSprivateVO.HoldReleasedBy` → configurable context / attribute) |
| FP-08 | On / off per pod with mandatory reason and change log; flag shown on trips |
| FP-09 | Batch submission (trip, selection, AI action) with live status and retry |
| FP-10 | Gateway over HTTPS only (legacy `http://mra.busi.in/MRAInvoice.php` — require TLS proxy or vendor HTTPS) |

Legacy references: `classes/MRAProcessor.cs`, `MRAModels.cs`, `Form1_AiMraHandlers.cs`, `wms/mra-processor.js`,
`wms/mra-interface.js`, `apex_sql/79_mra_interface_config.sql`, `MRA_IMPLEMENTATION_GUIDE.md`, `MRA_KNOWLEDGE` in
`classes/ClaudeCliService.cs`. Next packs: India e-invoice (IRP), KSA ZATCA, UAE.
