#!/usr/bin/env bash
# Applies bootstrap (as admin) then every module schema (as migrator by proxy).
#   migrate.sh update          apply everything
#   migrate.sh rollback-test   per schema: update → roll back EVERYTHING → update again (destructive: dev / CI only)
#   migrate.sh status          pending changesets per schema
# Env: DB_URL (jdbc:oracle:thin:@host:1521/SERVICE), DB_ADMIN_USER, DB_ADMIN_PASSWORD, DB_MIGRATOR_USER, DB_MIGRATOR_PASSWORD
set -euo pipefail
cmd="${1:-update}"
: "${DB_URL:?}" "${DB_ADMIN_USER:?}" "${DB_ADMIN_PASSWORD:?}" "${DB_MIGRATOR_USER:?}" "${DB_MIGRATOR_PASSWORD:?}"

lb() { liquibase --headless=true --show-banner=false --log-level=WARNING --url="$DB_URL" "$@"; }

echo "== bootstrap (as $DB_ADMIN_USER)"
lb --username="$DB_ADMIN_USER" --password="$DB_ADMIN_PASSWORD" --changelog-file=bootstrap/changelog.yaml \
  "$( [ "$cmd" = status ] && echo status || echo update )" -Dmigrator="${DB_MIGRATOR_USER^^}"

grep -vE '^\s*(#|$)' schemas.txt | while read -r schema; do
  echo "== $schema (as $DB_MIGRATOR_USER[$schema])"
  args=(--username="$DB_MIGRATOR_USER[$schema]" --password="$DB_MIGRATOR_PASSWORD" --changelog-file="$schema/changelog.yaml")
  case "$cmd" in
    update) lb "${args[@]}" update ;;
    status) lb "${args[@]}" status --verbose ;;
    rollback-test)
      lb "${args[@]}" update
      lb "${args[@]}" rollback-to-date --date=2000-01-01T00:00:00
      remaining=$(lb "${args[@]}" history 2>/dev/null | grep -c "$schema/changelog.yaml" || true)
      [ "$remaining" -eq 0 ] || { echo "rollback left $remaining changesets applied" >&2; exit 1; }
      echo "rolled back all changesets of $schema"
      lb "${args[@]}" update ;;
    *) echo "unknown command: $cmd" >&2; exit 2 ;;
  esac
done
echo "== done ($cmd)"
