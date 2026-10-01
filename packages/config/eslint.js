import js from '@eslint/js';
import prettier from 'eslint-config-prettier';
import globals from 'globals';
import tseslint from 'typescript-eslint';

/** Base flat config for every Nexora TS / JS workspace. Apps extend it (React rules come with apps/web). */
export default tseslint.config(
  {
    ignores: ['**/dist/**', '**/coverage/**', '**/bin/**', '**/obj/**', '**/.venv/**', '**/.turbo/**'],
  },
  js.configs.recommended,
  ...tseslint.configs.strict,
  {
    languageOptions: { globals: { ...globals.node } },
    rules: {
      'no-console': ['error', { allow: ['warn', 'error'] }],
      eqeqeq: ['error', 'always'],
      '@typescript-eslint/consistent-type-imports': 'error',
    },
  },
  prettier,
);
