using Nexora.BuildingBlocks.Idempotency;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Nexora.Api.Tests;

/// <summary>Same contract for both stores; Redis runs in a real container (Testcontainers).</summary>
public abstract class IdempotencyStoreContract
{
    protected abstract IIdempotencyStore Store { get; }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly TimeSpan Lock = TimeSpan.FromMinutes(1);
    private static readonly StoredResponse Response = new(201, "application/json", "{\"id\":1}"u8.ToArray());

    [Fact]
    public async Task Lifecycle_claim_inprogress_mismatch_complete_replay()
    {
        var key = $"t:{Guid.NewGuid()}";
        Assert.Equal(ClaimOutcome.Claimed, (await Store.ClaimAsync(key, "fp", Lock, Ct)).Outcome);
        Assert.Equal(ClaimOutcome.InProgress, (await Store.ClaimAsync(key, "fp", Lock, Ct)).Outcome);
        Assert.Equal(ClaimOutcome.Mismatch, (await Store.ClaimAsync(key, "other", Lock, Ct)).Outcome);

        await Store.CompleteAsync(key, "fp", Response, TimeSpan.FromHours(1), Ct);
        var replay = await Store.ClaimAsync(key, "fp", Lock, Ct);
        Assert.Equal(ClaimOutcome.Completed, replay.Outcome);
        Assert.Equal(201, replay.Response!.StatusCode);
        Assert.Equal(Response.Body, replay.Response.Body);
    }

    [Fact]
    public async Task Release_allows_a_new_claim()
    {
        var key = $"t:{Guid.NewGuid()}";
        await Store.ClaimAsync(key, "fp", Lock, Ct);
        await Store.ReleaseAsync(key, Ct);
        Assert.Equal(ClaimOutcome.Claimed, (await Store.ClaimAsync(key, "fp", Lock, Ct)).Outcome);
    }

    [Fact]
    public async Task Exactly_one_of_many_concurrent_claims_wins()
    {
        var key = $"t:{Guid.NewGuid()}";
        var results = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => Store.ClaimAsync(key, "fp", Lock, Ct), Ct)));
        Assert.Single(results, r => r.Outcome == ClaimOutcome.Claimed);
        Assert.All(results.Where(r => r.Outcome != ClaimOutcome.Claimed), r => Assert.Equal(ClaimOutcome.InProgress, r.Outcome));
    }
}

public sealed class InMemoryIdempotencyStoreTests : IdempotencyStoreContract
{
    protected override IIdempotencyStore Store { get; } = new InMemoryIdempotencyStore();

    [Fact]
    public async Task Expired_claim_can_be_taken_over()
    {
        var time = new ManualTime();
        var store = new InMemoryIdempotencyStore(time);
        var ct = TestContext.Current.CancellationToken;
        await store.ClaimAsync("k", "fp", TimeSpan.FromSeconds(30), ct);
        time.Advance(TimeSpan.FromSeconds(31));
        Assert.Equal(ClaimOutcome.Claimed, (await store.ClaimAsync("k", "fp", TimeSpan.FromSeconds(30), ct)).Outcome);
    }

    private sealed class ManualTime : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UnixEpoch;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}

public sealed class RedisIdempotencyStoreTests : IdempotencyStoreContract, IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:8.8.3-alpine").Build();
    private ConnectionMultiplexer? _connection;

    protected override IIdempotencyStore Store => new RedisIdempotencyStore(_connection!);

    public async ValueTask InitializeAsync()
    {
        await _redis.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _redis.DisposeAsync();
    }
}
