// Probes every service of the local stack and prints where to find it. Exit code 1 if anything is down.
import { readFileSync } from 'node:fs';
import net from 'node:net';
import { fileURLToPath } from 'node:url';

import { parseEnv } from './dev-env.mjs';

const env = parseEnv(readFileSync(fileURLToPath(new URL('../infra/compose/.env', import.meta.url)), 'utf8'));

const http = (url) => async () => (await fetch(url, { signal: AbortSignal.timeout(3000) })).status < 500;
const tcp = (port) => () =>
  new Promise((resolve) => {
    const socket = net.connect({ host: '127.0.0.1', port, timeout: 3000 }, () => {
      socket.end();
      resolve(true);
    });
    socket.on('error', () => resolve(false)).on('timeout', () => resolve(false));
  });

const services = [
  ['Oracle 23ai', 'localhost:1521/FREEPDB1  migrator nexora_migrator / ORACLE_MIGRATOR_PASSWORD', tcp(1521)],
  [
    'Keycloak',
    'http://localhost:8180  realm nexora · admin / KEYCLOAK_ADMIN_PASSWORD',
    http('http://127.0.0.1:8180/realms/nexora'),
  ],
  ['Temporal UI', 'http://localhost:8233  (gRPC localhost:7233, namespace nexora)', http('http://127.0.0.1:8233')],
  ['Redpanda (Kafka)', 'localhost:19092  · schema registry http://localhost:18081', tcp(19092)],
  ['Redpanda Console', 'http://localhost:8088', http('http://127.0.0.1:8088')],
  ['Redis', 'localhost:6379  password REDIS_PASSWORD', tcp(6379)],
  ['S3 (SeaweedFS)', 'http://localhost:8333  bucket nexora-files · S3_ACCESS_KEY / S3_SECRET_KEY', tcp(8333)],
  ['Vault', 'http://localhost:8200  token VAULT_ROOT_TOKEN', http('http://127.0.0.1:8200/v1/sys/health')],
  [
    'Grafana (LGTM)',
    'http://localhost:3000  · OTLP localhost:4317 (gRPC) / :4318 (HTTP)',
    http('http://127.0.0.1:3000/api/health'),
  ],
  ['Mailpit', 'http://localhost:8025  · SMTP localhost:1025', http('http://127.0.0.1:8025/readyz')],
];

const results = await Promise.all(
  services.map(async ([name, where, probe]) => [name, where, await probe().catch(() => false)]),
);
for (const [name, where, ok] of results) process.stdout.write(`${ok ? 'up  ' : 'DOWN'}  ${name.padEnd(17)} ${where}\n`);
process.stdout.write(
  `\nDemo users (organisation = tenant): admin@acme.test, user@globex.test, vendor@nexora.test` +
    ` — password NEXORA_DEV_USER_PASSWORD in infra/compose/.env${env.has('NEXORA_DEV_USER_PASSWORD') ? '' : ' (missing!)'}\n`,
);
process.exitCode = results.every(([, , ok]) => ok) ? 0 : 1;
