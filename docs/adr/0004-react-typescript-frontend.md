# ADR 0004 — React + TypeScript web app, PWA for the floor
Status: accepted.
Decision: React 19 + TypeScript (Vite, TanStack Router / Query, Tailwind + shadcn/ui, AG Grid, Monaco); one design
system package; picker / driver PWA. No desktop host — local devices via Nexora Edge.
Consequences: legacy jQuery / vanilla pages and WebView2 IPC are not reused; business logic is re-implemented server-side.
