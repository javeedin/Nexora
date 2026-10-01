# M1 — Nexora WMS (warehouse execution on top of Fusion)

Purpose: plan and run the outbound warehouse day — trips, picking, Fusion pick / ship confirmation, printing, store
transfers — with background agents doing the routine work and people approving exceptions.

> **Important legacy gap:** most trip / pick / S2V server logic lives only in the customer's APEX workspace (ORDS
> handlers + PL/SQL), not in the repository. Before rebuilding M1, export the APEX workspace (ORDS modules
> TRIPMANAGEMENT, WAREHOUSEMANAGEMENT, ARMODULE and their PL/SQL packages) into `legacy/apex-export/` (see 95 §Phase 0).

## 1.1 Trips
| ID | Requirement | Legacy pointers |
|---|---|---|
| WM-01 | Trip list by date range and pod with filters (lorry, picker, status), grid / cards, tabs Orders / Customers / Lorries / Pickers / Trips, KPIs, volume detail + summary, export | `wms/app.js` `openTripManagementTab`, `displayTripData` |
| WM-02 | Create trip; add orders from pending orders, pasted order numbers or pending Fusion shipment lines | ORDS `trips/create`, `trips/addorders`, `trips/getpendingorders`, `getpendingshipmentlines` |
| WM-03 | Remove order from trip; **move** order to another trip (delete + add, roll back on failure) | `wms/trip-move.js` |
| WM-04 | Trip header: lorry (required), priority, loading bay; status kept; profit centres shown | `saveTripHeader`, `UPDATETRIPHEADER` |
| WM-05 | Assign picker to a trip or selected orders with assignment date | `trip/assignpicker` |
| WM-06 | Pick release: single order; **release all** = per order (1) release pick wave (warehouse = tenant config, legacy `GIC`) (2) get open picks (3) get lots; line counts first; per-step progress; retry failed orders | `pickReleaseAll`, `trip/callpickwave`, `getopenpicksbyorder`, `getlotsforpicks` |
| WM-07 | Allocate lots / store-to-van processing per order | `materialtrx/allocatelots`, `trip/processs2v` |
| WM-08 | Order transactions view: pick-release details, SO lines, lots, Fusion shipment lines, pick / ship confirm responses, pick slip | `openTripDetails` dialog |
| WM-09 | Cancel lines in Fusion (scheduled / selected / not picked) via `salesOrdersForOrderHub` PATCH with a tenant-configured cancel reason (legacy "OUT OF STOCK"); child lines (numbered sub-lines, BOGO / promo companions from a mapping table) handled together | `trip/orders/cancel*`, BOGO map `ARMODULE/BOGO` |
| WM-10 | Pick confirm by line / all via Fusion `pickTransactions`; on success update local status | `trip/updatepickconfirmstatus` |
| WM-11 | Ship confirm: get shipment number → Fusion `shippingTransactions` CONFIRM → update local status; step-by-step result, stop on first failure | `shipConfirmOrder` |
| WM-12 | Shipment lines per trip: update actual ship date (`shipmentTransactionRequests` ShipmentUpdate), bulk cancel | |
| WM-13 | Store transactions (S2V / V2S): details, allocated lots, QOH, cancel line / lot, admin "set data" override (audited), process | `trip/sets2vdata`, `cancels2v*` |
| WM-14 | Trip analytics: order trends (day / week / month), status distribution, by lorry / picker, top customers, weight by lorry | |
| WM-15 | Open each page in its own window / tab (multi-monitor dispatch desks) | legacy `--page=` new exe instance |

## 1.2 Printing (via Nexora Edge)
| ID | Requirement | Legacy pointers |
|---|---|---|
| WM-20 | Printer registry per site through Edge: discover local / network / Bluetooth / Wi-Fi printers, test print, paper size, orientation, default printer per user / station | `PrinterService.cs`, `printer-management-new.js`, `wms_printer_config` (plain-text Fusion password — **must not be repeated**) |
| WM-21 | Document types per tenant: report path + parameter for each document (sales order, sales order with lots, material transaction, line-count check …), chosen by order type rules | legacy `/Custom/OQ/GR_SalesOrder_Rep.xdo`, `GR_SalesOrder_Lot_Rep.xdo`, `/Custom/DEXPRESS/STORETRANSACTIONS/…`; order-type rule S2V/V2S → material transactions |
| WM-22 | Print job lifecycle: download (Pending → Downloading → Completed / Failed) and print (Pending → Queued → Printing → Printed / Failed), retries, history; **durable queue** (legacy was in-memory, lost on restart) | `PrintJobManager.cs`, `wms_print_jobs` |
| WM-23 | Auto-print per trip: download all order documents and print in sequence; skip orders with 0 lines (line-count report, tiny PDFs) | `enableAutoPrint`, `GR_SO_LINE_COUNT_BIP` |
| WM-24 | Monitor printing per trip: download / preview / print / print all, per-order status persisted, diagnostics | `wms/monitor-printing*.js`, `wms_monitor_printing` |
| WM-25 | Print job grid: filters, stats, preview, open, export, retry all failed | `loadPrintJobs` |
| WM-26 | PDF storage in tenant object storage (not C:\fusion), cached on Edge for printing | |

