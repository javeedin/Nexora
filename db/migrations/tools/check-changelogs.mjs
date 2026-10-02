// Enforces migration conventions on every Liquibase YAML changelog (run in CI via `pnpm lint`):
//   - changeSet id `NNNN-kebab-case`, unique per file, author set
//   - raw `sql` / `sqlFile` changes need an explicit rollback
//   - every createTable has `tenant_id VARCHAR2(36)` NOT NULL (CLAUDE.md rule 1), unless remarks start with
//     "[tenant-exempt: <reason>]" — the exemption is then visible as a table comment in the database
//   - bootstrap changeSets are idempotent: preConditions with onFail MARK_RAN
//   - every schema in schemas.txt has <schema>/changelog.yaml
import { existsSync, readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

import { parse } from 'yaml';

const ID = /^\d{4}-[a-z0-9]+(?:-[a-z0-9]+)*$/;
const EXEMPT = /^\[tenant-exempt: [^\]]{5,}\]/;

const changeSetsOf = (doc) =>
  (doc?.databaseChangeLog ?? []).flatMap((entry) => (entry.changeSet ? [entry.changeSet] : []));

/**
 * @param {string} file  path shown in messages
 * @param {string} text  YAML content
 * @param {{ bootstrap?: boolean }} [options]
 * @returns {string[]} problems
 */
export const checkChangelog = (file, text, { bootstrap = false } = {}) => {
  const problems = [];
  const seen = new Set();
  for (const cs of changeSetsOf(parse(text))) {
    const where = `${file} › ${cs.id ?? '(no id)'}`;
    if (!ID.test(String(cs.id ?? ''))) problems.push(`${where}: id must look like 0001-kebab-case`);
    if (seen.has(cs.id)) problems.push(`${where}: duplicate id`);
    seen.add(cs.id);
    if (!cs.author) problems.push(`${where}: author missing`);

    const changes = cs.changes ?? [];
    if (changes.some((c) => c.sql || c.sqlFile) && !cs.rollback) {
      problems.push(`${where}: raw sql needs an explicit rollback`);
    }
    for (const { createTable: table } of changes.filter((c) => c.createTable)) {
      if (EXEMPT.test(table.remarks ?? '')) continue;
      const tenantId = (table.columns ?? []).map((c) => c.column).find((c) => c?.name === 'tenant_id');
      if (!tenantId)
        problems.push(
          `${where}: table ${table.tableName} has no tenant_id (rule 1) — or mark it "[tenant-exempt: why]"`,
        );
      else {
        if (tenantId.constraints?.nullable !== false)
          problems.push(`${where}: ${table.tableName}.tenant_id must be NOT NULL`);
        if (String(tenantId.type).toUpperCase().replace(/\s/g, '') !== 'VARCHAR2(36)') {
          problems.push(`${where}: ${table.tableName}.tenant_id must be VARCHAR2(36)`);
        }
      }
    }
    if (bootstrap && !(cs.preConditions ?? []).some((p) => p.onFail === 'MARK_RAN')) {
      problems.push(`${where}: bootstrap changeSets must be idempotent (preConditions with onFail: MARK_RAN)`);
    }
  }
  return problems;
};

/** Checks the whole db/migrations tree; returns problems. */
export const checkTree = (dir) => {
  const read = (rel) => readFileSync(`${dir}${rel}`, 'utf8');
  const problems = checkChangelog('bootstrap/changelog.yaml', read('bootstrap/changelog.yaml'), { bootstrap: true });
  const schemas = read('schemas.txt')
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter((l) => l && !l.startsWith('#'));
  for (const schema of schemas) {
    const rel = `${schema}/changelog.yaml`;
    if (!existsSync(`${dir}${rel}`)) problems.push(`schemas.txt lists ${schema} but ${rel} is missing`);
    else problems.push(...checkChangelog(rel, read(rel)));
  }
  return problems;
};

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const problems = checkTree(fileURLToPath(new URL('../', import.meta.url)));
  for (const p of problems) process.stderr.write(`✖ ${p}\n`);
  process.stderr.write(problems.length ? `${problems.length} problem(s)\n` : 'changelogs ok\n');
  process.exitCode = problems.length ? 1 : 0;
}
