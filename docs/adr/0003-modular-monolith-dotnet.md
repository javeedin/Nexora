# ADR 0003 — .NET 9 modular monolith first
Status: accepted.
Decision: one deployable core service with strict module boundaries (own folder, own schema, public contracts only),
split into services only when scale or team size needs it. Fusion connector, workflows and the AI platform are separate
services from the start.
Consequences: simpler operations early; boundaries enforced by architecture tests (NetArchTest).
