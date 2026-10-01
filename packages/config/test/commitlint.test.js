import assert from 'node:assert/strict';
import { test } from 'node:test';

import lint from '@commitlint/lint';
import load from '@commitlint/load';

import config from '../commitlint.js';

const check = async (message) => {
  const { rules, parserPreset, plugins } = await load(config);
  return lint(message, rules, { parserOpts: parserPreset?.parserOpts, plugins });
};

test('feat with a requirement ID passes', async () => {
  assert.equal((await check('feat(wms): ship confirm workflow (WM-11)')).valid, true);
});

test('feat with a plan task ID in the body passes', async () => {
  assert.equal((await check('feat(platform): tenancy core\n\nImplements P0-T07.')).valid, true);
});

test('later-phase task IDs count', async () => {
  assert.equal((await check('feat(ml): demand forecast (L-T01)')).valid, true);
});

test('feat without any ID is rejected', async () => {
  const result = await check('feat(wms): ship confirm workflow');
  assert.equal(result.valid, false);
  assert.ok(result.errors.some((e) => e.name === 'nexora/references-id'));
});

test('chore and docs need no ID', async () => {
  assert.equal((await check('chore: bump deps')).valid, true);
  assert.equal((await check('docs(plan): update progress')).valid, true);
});

test('non-conventional message is rejected', async () => {
  assert.equal((await check('Added stuff')).valid, false);
});
