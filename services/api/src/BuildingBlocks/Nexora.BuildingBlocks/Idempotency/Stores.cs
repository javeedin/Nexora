using System.Collections.Concurrent;
using System.Text.Json;
using StackExchange.Redis;

namespace Nexora.BuildingBlocks.Idempotency;

/// <summary>Single-instance store (tests, local runs without Redis).</summary>
public sealed class InMemoryIdempotencyStore(TimeProvider? time = null) : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new();
    private readonly TimeProvider _time = time ?? TimeProvider.System;

    /// <inheritdoc />
    public Task<ClaimResult> ClaimAsync(string key, string fingerprint, TimeSpan lockFor, CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        var pending = new Entry(fingerprint, null, now + lockFor);
        while (true)
        {
            if (_entries.TryAdd(key, pending))
            {
                return Task.FromResult(new ClaimResult(ClaimOutcome.Claimed));
            }

            if (!_entries.TryGetValue(key, out var existing))
            {
                continue;
            }

            if (existing.ExpiresAt <= now)
            {
                if (_entries.TryUpdate(key, pending, existing))
                {
                    return Task.FromResult(new ClaimResult(ClaimOutcome.Claimed));
                }

                continue;
            }

            return Task.FromResult(Classify(existing.Fingerprint, existing.Response, fingerprint));
        }
    }

    /// <inheritdoc />
    public Task CompleteAsync(string key, string fingerprint, StoredResponse response, TimeSpan keepFor, CancellationToken cancellationToken)
    {
        _entries[key] = new Entry(fingerprint, response, _time.GetUtcNow() + keepFor);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ReleaseAsync(string key, CancellationToken cancellationToken)
    {
        _entries.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    internal static ClaimResult Classify(string storedFingerprint, StoredResponse? stored, string fingerprint) =>
        storedFingerprint != fingerprint ? new ClaimResult(ClaimOutcome.Mismatch)
        : stored is null ? new ClaimResult(ClaimOutcome.InProgress)
        : new ClaimResult(ClaimOutcome.Completed, stored);

    private sealed record Entry(string Fingerprint, StoredResponse? Response, DateTimeOffset ExpiresAt);
}

/// <summary>Shared store for all API instances: <c>SET NX</c> claims, JSON values, Redis expiry.</summary>
public sealed class RedisIdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
{
    /// <inheritdoc />
    public async Task<ClaimResult> ClaimAsync(string key, string fingerprint, TimeSpan lockFor, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();
        var pending = JsonSerializer.Serialize(new Entry(fingerprint, null));
        for (var attempt = 0; attempt < 3; attempt++)
        {
            if (await db.StringSetAsync(key, pending, lockFor, When.NotExists))
            {
                return new ClaimResult(ClaimOutcome.Claimed);
            }

            var current = await db.StringGetAsync(key);
            if (current.HasValue)
            {
                var entry = JsonSerializer.Deserialize<Entry>(current.ToString())!;
                return InMemoryIdempotencyStore.Classify(entry.Fingerprint, entry.Response, fingerprint);
            }
        }

        return new ClaimResult(ClaimOutcome.InProgress);
    }

    /// <inheritdoc />
    public Task CompleteAsync(string key, string fingerprint, StoredResponse response, TimeSpan keepFor, CancellationToken cancellationToken) =>
        redis.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(new Entry(fingerprint, response)), keepFor);

    /// <inheritdoc />
    public Task ReleaseAsync(string key, CancellationToken cancellationToken) => redis.GetDatabase().KeyDeleteAsync(key);

    private sealed record Entry(string Fingerprint, StoredResponse? Response);
}
