# ADR 0006 — Python AI platform with a governed model gateway
Status: accepted.
Decision: a Python service (FastAPI) owns model access (gateway: routing, fallback, data-class policy, budgets, usage;
providers AWS-first — Claude in Amazon Bedrock, Claude Platform on AWS, Bedrock Converse — plus Claude API and NVIDIA
NIM), LangGraph agents with checkpoints, evals and ML models. Tools are exposed as MCP servers backed by Nexora APIs.
Starting point: `ai-hub/` on `legacy/v12`.
Consequences: every AI call is traced, costed and policy-checked; prompts and models change only through evals.
