# M9 — Nexora AI Hub (model gateway, evals, ML)

Purpose: one governed gateway between all Nexora AI features and model providers; proof of quality through evals.

| ID | Requirement | Legacy (`ai-hub/`, `aihub/`) |
|---|---|---|
| AH-01 | Providers: **Claude in Amazon Bedrock** (`AnthropicBedrockMantle`, `anthropic.claude-*` ids), **Claude Platform on AWS** (`AnthropicAWS`, workspace id), **Bedrock Converse** for other models (Nova, Llama, Mistral; discovered via ListFoundationModels), **Claude API**, **NVIDIA NIM** (hosted, OpenAI-compatible; discovered), offline demo. Next: Azure AI Foundry, Google Vertex AI, OCI Generative AI, self-hosted NIM, local ONNX models | `providers.py` |
| AH-02 | Credentials per tenant: SigV4 keys / bearer API key / workload identity; in Vault; never returned | keyring per PC |
| AH-03 | **Router**: per task ordered candidates with fallback on error or refusal; **data classes** (public / internal / fusion-data / personal) a provider must be allowed; monthly **budget** stops paid models; route preview | `gateway.py` |
| AH-04 | Usage ledger: per attempt task, provider, model, ok, latency, tokens, cost, user, fallback → metering + billing | `usage.py` |
| AH-05 | Playground: one prompt → up to 6 models side by side with speed / tokens / cost | `aihub/playground.js` |
| AH-06 | **Evals**: tenant's verified questions (Knowledge EXAMPLES) → each model writes SQL → both SQLs run on Fusion → results compared (order / names ignored) → scoreboard (accuracy, latency, cost per correct); eval gates for prompt / model changes in CI | `aihub/evals.js`, WMS_AIHUB_EVALS |
| AH-07 | LangChain adapter (`GatewayChatModel`) so any LangChain / LangGraph code uses the router | `lc.py` |
| AH-08 | Tracing (Langfuse / OTel) of every prompt, tool call and cost per tenant | new |
| AH-09 | **ML models** (new): forecasting (order-pad usual items with quantities, item demand), trip / picking time, anomaly detection (watchdogs, fiscal); trained per tenant (scikit-learn / LightGBM / TensorFlow), registered in MLflow, served as ONNX; scoreboard vs naive baseline before use | new |
| AH-10 | Document AI (new): scanned invoices / delivery notes → structured rows → Load (FBDI) | new |
