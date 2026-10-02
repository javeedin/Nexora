using System.Net;

namespace Nexora.Api.Tests;

public sealed class RateLimitTests
{
    [Fact]
    public async Task Requests_over_the_limit_get_429_problem_with_retry_after_but_health_is_exempt()
    {
        await using var api = new TestApi { Settings = new() { ["RateLimiting:PermitsPerMinute"] = "3" } };
        using var client = api.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(new Uri("/api/v1/platform/info", UriKind.Relative), ct)).StatusCode);
        }

        var limited = await client.GetAsync(new Uri("/api/v1/platform/info", UriKind.Relative), ct);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal("application/problem+json", limited.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(limited.Headers.RetryAfter);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(new Uri("/health/live", UriKind.Relative), ct)).StatusCode);
    }
}