## 1.3 Shipping agent (background)
| ID | Requirement | Legacy pointers |
|---|---|---|
| WM-30 | Agents: create (name, pod, capabilities), assign trips, start / pause / stop / close; interval ≥ 5 min; per-trip task choice; quick modes (cancel only / print only); statuses IDLE / RUNNING / PAUSED / COMPLETED / ERROR | `wms/shipping-agent.js` |
| WM-31 | Task 1 — order status from Fusion shipment lines (Released to WH, Staged, Part Staged, Part Interfaced, Interfaced / Shipped) saved per order | `saProcessTripTick` |
| WM-32 | Task 2 — lines stuck SCHEDULED / MANUAL RESERVATION (+ child lines): **never cancels without approval** → inbox request; approved → Fusion PATCH; rejected not re-raised until lines change; held orders not printed | `aiInboxCreate` source SHIPPING_AGENT |
| WM-33 | Task 3 — auto-print orders Interfaced / Shipped without a document; trip done when all interfaced + printed; agent stops itself with a completion report + AI summary | |
| WM-34 | Print trip with fiscal step: if the tenant's fiscal pack is on for the pod, submit each order first and print only DONE / ALREADY / NOT_REQUIRED; stop after 2 consecutive gateway problems (safe to retry) | `saPrintTrip`, `WMS_MRA_INTERFACE_CONFIG` |
| WM-35 | Activity log, notifications, performance, verify PDFs, backordered lines views | |
| WM-36 | **Runs server-side** as a Temporal workflow per agent (legacy ran in a browser tab on one PC); kill switch checked every tick | |

## 1.4 Auto processing and store transactions
| ID | Requirement | Legacy pointers |
|---|---|---|
| WM-40 | Auto sales-order processing: trip → order → line tree with status (CANCELLED / PROCESSING / FAILED / SUCCESS / PENDING), filters, pick release / assign picker / cancel scheduled lines for selected; trip print modal with document choice; batch fiscal with retry. Legacy "Run processing" is a **simulation** — implement for real | `wms/auto-sales-order-processing.js` |
| WM-41 | Auto inventory (S2V) processing: pending transactions by trip / order / LID, bypass, process (`processs2vauto`), on failure delete Fusion `inventoryStagedTransactions` errors, error detail, retry, cancel lot, verify against Fusion (side by side), print, analytics, share results (Teams / e-mail) | `wms/auto-inventory-processing.js` |
| WM-42 | Pending store transactions grid (by source org) → add to trip, assign picker, allocate lots, order volume | `wms/pending-store-transactions.js` |
| WM-43 | Pending shipment lines grid with bulk order-volume calculation (cancellable) | `wms/pending-shipment-lines.js` |

## 1.5 People, vehicles, floor
| ID | Requirement | Legacy pointers |
|---|---|---|
| WM-50 | Vehicles master + dashboard (trips per lorry) — legacy add / edit / delete were stubs | `wms/vehicles.js` |
| WM-51 | Pickers master — legacy add / edit / delete were stubs | `wms/pickers.js` |
| WM-52 | Picker monitor: per-day assignments (cards / grid), export, AI summary (saved), open order PDF | `wms/picker-view.html` |
| WM-53 | Picking time monitor: by order / picker / lorry / priority / bay, top performers; score = 0.6·max(0, 100 − 2·avgMin) + 0.4·min(50, 2·orders + 0.3·lines) (tenant-tunable) | `wms/picking-time-monitor.js` |
| WM-54 | **Picker / driver PWA** (new): my picks, scan to confirm, exceptions, location pings, offline queue | replaces the LAN mobile listener (port 8766) |
| WM-55 | Route tracker: live picker routes on a warehouse zone map | `wms/tracker.html` |
| WM-56 | Daily history / task mining: timeline, journey replay, repetitive work, friction, spoken / typed feedback (never keystrokes or credentials) | `wms/wms-activity.js`, `daily-history.js`, `wms_activity_log` |

## 1.6 Inventory (read) and planned receiving
| ID | Requirement | Legacy pointers |
|---|---|---|
| WM-60 | On-hand query (org, subinventory, item, lots), organizations, subinventories, lot search, item lots | `Inventory/onhand.html` |
| WM-61 | Transactions: completed transactions, transaction types, stock movement report, SO details, reservations | `Inventory/transactions.html` |
| WM-62 | **Receiving** (designed, not built): PO receiving, duty-free receiving, customer / van / store returns, lots, locator suggestion | `wms/solution-receiving-goods.html` |
| WM-63 | **Locators / putaway / guided picking** (designed, not built): Fusion locator control vs WMS locator layer | `wms/solution-warehouse-structure.html` |

## 1.7 Product changes vs legacy
- Org code, BU / inventory org ids, order-type strings, line statuses, cancel reason, BOGO map, document report paths → tenant config.
- ORDS endpoints → typed Nexora APIs; PL/SQL from the APEX export re-implemented or kept as DB packages behind them.
- Login password in URL, plain-text passwords in tables, client-built SQL → removed.
- Browser-tab agents and timers → server workflows; LAN mobile listener → PWA + push.
