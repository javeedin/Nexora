# M11 — Other legacy pieces: decisions

| Legacy piece | Decision | Notes |
|---|---|---|
| Root `index.html` / `app.js` (older WMS copy) | **Drop** | Superseded by `wms/` |
| Forms Designer (`formsdesigner/`) | **Keep → Agents (AG-24)** | JSON form engine is reusable for tenant-defined forms |
| Agent Flow (`agentflow/`) | **Keep → Agents (AG-25)**, compile to LangGraph | |
| RAG desktop service (`rag/`) | **Replace** with AI platform RAG (vector store in Oracle 23ai / pgvector) | |
| DLL Explorer (`dllexplorer/`, `DllInspector.cs`) | **Optional developer add-on** | Useful for partners documenting legacy integrations; not in core plans |
| Power BI (`powerbi/`) | **Keep → Analytics (AN-10)** | |
| BI Dashboard (generic REST connector, `wms/bi-dashboard.js`) | **Replace** by Analytics dashboards | |
| GL shell (`gl/`) | **Drop** (mostly "Coming soon"); requirements doc kept as idea backlog | Fusion is the GL; Nexora adds reconciliations (SQL Flows, Analytics checks) |
| Sync jobs (`sync/`, RR_SYNC_JOBS_*) | **Replace** by Pipelines | had no scheduler, plain-text passwords |
| API endpoint registry (`sync/`, REERP) | **Replace** by the action catalog (AG-23) | |
| Update / distribution / release managers (`wms/update-manager.js`, `distribution-manager.js`, release.bat, Admin › Create ZIP, installer.iss) | **Drop** | SaaS deploys through CI/CD; Edge auto-updates |
| WMS Co-Pilot (`wms/copilot.js`) | **Merge** into Agents chat | |
| Claude Code CLI mode + installer | **Drop** | server-side models only |
| Internet Search (`internetsearch/`) | **Out of scope** | personal tool, not part of the product |
| Trial gate (`Home/trial-gate.js`, WMS_AI_TRIAL) | **Replace** by subscriptions / trials (02 §2.3) | |
| `Wms-dist-*` folders and `*.zip` in the repo | **Drop** | old builds committed to git |
