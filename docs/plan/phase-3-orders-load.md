# Phase 3 — Orders + Load

**Exit criteria:** the pilot's order desk enters and submits orders in Nexora with legacy-identical prices and discounts;
a first FBDI import (e.g. AP invoices or journals) is prepared, checked, uploaded and imported end-to-end through Nexora.

| Task | What | Covers | Depends | Done when |
|---|---|---|---|---|
| P3-T01 | **Pricing / discount / checks engine** as a shared library: port `om/om-engine.js` with all tests (line maths, discount rules, checks, Fusion payload); tax rates, crate / deposit items, order types as tenant config | OR-02, OR-03, OR-04 | P0 | legacy test vectors pass byte-for-byte |
| P3-T02 | Orders data model + APIs (orders, lines, events, approvals, discounts, back-orders, user defaults, prefixes); migrate pilot data | OR-07 | T01 | |
| P3-T03 | **Order pad UI** (keyboard-first: F7 customer, F6 items, scan, `12*code`, scanner words), live totals and checks | OR-01 | T02 | order desk test session |
| P3-T04 | Lookup sources (BIP / Fusion SQL / tenant SQL with placeholders) + test | OR-10 | T02 | |
| P3-T05 | Smart paste (rules → AI gateway, candidate items only) | OR-05 | T03, P1-T03 | |
| P3-T06 | Usual items | OR-06 | T02 | |
| P3-T07 | Approvals (content-hash bound) | OR-07 | T02 | |
| P3-T08 | Fusion submission (draft / submit, price modes, consignment / crate lines, extras with placeholders) | OR-08 | T01, P0-T14 | order created on TEST pod |
| P3-T09 | Orders board / list, timeline, copy, credit note, status sync, open Fusion order, print, e-mail draft, fiscal submit | OR-09 | T08, P2-T14, P2-T17 | |
| P3-T10 | **FBDI catalog + specs**: port generator + generated catalog / specs per release; download with zip-signature check; newer-release probe | LD-01, LD-02 | P0 | 45 templates load |
| P3-T11 | **FBDI engine + rules**: port `fbdi-engine.js`, `fbdi-rules.js` with tests | LD-03 | T10 | legacy CSV outputs identical |
| P3-T12 | **Prepare & Load wizard** (Source → Map → Check → Generate) + assist (browse tables, input template round-trip, Prepare FBDI) | LD-03, LD-04 | T11 | |
| P3-T13 | **Upload + import**: UCM upload, `importBulkData`, ESS monitoring, results and error rows back into the load | LD-05 | T12 | end-to-end import on TEST pod |
| P3-T14 | History of loads / runs / files, rebuild ZIP | LD-06 | T12 | |
| P3-T15 | Fusion REST loads (catalog, probe, describe, sample + drill-down, map, group children, validate, POST / PATCH with parallelism and stop / retry) | LD-07 | P0-T14 | |
| P3-T16 | FSM setup projects tracking | LD-08 | P0-T14 | |
| P3-T17 | FSM setup data analysis (exports, counts, match to tasks, compare) | LD-09 | T16 | |
| P3-T18 | Load safety policies (TEST first, mandatory checks, approvals for PROD above threshold) | LD-10 | T13 | |
