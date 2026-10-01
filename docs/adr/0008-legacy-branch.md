# ADR 0008 — Legacy code kept as a read-only branch
Status: accepted.
Decision: the full history of Gray's WMS v12 is kept on branch `legacy/v12` (one committed Fusion password replaced by
`***REMOVED***`, a committed WebView2 browser profile removed). It is reference material for business logic, never
merged into `main`. Missing server logic from the customer's APEX workspace is to be exported to `legacy/apex-export/`.
