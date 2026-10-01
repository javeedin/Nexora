import conventional from '@commitlint/config-conventional';
import createPreset from 'conventional-changelog-conventionalcommits';

/**
 * Conventional Commits (CLAUDE.md rule 9) plus a Nexora rule: feat / fix / perf / refactor commits must reference
 * a requirement ID (`WM-11`, `SQL-08`, `PC-01` …) or a plan task ID (`P0-T07`, `L-T03`).
 */
export const ID_PATTERN = /\b(?:[A-Z]{2,4}-\d{1,3}|(?:P\d|L)-T\d{2})\b/;
const TYPES_NEEDING_ID = new Set(['feat', 'fix', 'perf', 'refactor']);

export const referencesId = (parsed) => {
  if (!TYPES_NEEDING_ID.has(parsed.type)) return [true];
  const text = [parsed.header, parsed.body, parsed.footer].filter(Boolean).join('\n');
  return [ID_PATTERN.test(text), `${parsed.type} commits must reference a requirement or task ID (e.g. WM-11, P0-T07)`];
};

// Build the parser preset here instead of naming it: commitlint would resolve a name from the caller's cwd, which
// pnpm's strict layout hides.
const { parser } = await createPreset();

export default {
  parserPreset: { parserOpts: parser },
  plugins: [{ rules: { 'nexora/references-id': referencesId } }],
  rules: {
    ...conventional.rules,
    'header-max-length': [2, 'always', 100],
    'body-max-line-length': [1, 'always', 120],
    'nexora/references-id': [2, 'always'],
  },
};
