import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';

import eslint from '../eslint.js';
import prettier from '../prettier.js';

test('eslint config is a non-empty flat config', () => {
  assert.ok(Array.isArray(eslint) && eslint.length > 0);
});

test('prettier config matches .editorconfig width', () => {
  assert.equal(prettier.printWidth, 120);
});

test('tsconfig base is strict', async () => {
  const base = JSON.parse(await readFile(new URL('../tsconfig/base.json', import.meta.url), 'utf8'));
  assert.equal(base.compilerOptions.strict, true);
});
