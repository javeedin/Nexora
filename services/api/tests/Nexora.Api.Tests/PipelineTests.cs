using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nexora.Api.Tests;

public sealed class PipelineTests(TestApi api) : IClassFixture<TestApi>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoints_answer(string path)
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Platform_info_is_served_under_api_v1()
    {
        using var client = api.CreateClient();
        var info = await client.GetFromJsonAsync<JsonElement>("/api/v1/platform/info", Ct);
        Assert.Equal("nexora-api", info.GetProperty("service").GetString());
    }

    [Fact]
    public async Task Errors_are_problem_details_with_trace_id()
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync(new Uri("/api/v1/does-not-exist", UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Invalid_request_is_rejected_with_field_errors_before_the_handler_runs()
    {
        using var client = api.CreateClient();
        var before = api.Module.Executions;
        var response = await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "", quantity = 0 }, NewKey()), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Quantity", out _));
        Assert.Equal(before, api.Module.Executions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("short")]
    [InlineData("has spaces in it")]
    public async Task Write_without_valid_idempotency_key_is_rejected(string? key)
    {
        using var client = api.CreateClient();
        var response = await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, key), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Idempotency-Key", await response.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Same_key_and_request_replays_the_first_response_without_executing_again()
    {
        using var client = api.CreateClient();
        var key = NewKey();
        var body = new { name = "pallet", quantity = 3 };
        var before = api.Module.Executions;

        var first = await client.SendAsync(TestApi.Post("/api/v1/test/things", body, key), Ct);
        var second = await client.SendAsync(TestApi.Post("/api/v1/test/things", body, key), Ct);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(await first.Content.ReadAsStringAsync(Ct), await second.Content.ReadAsStringAsync(Ct));
        Assert.False(first.Headers.Contains("Idempotent-Replayed"));
        Assert.Equal("true", second.Headers.GetValues("Idempotent-Replayed").Single());
        Assert.Equal(before + 1, api.Module.Executions);
    }

    [Fact]
    public async Task Same_key_with_a_different_request_is_422()
    {
        using var client = api.CreateClient();
        var key = NewKey();
        await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, key), Ct);
        var reuse = await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 2 }, key), Ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reuse.StatusCode);
    }

    [Fact]
    public async Task Server_errors_are_not_stored_so_a_retry_executes_again()
    {
        using var client = api.CreateClient();
        var key = NewKey();
        var before = api.Module.Executions;
        var first = await client.SendAsync(TestApi.Post("/api/v1/test/fail", null, key), Ct);
        var retry = await client.SendAsync(TestApi.Post("/api/v1/test/fail", null, key), Ct);
        Assert.Equal(HttpStatusCode.InternalServerError, first.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, retry.StatusCode);
        Assert.Equal(before + 2, api.Module.Executions);
    }

    [Fact]
    public async Task Concurrent_duplicate_while_first_is_running_is_409()
    {
        using var client = api.CreateClient();
        var key = NewKey();
        api.Module.SlowGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = api.Module.Executions;

        var first = client.SendAsync(TestApi.Post("/api/v1/test/slow", null, key), Ct);
        while (api.Module.Executions == started)
        {
            await Task.Delay(10, Ct);
        }

        var duplicate = await client.SendAsync(TestApi.Post("/api/v1/test/slow", null, key), Ct);
        api.Module.SlowGate.SetResult();

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await first).StatusCode);
    }

    [Fact]
    public async Task Keys_are_scoped_per_route()
    {
        using var client = api.CreateClient();
        var key = NewKey();
        var a = await client.SendAsync(TestApi.Post("/api/v1/test/things", new { name = "a", quantity = 1 }, key), Ct);
        var b = await client.SendAsync(TestApi.Post("/api/v1/test/fail", null, key), Ct);
        Assert.Equal(HttpStatusCode.Created, a.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, b.StatusCode);
    }

    private static string NewKey() => Guid.NewGuid().ToString();
}
