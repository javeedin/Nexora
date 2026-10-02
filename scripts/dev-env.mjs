// Creates / completes infra/compose/.env from .env.example: every `generate` value becomes a random secret.
// Idempotent — existing values are kept, so containers keep working across runs.
import { randomInt } from 'node:crypto';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const dir = fileURLToPath(new URL('../infra/compose/', import.meta.url));
const examplePath = `${dir}.env.example`;
const envPath = `${dir}.env`;

const ALPHABET = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789';

/** Letters + digits only (safe in URLs, shells and Oracle passwords); starts with a letter. */
export const generateSecret = (length = 24) => {
  let out = ALPHABET[randomInt(0, 49)] ?? 'A';
  while (out.length < length) out += ALPHABET[randomInt(0, ALPHABET.length)];
  return out;
};

/** @param {string} text */
export const parseEnv = (text) =>
  new Map(
    text
      .split(/\r?\n/)
      .filter((line) => line && !line.startsWith('#') && line.includes('='))
      .map((line) => [line.slice(0, line.indexOf('=')), line.slice(line.indexOf('=') + 1)]),
  );

/** Merges the template with an existing .env; returns the new file content and the keys it generated. */
export const buildEnv = (exampleText, existingText = '') => {
  const existing = parseEnv(existingText);
  const generated = [];
  const lines = exampleText.split(/\r?\n/).map((line) => {
    if (!line || line.startsWith('#') || !line.includes('=')) return line;
    const key = line.slice(0, line.indexOf('='));
    const template = line.slice(line.indexOf('=') + 1);
    const current = existing.get(key);
    if (current !== undefined && current !== 'generate') return `${key}=${current}`;
    if (template === 'generate') {
      generated.push(key);
      return `${key}=${generateSecret()}`;
    }
    return line;
  });
  return { content: lines.join('\n'), generated };
};

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const { content, generated } = buildEnv(
    readFileSync(examplePath, 'utf8'),
    existsSync(envPath) ? readFileSync(envPath, 'utf8') : '',
  );
  writeFileSync(envPath, content, { mode: 0o600 });
  process.stderr.write(
    generated.length ? `dev-env: generated ${generated.join(', ')}\n` : 'dev-env: infra/compose/.env up to date\n',
  );
}
