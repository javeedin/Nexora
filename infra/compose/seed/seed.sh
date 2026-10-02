#!/bin/sh
# Idempotent Vault seed for local dev (rule 3: secrets only in Vault, per-tenant keys).
#   transit/          envelope encryption, one key per tenant: tenant-<id>
#   secret/ (kv-v2)   secret references, laid out as secret/tenants/<id>/...
#   policy nexora-api least privilege for the core API
set -eu

vault secrets list -format=json | grep -q '"transit/"' || vault secrets enable transit
vault secrets list -format=json | grep -q '"secret/"' || vault secrets enable -path=secret kv-v2

for tenant in $(echo "$SEED_TENANTS" | tr "," " "); do
  vault read "transit/keys/tenant-$tenant" >/dev/null 2>&1 \
    || vault write -f "transit/keys/tenant-$tenant" type=aes256-gcm96 >/dev/null
  echo "vault: transit key tenant-$tenant"
done

vault policy write nexora-api - <<'HCL'
# Encrypt / decrypt with any tenant key; never export or delete keys.
path "transit/encrypt/tenant-*" { capabilities = ["update"] }
path "transit/decrypt/tenant-*" { capabilities = ["update"] }
path "transit/datakey/plaintext/tenant-*" { capabilities = ["update"] }
# Tenant secret references (pod credentials, SMTP, ...). The API itself scopes every path by tenant_id.
path "secret/data/tenants/*" { capabilities = ["create", "read", "update"] }
path "secret/metadata/tenants/*" { capabilities = ["list", "read"] }
HCL
echo "vault: policy nexora-api"
echo "seed: done"
