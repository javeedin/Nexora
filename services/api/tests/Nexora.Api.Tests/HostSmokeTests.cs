using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Nexora.Api.Tests;

public sealed class HostSmokeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Host_starts_and_answers()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
