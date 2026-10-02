using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Nexora.BuildingBlocks.Caching;

/// <summary>
/// Shared Redis connection, created on first use from <c>ConnectionStrings:Redis</c>. <see cref="Multiplexer"/> is
/// <c>null</c> when Redis is not configured (tests, minimal local runs) so callers can fall back.
/// </summary>
public sealed class RedisConnection(IConfiguration configuration) : IAsyncDisposable
{
    private readonly Lazy<IConnectionMultiplexer?> _multiplexer = new(() =>
    {
        var connectionString = configuration.GetConnectionString("Redis");
        return string.IsNullOrWhiteSpace(connectionString) ? null : ConnectionMultiplexer.Connect(connectionString);
    });

    /// <summary>The connection, or <c>null</c> when Redis is not configured.</summary>
    public IConnectionMultiplexer? Multiplexer => _multiplexer.Value;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_multiplexer.IsValueCreated && _multiplexer.Value is { } multiplexer)
        {
            await multiplexer.DisposeAsync();
        }
    }
}
