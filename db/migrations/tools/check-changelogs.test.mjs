import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';

import { checkChangelog, checkTree } from './check-changelogs.mjs';

const table = (columns, remarks) => `
databaseChangeLog:
  - changeSet:
      id: 0001-thing
      author: nexora
      changes:
        - createTable:
            tableName: thing
            ${remarks ? `remarks: "${remarks}"` : ''}
            columns:
${columns.map((c) => `              - column: ${c}`).join('\n')}
`;
const ok = '{ name: tenant_id, type: VARCHAR2(36), constraints: { nullable: false } }';

test('the real changelogs pass', () => {
  assert.deepEqual(checkTree(fileURLToPath(new URL('../', import.meta.url))), []);
});

test('table with NOT NULL tenant_id passes', () => {
  assert.deepEqual(checkChangelog('t', table([ok, '{ name: id, type: NUMBER }'])), []);
});

test('table without tenant_id fails (rule 1)', () => {
  assert.match(checkChangelog('t', table(['{ name: id, type: NUMBER }'])).join(), /has no tenant_id/);
});

test('nullable or wrongly typed tenant_id fails', () => {
  assert.match(checkChangelog('t', table(['{ name: tenant_id, type: VARCHAR2(36) }'])).join(), /NOT NULL/);
  assert.match(
    checkChangelog('t', table(['{ name: tenant_id, type: NUMBER, constraints: { nullable: false } }'])).join(),
    /VARCHAR2\(36\)/,
  );
});

test('exemption needs a reason', () => {
  assert.deepEqual(checkChangelog('t', table(['{ name: id, type: NUMBER }'], '[tenant-exempt: global lookup]')), []);
  assert.match(checkChangelog('t', table(['{ name: id, type: NUMBER }'], '[tenant-exempt: ]')).join(), /no tenant_id/);
});

test('raw sql without rollback fails; id and author are checked', () => {
  const problems = checkChangelog(
    't',
    `
databaseChangeLog:
  - changeSet:
      id: Bad_Id
      changes:
        - sql: { sql: "select 1 from dual" }
`,
  ).join('\n');
  assert.match(problems, /0001-kebab-case/);
  assert.match(problems, /author missing/);
  assert.match(problems, /explicit rollback/);
});

test('duplicate ids fail', () => {
  const cs = '  - changeSet: { id: 0001-a, author: x, changes: [ { tagDatabase: { tag: a } } ] }\n';
  assert.match(checkChangelog('t', `databaseChangeLog:\n${cs}${cs}`).join(), /duplicate id/);
});

test('bootstrap changeSets must be guarded by MARK_RAN', () => {
  const text = `
databaseChangeLog:
  - changeSet:
      id: 0001-schema-x
      author: nexora
      changes: [ { sql: { sql: "create user x no authentication" } } ]
      rollback: [ { sql: { sql: "drop user x cascade" } } ]
`;
  assert.match(checkChangelog('b', text, { bootstrap: true }).join(), /idempotent/);
  assert.deepEqual(checkChangelog('b', text), []);
});
