// End-to-end smoke test of the local stack: one real round trip per service (not just "port open").
// Usage: make smoke   (or pnpm stack:smoke). Exit code 1 if any check fails.
import { execFileSync } from 'node:child_process';
import { randomBytes } from 'node:crypto';
import { readFileSync } from 'node:fs';
import net from 'node:net';
import { fileURLToPath } from 'node:url';

import { parseEnv } from './dev-env.mjs';

const root = fileURLToPath(new URL('..', import.meta.url));
const env = parseEnv(readFileSync(`${root}infra/compose/.env`, 'utf8'));
const need = (key) =>
  env.get(key) ??
  (() => {
    throw new Error(`${key} missing in infra/compose/.env`);
  })();

const compose = (...args) =>
  execFileSync('docker', ['compose', '-f', 'infra/compose/compose.yaml', '--env-file', 'infra/compose/.env', ...args], {
    cwd: root,
    encoding: 'utf8',
    stdio: ['pipe', 'pipe', 'pipe'],
  });
const exec = (service, ...cmd) => compose('exec', '-T', service, ...cmd);
const json = async (url, init) => {
  const res = await fetch(url, { ...init, signal: AbortSignal.timeout(10_000) });
  return {
    status: res.status,
    body: res.headers.get('content-type')?.includes('json') ? await res.json() : await res.text(),
  };
};
const assert = (cond, msg) => {
  if (!cond) throw new Error(msg);
};
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const id = randomBytes(4).toString('hex');

