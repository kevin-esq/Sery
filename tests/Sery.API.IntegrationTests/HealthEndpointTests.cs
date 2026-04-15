using Microsoft.AspNetCore.Mvc.Testing;

namespace Sery.API.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();
    }
}
