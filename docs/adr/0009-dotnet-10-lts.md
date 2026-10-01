# ADR 0009 — .NET 10 (LTS) instead of .NET 9
Status: accepted (2026-10-01). Supersedes the runtime version in ADR 0003; the modular-monolith decision stands.
Context: ADR 0003 named .NET 9, a standard-term release whose support ends on 2026-11-10 — before Nexora's first
production release. .NET 10 is the long-term-support release (supported to November 2028) and the newest SDK.
Decision: every .NET project (core API, Fusion connector, workflows, Nexora Edge) targets `net10.0`; SDK pinned by
`global.json` to the 10.0 line (`rollForward: latestFeature`, never a new major silently); container images use the
`10.0` SDK and chiseled ASP.NET runtime images, pinned by digest. The target framework is set once
(`NexoraTargetFramework` in `Directory.Build.props`).
Consequences: one supported runtime until late 2028; the move to .NET 12 (next LTS) gets its own ADR. Libraries must
support .NET 10 (all chosen ones do). C# 14 is available.
