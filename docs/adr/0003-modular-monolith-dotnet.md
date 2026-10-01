# ADR 0003 — .NET modular monolith first
Status: accepted. Runtime version superseded by ADR 0009 (.NET 10 LTS; originally .NET 9).
Decision: one deployable core service with strict module boundaries (own folder, own schema, public contracts only),
split into services only when scale or team size needs it. Fusion connector, workflows and the AI platform are separate
services from the start.
Consequences: simpler operations early; boundaries enforced by architecture tests (NetArchTest).
