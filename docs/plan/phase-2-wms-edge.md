# Phase 2 — WMS core + Nexora Edge + MRA pack

Goal: the pilot's warehouse runs a full day on Nexora; the legacy desktop app is no longer needed for WMS.
Prerequisite: **P0-T24 APEX export** (trip / pick / S2V logic lives there).
**Exit criteria:** dispatch creates trips, releases / confirms picks, ship-confirms, prints every document through Edge
and submits MRA invoices for a full day at the pilot; shipping agent runs server-side; pickers use the PWA.

| Task | What | Covers | Depends | Done when |
|---|---|---|---|---|
| P2-T01 | **Packs registry**: install / upgrade / configure packs per tenant (manifest, config schema, migrations, plugin interface) | PC-11 | P0 | sample pack installs per tenant |
| P2-T02 | **Nexora Edge** agent (`apps/print-agent`, .NET worker or Go): pairing to a site, outbound WebSocket/gRPC, auto-update, health; printer discovery (local / network / Bluetooth / Wi-Fi), test print, print PDF (Sumatra / native), local file intake; registration UI | PC-15, WM-20 | P0 | Edge on a Windows PC prints a test page from the cloud |
| P2-T03 | **WMS data model + APIs** from the APEX export: trips, trip lines, pickers, vehicles, assignments, statuses; migrations; pilot data import | WM-01, WM-04, WM-50, WM-51 | P0-T24 | trip list parity with legacy for a sample day |
| P2-T04 | Trip list / board UI (filters, tabs, KPIs, volume grids, export, multi-window) | WM-01, WM-15 | T03 | |
| P2-T05 | Create trip, add orders (pending orders / paste / pending shipment lines), remove, move with rollback | WM-02, WM-03 | T03 | |
| P2-T06 | Assign picker | WM-05 | T03 | |
| P2-T07 | **Pick release** workflow (Temporal): pick wave → open picks → lots per order; release all with progress and retry | WM-06 | T03, P0-T16 | runs against TEST pod |
| P2-T08 | Allocate lots / S2V; store transactions view with cancel line / lot, audited set-data override, process | WM-07, WM-13, WM-42 | T03 | |
| P2-T09 | Order transactions view (pick release, SO lines, lots, shipment lines, confirm responses, pick slip) | WM-08 | T03 | |
| P2-T10 | Cancel lines (scheduled / selected / not picked) with child lines + promo map (tenant config), via connector write scope | WM-09 | T03 | |
| P2-T11 | **Pick confirm** + **ship confirm** workflows (step results, stop on failure, idempotent) | WM-10, WM-11 | T07 | |
| P2-T12 | Shipment lines per trip: actual ship date update, bulk cancel | WM-12 | T11 | |
| P2-T13 | Trip analytics | WM-14 | T03 | |
| P2-T14 | **Document types** (tenant config: report path + parameter + order-type rules) and **print job service** (durable queue, statuses, retries, history) | WM-21, WM-22, WM-26 | T02 | |
| P2-T15 | Auto-print per trip + skip zero-line orders | WM-23 | T14 | |
| P2-T16 | Monitor printing UI + print job grid | WM-24, WM-25 | T14 | |
| P2-T17 | **MRA fiscal pack** (pack interface + MRA implementation, config for seller block / tax mapping / EFF / gateway over HTTPS), on/off per pod with reason + log, batch submit with breaker | FP-01…FP-10 | T01 | parity tests against legacy `MRAProcessor` behaviour with a mock gateway |
| P2-T18 | **Shipping agent** as Temporal workflow: agents, trips, tasks 1–3, approvals via inbox (minimal inbox from P4-T06 brought forward), print trip with fiscal step, reports, views | WM-30…WM-36, AG-31 | T11, T15, T17 | agent runs a full day without a browser open |
| P2-T19 | Auto SO processing (real, not simulated) | WM-40 | T07, T11 | |
| P2-T20 | Auto inventory (S2V) processing incl. staged-transaction error clean-up, verify vs Fusion, share results | WM-41 | T08 | |
| P2-T21 | Pending shipment lines grid + bulk volume | WM-43 | T03 | |
| P2-T22 | Picker monitor, picking time monitor (score tunable) | WM-52, WM-53 | T03 | |
| P2-T23 | **Picker / driver PWA** (my picks, scan confirm, exceptions, location pings, offline queue) + route tracker | WM-54, WM-55 | T03 | works offline in a Playwright mobile test |
| P2-T24 | Daily history / task mining (activity telemetry, journeys, friction, feedback) | WM-56, PC-16 | P0 | |
| P2-T25 | Inventory read screens (on-hand, lots, transactions, reservations) | WM-60, WM-61 | P0-T14 | |
| P2-T26 | Pilot migration + parallel run + switch | — | all | exit criteria checked |
