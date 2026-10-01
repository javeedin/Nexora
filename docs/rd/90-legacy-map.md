# 90. Legacy map — where the logic lives on `legacy/v12`

The `legacy/v12` branch is an exact copy (full history, one hard-coded password scrubbed) of the system this product
grows from: **Gray's WMS v12** — a .NET 8 WinForms + WebView2 desktop app with ~40 HTML/JS pages, an Oracle APEX / ORDS
backend and Oracle Fusion integrations. It is **read-only reference**: never merge it into `main`, never copy code
without re-designing it for tenancy, security and the target stack.

How to read it: `git show legacy/v12:<path>` or `git worktree add ../legacy legacy/v12`.
Its own `CLAUDE.md` (root of the branch) is the most complete technical description — read it first.

## How the legacy app works (one paragraph)
`Form1.cs` hosts WebView2; pages call C# through `window.chrome.webview.postMessage({action, requestId, …})`; the big
`switch` in `Form1.cs` (~L1700–2260) dispatches actions (partial classes `classes/Form1_*Handlers.cs` per module);
C# proxies HTTP to APEX ORDS (`executeGet/Post`) and Oracle Fusion (`executeOracleFusion*`, BIP SOAP) to avoid CORS
and keep credentials out of pages. Data lives in APEX tables (`apex_sql/NN_*.sql`, run in order; many tables are
auto-created by pages through the guarded `ai/executewrite` gateway) and local files under `C:\fusion\`.

## Module → legacy files
| Nexora module | Legacy entry points | Pure / tested logic worth porting |
|---|---|---|
| M0 Platform | `Form1.cs`, `classes/AiControl.cs`, `Form1_AdminHandlers.cs`, `Home/`, `login.html`, `classes/FusionCredentialsService.cs` | `engine/FusionModel/Licensing/Licensing.cs` (ECDSA licences) |
| M1 WMS | `wms/app.js`, `wms/*.js`, `wms/index.html`, `classes/PrintJobManager.cs`, `PrinterService.cs`, `FusionPdfDownloader.cs`, `FusionReportRunner.cs`, `MobileNotificationListener.cs`, `Inventory/` | picking score formula; status rules in `shipping-agent.js` |
| M2 SQL | `fusionsql/`, `classes/FusionSqlService.cs`, `FusionSqlExtras.cs`, `FusionSqlAgent.cs`, `Form1_FusionSqlHandlers.cs`, `docs/Fusion_SQL_Technical_RD.md` | `fusionsql/watch-engine.js`, `knowledge-engine.js` (pure, node-tested) |
| M3 Load | `dataload/`, `classes/Form1_DataLoadHandlers.cs` | `dataload/fbdi-engine.js`, `fbdi-rules.js`, generated `fbdi-catalog.js` / `fbdi-specs.js` / `specs/*` (+ generator scripts) |
| M4 Orders | `om/`, `classes/Form1_OrderMgmtHandlers.cs`, `aianalysis/order-entry.js` | `om/om-engine.js` (pricing, discounts, checks, Fusion payload — node-tested) |
| M5 SCM Workbench | `fscm/` (`core.js` = FX engine) | — |
| M6 Pipelines | `pipeline-server/` (Python, pytest), `fusionsql/pipelines.js`, `pipeline-setup.js` | whole `pipeline_server` package (connectors, runner, scheduler) |
| M7 Analytics | `engine/FusionModel*` (C#, xUnit), `fusionmodel/`, `powerbi/`, `classes/Form1_ModelHandlers.cs`, `PowerBiService.cs` | semantic compiler, packs, dashboards JSON schema |
| M8 Agents | `aianalysis/`, `classes/ClaudeCliService.cs` (prompt + action loop), `Form1_AiGuard.cs`, `AiControl.cs`, `formsdesigner/`, `agentflow/` | approval fingerprinting; policy resolution order |
| M9 AI Hub | `ai-hub/` (Python, pytest), `aihub/`, `classes/Form1_AiHubHandlers.cs` | providers, gateway, LangGraph Pipeline Doctor |
| M10 Fiscal | `classes/MRAProcessor.cs`, `MRAModels.cs`, `wms/mra-*.js`, `MRA_IMPLEMENTATION_GUIDE.md` | — |

## Not in the repository (must be exported)
- **APEX workspace `WKSP_GRAYSAPP`**: ORDS modules TRIPMANAGEMENT, WAREHOUSEMANAGEMENT (incl. `ai/*`), ARMODULE,
  ORDERCRATION, REERP and their PL/SQL. Most trip / pick / S2V / agent / print endpoint logic is there only.
  Export with SQLcl (`apex export -workspaceid …`, `ords export`) or SQL Developer and commit to
  `legacy/apex-export/` on the `legacy/v12` branch (or a `legacy/apex` branch).
- **BI Publisher reports** under `/Custom/OQ/`, `/Custom/DEXPRESS/`, `/Custom/GraysWMS/QueryRunner.xdo` — export the
  catalog objects (.xdoz) for the document and MRA reports.

## Customer-specific values found in legacy (become tenant config / packs)
ORDS base `…graysprod.adb.eu-frankfurt-1.oraclecloudapps.com/ords/WKSP_GRAYSAPP/…` (in 20+ files) and `graystest`
login host; Fusion pods `efmh.fa.em3.oraclecloud.com` / `efmh-test…`, REST `11.13.18.05`; BU `300000003234003`,
inventory org `300000003277749`, org code `GIC`, warehouse `SHOPS`, source system `OPS`, currency `MUR`, payment terms
`GRIMMEDIATE`; flexfields `HeaderEffBGRAYSprivateVO` / `FulfillLineEffBGRAYSprivateVO` (context GRAYS); cancel reason
"OUT OF STOCK"; order types STORE TO VAN / VAN TO STORE / S2V / V2S / PRO-FORMA INVOICE; tax codes GROT1.4 15 %,
GRTESTOUT17 17 %; BOGO table; MRA seller block, gateway, tax mapping; report paths; admin user `JAVEED`;
`C:\fusion\…` paths; ports 8765 (RAG), 8766 (mobile listener), 8000 / 8100 (pipeline server / AI hub).

## Security debts in legacy (do not repeat)
Login password in the URL; plain-text Fusion passwords in `wms_printer_config`, `WMS_AI_SETTINGS`, `RR_SYNC_JOBS_HEADER`;
one shared Fusion integration user for every user and pod; client-built SQL through `ai/executequery|executewrite`
(guarded, but no real authentication beyond an `appUser` field); MRA gateway over plain HTTP; API keys in browser
localStorage (Internet Search, BI dashboard, RAG); a Fusion password that was committed to git history (scrubbed on
`legacy/v12`; **rotate it**).
