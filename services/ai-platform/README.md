# nexora-ai-platform

Python 3.12 service for the model gateway, agents, evals and ML (ADR 0006). Managed with [uv](https://docs.astral.sh/uv/).

```sh
uv sync            # create .venv with dev tools
uv run pytest      # tests
uv run ruff check . && uv run ruff format --check . && uv run mypy   # lint + types
```