const checks = {
  async 'Keycloak: token carries sub, audience, tenant id + roles'() {
    const { body } = await json('http://127.0.0.1:8180/realms/nexora/protocol/openid-connect/token', {
      method: 'POST',
      body: new URLSearchParams({
        grant_type: 'password',
        client_id: 'nexora-dev-cli',
        username: 'admin@acme.test',
        password: need('NEXORA_DEV_USER_PASSWORD'),
        scope: 'openid',
      }),
    });
    assert(body.access_token, `no token: ${JSON.stringify(body)}`);
    const claims = JSON.parse(Buffer.from(body.access_token.split('.')[1], 'base64url').toString());
    const acme = claims.tenants?.acme?.id;
    assert(/^[0-9a-f-]{36}$/.test(acme ?? ''), `tenants claim: ${JSON.stringify(claims.tenants)}`);
    assert(claims.sub && claims.aud === 'nexora-api', `sub / aud: ${claims.sub} / ${claims.aud}`);
    assert(claims.realm_access?.roles?.includes('tenant-admin'), 'missing tenant-admin role');
    return `tenant acme=${acme.slice(0, 8)}…, aud=${claims.aud}`;
  },

  async 'Vault: per-tenant encrypt/decrypt under nexora-api policy; key export denied'() {
    const root = { 'X-Vault-Token': need('VAULT_ROOT_TOKEN') };
    const created = await json('http://127.0.0.1:8200/v1/auth/token/create', {
      method: 'POST',
      headers: root,
      body: JSON.stringify({ policies: ['nexora-api'], ttl: '5m', no_default_policy: true }),
    });
    const api = { 'X-Vault-Token': created.body.auth.client_token };
    const plaintext = Buffer.from(`smoke-${id}`).toString('base64');
    const enc = await json('http://127.0.0.1:8200/v1/transit/encrypt/tenant-acme', {
      method: 'POST',
      headers: api,
      body: JSON.stringify({ plaintext }),
    });
    assert(enc.status === 200, `encrypt ${enc.status}`);
    const dec = await json('http://127.0.0.1:8200/v1/transit/decrypt/tenant-acme', {
      method: 'POST',
      headers: api,
      body: JSON.stringify({ ciphertext: enc.body.data.ciphertext }),
    });
    assert(dec.body.data?.plaintext === plaintext, 'decrypt mismatch');
    const exported = await json('http://127.0.0.1:8200/v1/transit/export/encryption-key/tenant-acme', { headers: api });
    assert(exported.status === 403, `key export should be 403, got ${exported.status}`);
    return 'round trip ok, export 403';
  },

  async 'Oracle: migrator can query FREEPDB1'() {
    const out = exec(
      'oracle',
      'bash',
      '-c',
      `echo "select 'nexora-ok' as r from dual;" | sqlplus -s "nexora_migrator/$APP_USER_PASSWORD@localhost/FREEPDB1"`,
    );
    assert(out.includes('nexora-ok'), out.trim());
    return 'select from dual ok';
  },

  async 'Redpanda: produce + consume'() {
    const topic = `nexora.smoke.${id}`;
    exec('redpanda', 'rpk', 'topic', 'create', topic);
    execFileSync(
      'docker',
      [
        'compose',
        '-f',
        'infra/compose/compose.yaml',
        '--env-file',
        'infra/compose/.env',
        'exec',
        '-T',
        'redpanda',
        'rpk',
        'topic',
        'produce',
        topic,
      ],
      { cwd: root, input: `hello-${id}\n` },
    );
    const out = exec('redpanda', 'rpk', 'topic', 'consume', topic, '--num', '1', '--format', '%v\n');
    exec('redpanda', 'rpk', 'topic', 'delete', topic);
    assert(out.includes(`hello-${id}`), out);
    return topic;
  },

  async 'Redis: auth + set/get'() {
    const out = exec(
      'redis',
      'sh',
      '-c',
      `redis-cli -a "$REDIS_PASSWORD" --no-auth-warning SET smoke:${id} ok EX 60 >/dev/null && redis-cli -a "$REDIS_PASSWORD" --no-auth-warning GET smoke:${id}`,
    );
    assert(out.trim() === 'ok', out);
    return 'ok';
  },

  async 'S3: signed put + get + delete in nexora-files'() {
    const out = execFileSync(
      'uv',
      [
        'run',
        '--quiet',
        '--no-project',
        '--with',
        'boto3==1.40.*',
        'python',
        '-c',
        `
import boto3, sys
s3 = boto3.client("s3", endpoint_url="http://127.0.0.1:8333", region_name="us-east-1",
                  aws_access_key_id=sys.argv[1], aws_secret_access_key=sys.argv[2])
key = "tenants/acme/smoke/${id}.txt"
s3.put_object(Bucket="nexora-files", Key=key, Body=b"hello")
body = s3.get_object(Bucket="nexora-files", Key=key)["Body"].read()
s3.delete_object(Bucket="nexora-files", Key=key)
print(body.decode())
`,
        need('S3_ACCESS_KEY'),
        need('S3_SECRET_KEY'),
      ],
      { cwd: root, encoding: 'utf8' },
    );
    assert(out.trim() === 'hello', out);
    return 'tenants/acme/... round trip';
  },

  async 'Temporal: namespace nexora registered'() {
    const out = exec(
      'temporal',
      'temporal',
      'operator',
      'namespace',
      'describe',
      '--namespace',
      'nexora',
      '--address',
      '127.0.0.1:7233',
    );
    assert(/nexora/.test(out) && /Registered/i.test(out), out);
    return 'Registered';
  },

  async 'Mailpit: SMTP message arrives'() {
    const subject = `smoke ${id}`;
    await new Promise((resolve, reject) => {
      const lines = [
        'EHLO nexora.test',
        'MAIL FROM:<noreply@nexora.test>',
        'RCPT TO:<dev@nexora.test>',
        'DATA',
        `Subject: ${subject}\r\n\r\nhello\r\n.`,
        'QUIT',
      ];
      const socket = net.connect(1025, '127.0.0.1');
      socket.setEncoding('utf8');
      socket.on('data', () => {
        const line = lines.shift();
        if (line) socket.write(`${line}\r\n`);
        else socket.end();
      });
      socket.on('end', resolve).on('error', reject);
    });
    const { body } = await json(
      `http://127.0.0.1:8025/api/v1/search?query=${encodeURIComponent(`subject:"${subject}"`)}`,
    );
    assert(body.messages_count >= 1, JSON.stringify(body).slice(0, 200));
    return subject;
  },

  async 'OTel → Tempo: trace ingested and queryable'() {
    const traceId = randomBytes(16).toString('hex');
    const now = BigInt(Date.now()) * 1_000_000n;
    const res = await fetch('http://127.0.0.1:4318/v1/traces', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({
        resourceSpans: [
          {
            resource: {
              attributes: [
                { key: 'service.name', value: { stringValue: 'nexora-smoke' } },
                { key: 'tenant_id', value: { stringValue: 'acme' } },
              ],
            },
            scopeSpans: [
              {
                spans: [
                  {
                    traceId,
                    spanId: randomBytes(8).toString('hex'),
                    name: 'smoke',
                    kind: 1,
                    startTimeUnixNano: String(now),
                    endTimeUnixNano: String(now + 1_000_000n),
                  },
                ],
              },
            ],
          },
        ],
      }),
    });
    assert(res.ok, `OTLP ${res.status}`);
    for (let i = 0; i < 20; i += 1) {
      await sleep(1500);
      try {
        const out = exec(
          'otel-lgtm',
          'curl',
          '-s',
          '-o',
          '/dev/null',
          '-w',
          '%{http_code}',
          `http://127.0.0.1:3200/api/traces/${traceId}`,
        );
        if (out.trim() === '200') return `trace ${traceId.slice(0, 8)}…`;
      } catch {
        /* not yet */
      }
    }
    throw new Error('trace not found in Tempo after 30 s');
  },
};

let failed = 0;
for (const [name, check] of Object.entries(checks)) {
  try {
    process.stdout.write(`ok    ${name} — ${await check()}\n`);
  } catch (error) {
    failed += 1;
    process.stdout.write(`FAIL  ${name} — ${error instanceof Error ? error.message.split('\n')[0] : error}\n`);
  }
}
process.stdout.write(failed ? `\n${failed} check(s) failed\n` : '\nall checks passed\n');
process.exitCode = failed ? 1 : 0;
