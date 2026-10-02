# Local dev stack (P0-T03). Same commands without make: pnpm stack:up | stack:down | stack:reset | stack:check | stack:smoke
COMPOSE := docker compose -f infra/compose/compose.yaml --env-file infra/compose/.env

.PHONY: up down reset logs ps check smoke env

up: env ## start everything, wait until healthy, seed, print URLs
	$(COMPOSE) up -d --wait --wait-timeout 600
	$(COMPOSE) --profile seed run --rm seed
	$(COMPOSE) --profile seed run --rm seed-s3
	node scripts/dev-check.mjs

down: ## stop (data kept)
	$(COMPOSE) down

reset: ## stop and delete all data volumes
	$(COMPOSE) down -v

logs: ## follow logs (make logs s=keycloak)
	$(COMPOSE) logs -f $(s)

ps:
	$(COMPOSE) ps

check: ## probe every service
	node scripts/dev-check.mjs

smoke: ## end-to-end round trip through every service
	node scripts/dev-smoke.mjs

env:
	node scripts/dev-env.mjs
