import assert from 'node:assert/strict';
import { test } from 'node:test';

import { buildEnv, generateSecret, parseEnv } from './dev-env.mjs';

const example = '# comment\nA=generate\nB=fixed\nC=generate\n';

test('generates every `generate` value and keeps fixed ones', () => {
  const { content, generated } = buildEnv(example);
  const env = parseEnv(content);
  assert.deepEqual(generated, ['A', 'C']);
  assert.equal(env.get('B'), 'fixed');
  assert.match(env.get('A') ?? '', /^[A-Za-z][A-Za-z0-9]{23}$/);
  assert.notEqual(env.get('A'), env.get('C'));
});

test('keeps existing secrets so running twice is a no-op', () => {
  const first = buildEnv(example).content;
  const second = buildEnv(example, first);
  assert.equal(second.content, first);
  assert.deepEqual(second.generated, []);
});

test('adds keys that are new in the template', () => {
  const { content, generated } = buildEnv(`${example}D=generate\n`, buildEnv(example).content);
  assert.deepEqual(generated, ['D']);
  assert.ok(parseEnv(content).has('D'));
});

test('secrets are letters and digits only', () => {
  for (let i = 0; i < 200; i += 1) assert.match(generateSecret(), /^[A-Za-z][A-Za-z0-9]+$/);
});
